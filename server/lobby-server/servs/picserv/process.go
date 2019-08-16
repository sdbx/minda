package picserv

import (
	"image"
	"image/color"
	"image/draw"
	"math"

	"github.com/disintegration/imaging"
)

type circle struct {
	p image.Point
	r int
}

func (c *circle) ColorModel() color.Model {
	return color.AlphaModel
}

func (c *circle) Bounds() image.Rectangle {
	return image.Rect(c.p.X-c.r, c.p.Y-c.r, c.p.X+c.r, c.p.Y+c.r)
}

func (c *circle) At(x, y int) color.Color {
	xx, yy, rr := float64(x-c.p.X), float64(y-c.p.Y), float64(c.r)
	d := math.Sqrt(xx*xx+yy*yy) / rr
	if d > 1 {
		return color.Alpha{0}
	}
	return color.Alpha{255}
}

func addColor(cs ...color.Color) color.Color {
	out := color.RGBA64{}
	for _, c := range cs {
		r, g, b, a := c.RGBA()
		out.R += uint16(r)
		out.G += uint16(g)
		out.B += uint16(b)
		out.A += uint16(a)
	}
	return out
}

func divColor(c color.Color, n uint32) color.Color {
	r, g, b, a := c.RGBA()
	return &color.RGBA64{uint16(r / n), uint16(g / n), uint16(b / n), uint16(a / n)}
}

func antialias(src *image.RGBA) *image.RGBA {
	dst := image.NewRGBA(src.Bounds())
	for x := 0; x < src.Bounds().Max.X; x++ {
		for y := 0; y < src.Bounds().Max.Y; y++ {
			dst.Set(x, y, addColor(
				divColor(src.At(x, y), 2),
				divColor(src.At(x+1, y), 8),
				divColor(src.At(x-1, y), 8),
				divColor(src.At(x, y-1), 8),
				divColor(src.At(x, y+1), 8),
			))
		}
	}
	return dst
}

func (p *PicServ) CreateSkin(src image.Image, c color.Color) image.Image {
	width := src.Bounds().Max.X
	height := src.Bounds().Max.Y
	size := width
	if size > height {
		size = height
	}
	dst := image.NewRGBA(image.Rect(0, 0, size, size))
	draw.DrawMask(dst, dst.Bounds(), &image.Uniform{c}, image.ZP, &circle{image.Point{size / 2, size / 2}, size/2 - 1}, image.ZP, draw.Over)
	r := int(float64(size/2) * (1 - skinBorder))
	draw.DrawMask(dst, dst.Bounds(), src, image.ZP, &circle{image.Point{size / 2, size / 2}, r}, image.ZP, draw.Over)
	return imaging.Resize(antialias(dst), skinSize, skinSize, imaging.Lanczos)
}

func (p *PicServ) CreateProfile(src image.Image) image.Image {
	width := src.Bounds().Max.X
	height := src.Bounds().Max.Y
	size := width
	if size > height {
		size = height
	}
	dst := image.NewRGBA(image.Rect(0, 0, size, size))
	draw.Draw(dst, dst.Bounds(), src, image.Point{width/2 - size/2, height/2 - size/2}, draw.Src)
	return imaging.Resize(dst, profileSize, profileSize, imaging.Lanczos)
}
