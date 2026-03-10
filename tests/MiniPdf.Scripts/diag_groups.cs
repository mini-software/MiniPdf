// Diagnostic: dump column widths and grouping for each sheet
#:project ../../src/MiniPdf/MiniPdf.csproj
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

var path = @"..\Issue_Files\xlsx\payroll-calculator_f.xlsx";
using var fs = File.OpenRead(path);
var sheets = MiniSoftware.ExcelReader.ReadSheets(fs);

foreach (var sheet in sheets)
{
    var maxCols = sheet.Rows.Count > 0 ? sheet.Rows.Max(r => r.Count) : 0;
    // Trim trailing empty columns
    while (maxCols > 0)
    {
        var col = maxCols - 1;
        var hasContent = false;
        foreach (var row in sheet.Rows)
        {
            if (col < row.Count && !string.IsNullOrEmpty(row[col].Text))
            {
                hasContent = true;
                break;
            }
        }
        if (hasContent) break;
        maxCols--;
    }

    // Paper size
    var (baseW, baseH) = sheet.PaperSize switch
    {
        9 => (595f, 842f),
        8 => (842f, 1191f),
        _ => (612f, 792f),
    };
    if (sheet.IsLandscape) (baseW, baseH) = (baseH, baseW);

    var mL = sheet.MarginLeftPt > 0 ? sheet.MarginLeftPt : 54f;
    var mR = sheet.MarginRightPt > 0 ? sheet.MarginRightPt : 54f;
    var usableWidth = baseW - mL - mR;

    var fontSize = 7f;
    var colPadding = 4f;
    if (sheet.PrintScale != 100 && sheet.PrintScale > 0)
    {
        var sc = sheet.PrintScale / 100f;
        fontSize *= sc;
        colPadding *= sc;
    }
    var avgCharWidth = fontSize * 0.47f;

    if (maxCols > 6)
        colPadding = Math.Max(2f, colPadding * 6f / maxCols);

    // Calculate natural widths (same as CalculateNaturalColumnWidths)
    var naturalWidths = new float[maxCols];
    for (int c = 0; c < maxCols; c++)
    {
        if (sheet.ColumnWidths.TryGetValue(c, out float cw) && cw > 0)
        {
            naturalWidths[c] = cw;
        }
        else if (sheet.DefaultColumnWidth > 0)
        {
            naturalWidths[c] = sheet.DefaultColumnWidth;
        }
        else
        {
            // content-based
            float maxW = 0;
            foreach (var row in sheet.Rows)
            {
                if (c < row.Count && !string.IsNullOrEmpty(row[c].Text))
                {
                    float tw = row[c].Text.Length * avgCharWidth;
                    if (tw > maxW) maxW = tw;
                }
            }
            naturalWidths[c] = Math.Max(maxW, avgCharWidth * 2);
        }
    }
    if (sheet.PrintScale != 100 && sheet.PrintScale > 0)
    {
        var sc = sheet.PrintScale / 100f;
        for (int i = 0; i < naturalWidths.Length; i++)
            naturalWidths[i] *= sc;
    }

    // fitToPage
    var total = naturalWidths.Sum() + colPadding * (maxCols - 1);
    if (sheet.FitToPage && total > usableWidth && maxCols > 1)
    {
        var fitScale = usableWidth / total;
        for (int i = 0; i < naturalWidths.Length; i++)
            naturalWidths[i] *= fitScale;
        colPadding *= fitScale;
        total = usableWidth;
    }

    Console.WriteLine($"\n=== {sheet.Name}: {maxCols} cols, page={baseW}x{baseH}, margins={mL:.1f}/{mR:.1f}, usable={usableWidth:.1f}, total={total:.1f}, fit={total <= usableWidth} ===");

    // Show column groups
    if (total > usableWidth)
    {
        var groups = new System.Collections.Generic.List<int[]>();
        var currentGroup = new System.Collections.Generic.List<int>();
        var currentWidth = 0f;
        for (int col = 0; col < maxCols; col++)
        {
            var colWithPadding = naturalWidths[col] + (currentGroup.Count > 0 ? colPadding : 0);
            if (currentGroup.Count > 0 && currentWidth + colWithPadding > usableWidth)
            {
                groups.Add(currentGroup.ToArray());
                currentGroup = new System.Collections.Generic.List<int> { col };
                currentWidth = naturalWidths[col];
            }
            else
            {
                currentGroup.Add(col);
                currentWidth += colWithPadding;
            }
        }
        if (currentGroup.Count > 0) groups.Add(currentGroup.ToArray());
        Console.WriteLine($"  Groups: {groups.Count}");
        for (int g = 0; g < groups.Count; g++)
        {
            var grp = groups[g];
            var gw = grp.Sum(c => naturalWidths[c]) + colPadding * (grp.Length - 1);
            Console.WriteLine($"  Group {g}: cols {grp.First()}-{grp.Last()} ({grp.Length} cols), width={gw:.1f}/{usableWidth:.1f}");
        }
    }
    else
    {
        Console.WriteLine($"  Single group: all {maxCols} columns fit");
    }

    // Show top 5 widest columns
    var ranked = naturalWidths.Select((w, i) => (i, w)).OrderByDescending(x => x.w).Take(10);
    Console.Write("  Widest cols: ");
    foreach (var (idx, w) in ranked) Console.Write($"[{idx}]={w:.1f} ");
    Console.WriteLine();
}
