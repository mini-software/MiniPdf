package minipdf

import (
	"fmt"
	"math"
	"regexp"
	"strconv"
	"strings"
	"time"
)

var xlsxSimpleNumberFormat = regexp.MustCompile(`^[#0]+(?:,[#0]+)*(?:\.[0#]+)?%?$`)

func formatPrintedXLSXValue(value string, id int, code string, date1904 bool) (string, error) {
	number, err := strconv.ParseFloat(strings.TrimSpace(value), 64)
	if err != nil || math.IsNaN(number) || math.IsInf(number, 0) {
		return "", fmt.Errorf("invalid numeric XLSX value %q", value)
	}
	if code == "" {
		code = map[int]string{1: "0", 2: "0.00", 3: "#,##0", 4: "#,##0.00", 9: "0%", 10: "0.00%", 14: "m/d/yyyy", 15: "d-mmm-yy", 16: "d-mmm", 17: "mmm-yy"}[id]
	}
	if code == "" && id == 0 {
		return strconv.FormatFloat(number, 'g', 15, 64), nil
	}
	datePattern, quoted, bracket := false, false, false
	for index := 0; index < len(code); index++ {
		if code[index] == '\\' {
			index++
			if index >= len(code) {
				return "", fmt.Errorf("incomplete XLSX format escape %q", code)
			}
		} else if code[index] == '"' {
			quoted = !quoted
		} else if !quoted && code[index] == '[' {
			bracket = true
		} else if !quoted && code[index] == ']' {
			bracket = false
		} else if !quoted && !bracket && strings.ContainsAny(strings.ToLower(code[index:index+1]), "dy") {
			datePattern = true
		}
	}
	if quoted {
		return "", fmt.Errorf("unclosed XLSX format literal %q", code)
	}
	if datePattern {
		maximum := 2958465.0
		if date1904 {
			maximum -= 1462
		}
		if number < 0 || number > maximum {
			return "", fmt.Errorf("XLSX date serial %g is outside the supported range", number)
		}
		day := int(math.Floor(number))
		date := time.Date(1899, 12, 31, 0, 0, 0, 0, time.UTC)
		if date1904 {
			date = time.Date(1904, 1, 1, 0, 0, 0, 0, time.UTC)
		} else if day >= 60 {
			day--
		}
		date = date.AddDate(0, 0, day)
		dayOfMonth := date.Day()
		weekday := date.Weekday()
		if !date1904 && int(number) < 61 {
			weekday = time.Weekday((int(number) + 6) % 7)
		}
		if !date1904 && int(number) == 0 {
			date = time.Date(1900, 1, 1, 0, 0, 0, 0, time.UTC)
			dayOfMonth = 0
		}
		if !date1904 && int(number) == 60 {
			dayOfMonth = 29
		}
		var output strings.Builder
		for index := 0; index < len(code); {
			character := code[index]
			if character == '\\' {
				index++
				if index >= len(code) {
					return "", fmt.Errorf("incomplete XLSX format escape %q", code)
				}
				output.WriteByte(code[index])
				index++
				continue
			}
			if character == '"' {
				end := strings.IndexByte(code[index+1:], '"')
				if end < 0 {
					return "", fmt.Errorf("unclosed XLSX format literal %q", code)
				}
				output.WriteString(code[index+1 : index+1+end])
				index += end + 2
				continue
			}
			lower := strings.ToLower(string(character))
			end := index + 1
			for end < len(code) && strings.EqualFold(code[end:end+1], lower) {
				end++
			}
			count := end - index
			switch lower {
			case "d":
				switch count {
				case 1:
					output.WriteString(strconv.Itoa(dayOfMonth))
				case 2:
					fmt.Fprintf(&output, "%02d", dayOfMonth)
				case 3:
					output.WriteString(weekday.String()[:3])
				case 4:
					output.WriteString(weekday.String())
				default:
					return value, nil
				}
			case "m":
				switch count {
				case 1:
					output.WriteString(strconv.Itoa(int(date.Month())))
				case 2:
					fmt.Fprintf(&output, "%02d", date.Month())
				case 3:
					output.WriteString(date.Format("Jan"))
				case 4:
					output.WriteString(date.Format("January"))
				default:
					return value, nil
				}
			case "y":
				if count == 2 {
					fmt.Fprintf(&output, "%02d", date.Year()%100)
				} else if count == 4 {
					fmt.Fprintf(&output, "%04d", date.Year())
				} else {
					return value, nil
				}
			default:
				if (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || strings.ContainsRune("[];_", rune(character)) {
					return value, nil
				}
				output.WriteString(code[index:end])
			}
			index = end
		}
		return output.String(), nil
	}
	if !xlsxSimpleNumberFormat.MatchString(code) {
		return value, nil
	}
	percent := strings.HasSuffix(code, "%")
	if percent {
		number *= 100
		code = strings.TrimSuffix(code, "%")
	}
	precision, minimum := 0, 0
	if index := strings.IndexByte(code, '.'); index >= 0 {
		fraction := code[index+1:]
		precision = len(fraction)
		minimum = strings.Count(fraction, "0")
	}
	if precision > 30 {
		return value, nil
	}
	factor := math.Pow10(precision)
	if math.Abs(number) <= math.MaxFloat64/factor {
		number = math.Round(number*factor) / factor
	}
	formatted := strconv.FormatFloat(number, 'f', precision, 64)
	parts := strings.SplitN(formatted, ".", 2)
	digits := parts[0]
	sign := ""
	if strings.HasPrefix(digits, "-") {
		sign, digits = "-", digits[1:]
	}
	if strings.Contains(code, ",") {
		for index := len(digits) - 3; index > 0; index -= 3 {
			digits = digits[:index] + "," + digits[index:]
		}
	}
	result := sign + digits
	if len(parts) > 1 {
		fraction := parts[1]
		for len(fraction) > minimum && strings.HasSuffix(fraction, "0") {
			fraction = strings.TrimSuffix(fraction, "0")
		}
		if fraction != "" {
			result += "." + fraction
		}
	}
	if percent {
		result += "%"
	}
	return result, nil
}
