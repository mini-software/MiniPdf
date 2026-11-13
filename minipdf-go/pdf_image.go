package minipdf

import (
	"bytes"
	"compress/zlib"
	"encoding/hex"
	"fmt"
	"image"
	"image/color"
	"image/draw"
)

type imageOperation struct {
	x, y, width, height     float64
	pixelWidth, pixelHeight int
	data                    []byte
}

func newPDFImage(source image.Image, crop image.Rectangle) (imageOperation, error) {
	if crop.Empty() || !crop.In(source.Bounds()) {
		return imageOperation{}, fmt.Errorf("invalid PDF image crop %v", crop)
	}
	if int64(crop.Dx()) > 25000000/int64(crop.Dy()) {
		return imageOperation{}, fmt.Errorf("PDF image exceeds 25 million pixels")
	}
	raster := image.NewRGBA(image.Rect(0, 0, crop.Dx(), crop.Dy()))
	draw.Draw(raster, raster.Bounds(), &image.Uniform{C: color.White}, image.Point{}, draw.Src)
	draw.Draw(raster, raster.Bounds(), source, crop.Min, draw.Over)
	var compressed bytes.Buffer
	writer := zlib.NewWriter(&compressed)
	row := make([]byte, raster.Bounds().Dx()*3)
	for y := 0; y < raster.Bounds().Dy(); y++ {
		for x := 0; x < raster.Bounds().Dx(); x++ {
			value := raster.RGBAAt(x, y)
			row[x*3], row[x*3+1], row[x*3+2] = value.R, value.G, value.B
		}
		if _, err := writer.Write(row); err != nil {
			return imageOperation{}, err
		}
	}
	if err := writer.Close(); err != nil {
		return imageOperation{}, err
	}
	return imageOperation{pixelWidth: raster.Bounds().Dx(), pixelHeight: raster.Bounds().Dy(), data: compressed.Bytes()}, nil
}

func (operation imageOperation) appendPDF(buffer *bytes.Buffer, _ *embeddedFont) {
	fmt.Fprintf(buffer, "q %s 0 0 %s %s %s cm BI /W %d /H %d /CS /RGB /BPC 8 /F [/ASCIIHexDecode /FlateDecode] ID\n",
		pdfNumber(operation.width), pdfNumber(operation.height), pdfNumber(operation.x), pdfNumber(operation.y),
		operation.pixelWidth, operation.pixelHeight)
	buffer.WriteString(hex.EncodeToString(operation.data))
	buffer.WriteString(">\nEI Q\n")
}
