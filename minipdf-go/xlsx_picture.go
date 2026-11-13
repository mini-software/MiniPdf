package minipdf

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"image"
	_ "image/gif"
	_ "image/jpeg"
	_ "image/png"
	"math"
	"strings"
)

type xlsxPicture struct {
	anchorRow, anchorColumn                int
	rowOffset, columnOffset, width, height float64
	image                                  imageOperation
}

func readXLSXPictures(files officePackage, worksheetName string, grid xlsxGrid) ([]xlsxPicture, error) {
	name, drawing, err := readXLSXDrawing(files, worksheetName)
	if err != nil {
		return nil, err
	}
	anchors := append(drawing.OneCell, drawing.TwoCell...)
	var pictures []xlsxPicture
	var targets map[string]string
	for _, anchor := range anchors {
		if anchor.Picture == nil || (anchor.Client.Print != nil && !*anchor.Client.Print) {
			continue
		}
		if anchor.From.Row < 0 || anchor.From.Row >= 1048576 || anchor.From.Column < 0 || anchor.From.Column >= 16384 ||
			anchor.From.RowOffset < 0 || anchor.From.ColumnOffset < 0 || anchor.To.Row > 1048576 || anchor.To.Column > 16384 {
			return nil, fmt.Errorf("invalid XLSX picture anchor in %s", name)
		}
		if targets == nil {
			part := xlsxRelationshipPartName(name)
			data, err := files.read(part)
			if err != nil {
				return nil, err
			}
			var relationships xlsxRelationshipsDocument
			if err := xml.Unmarshal(data, &relationships); err != nil {
				return nil, fmt.Errorf("parse %s: %w", part, err)
			}
			targets = make(map[string]string)
			for _, relationship := range relationships.Items {
				if strings.HasSuffix(relationship.Type, "/image") {
					if relationship.TargetMode == "External" {
						return nil, fmt.Errorf("unsupported external XLSX picture %s", relationship.ID)
					}
					targets[relationship.ID] = xlsxResolveRelationshipTarget(name, relationship.Target)
				}
			}
		}
		target := targets[anchor.Picture.Fill.Blip.ID]
		if target == "" {
			return nil, fmt.Errorf("missing XLSX picture relationship %q", anchor.Picture.Fill.Blip.ID)
		}
		data, err := files.read(target)
		if err != nil {
			return nil, err
		}
		config, _, err := image.DecodeConfig(bytes.NewReader(data))
		if err != nil {
			return nil, fmt.Errorf("decode XLSX picture %s: %w", target, err)
		}
		if config.Width <= 0 || config.Height <= 0 || int64(config.Width) > 25000000/int64(config.Height) {
			return nil, fmt.Errorf("XLSX picture %s exceeds 25 million pixels", target)
		}
		source, _, err := image.Decode(bytes.NewReader(data))
		if err != nil {
			return nil, fmt.Errorf("decode XLSX picture %s: %w", target, err)
		}
		crop := anchor.Picture.Fill.Crop
		if min(crop.Left, crop.Right, crop.Top, crop.Bottom) < 0 || max(crop.Left, crop.Right, crop.Top, crop.Bottom) > 100000 ||
			crop.Left+crop.Right >= 100000 || crop.Top+crop.Bottom >= 100000 {
			return nil, fmt.Errorf("unsupported XLSX picture crop in %s", name)
		}
		bounds := source.Bounds()
		left := bounds.Min.X + int(math.Round(float64(bounds.Dx()*crop.Left)/100000))
		top := bounds.Min.Y + int(math.Round(float64(bounds.Dy()*crop.Top)/100000))
		cropWidth := int(math.Round(float64(bounds.Dx()*(100000-crop.Left-crop.Right)) / 100000))
		cropHeight := int(math.Round(float64(bounds.Dy()*(100000-crop.Top-crop.Bottom)) / 100000))
		rectangle := image.Rect(left, top, min(bounds.Max.X, left+cropWidth), min(bounds.Max.Y, top+cropHeight))
		raster, err := newPDFImage(source, rectangle)
		if err != nil {
			return nil, fmt.Errorf("read XLSX picture %s: %w", target, err)
		}
		x := float64(anchor.From.ColumnOffset) / xlsxEmuPerPoint
		y := float64(anchor.From.RowOffset) / xlsxEmuPerPoint
		width := float64(anchor.Extension.Width) / xlsxEmuPerPoint
		height := float64(anchor.Extension.Height) / xlsxEmuPerPoint
		if width <= 0 {
			width = grid.columnOffset(anchor.To.Column) - grid.columnOffset(anchor.From.Column) + float64(anchor.To.ColumnOffset)/xlsxEmuPerPoint - x
		}
		if height <= 0 {
			height = grid.rowOffset(anchor.To.Row) - grid.rowOffset(anchor.From.Row) + float64(anchor.To.RowOffset)/xlsxEmuPerPoint - y
		}
		if width <= 0 || height <= 0 {
			return nil, fmt.Errorf("invalid XLSX picture size in %s", name)
		}
		pictures = append(pictures, xlsxPicture{anchorRow: anchor.From.Row + 1, anchorColumn: anchor.From.Column + 1,
			columnOffset: x, rowOffset: y, width: width, height: height, image: raster})
	}
	return pictures, nil
}

func renderXLSXPictures(page *PDFPage, grid xlsxGrid, rows, columns xlsxRange) {
	for _, picture := range grid.pictures {
		if picture.anchorRow < rows.start || picture.anchorRow > rows.end || picture.anchorColumn < columns.start || picture.anchorColumn > columns.end ||
			grid.hiddenRows[picture.anchorRow] || grid.hiddenColumns[picture.anchorColumn] {
			continue
		}
		operation := picture.image
		operation.x = grid.margins.Left + xlsxRangeSize(xlsxRange{start: columns.start, end: picture.anchorColumn - 1}, grid.columnWidth) + picture.columnOffset*grid.layoutScale()
		operation.y = grid.pageSize.Height - grid.margins.Top - xlsxRangeSize(xlsxRange{start: rows.start, end: picture.anchorRow - 1}, grid.rowHeight) - (picture.rowOffset+picture.height)*grid.layoutScale()
		operation.width, operation.height = picture.width*grid.layoutScale(), picture.height*grid.layoutScale()
		page.operations = append(page.operations, operation)
	}
}
