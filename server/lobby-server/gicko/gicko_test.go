package gicko_test

import (
	"lobby/gicko"
	"math"
	"testing"

	"github.com/stretchr/testify/assert"
)

const place = 0.000000005

func round(x, unit float64) float64 {
	return math.Round(x/unit) * unit
}

func TestGicko(t *testing.T) {
	p := gicko.Player{
		R:  1500,
		RD: 200,
		V:  0.06,
	}
	p2 := gicko.Player{
		R:  1400,
		RD: 30,
		V:  0.06,
	}
	p3, _ := gicko.Update(p, p2, 1, 0.5)
	assert.Equal(t, round(1563.5641943063383, place), p3.R, "they should be equal")
	assert.Equal(t, round(175.402655938555, place), p3.RD, "they should be equal")
	assert.Equal(t, round(0.059998657304847616, place), p3.V, "they should be equal")
}

func TestGicko2(t *testing.T) {
	p := gicko.Player{
		R:  1500,
		RD: 200,
		V:  0.06,
	}
	p2 := gicko.Player{
		R:  1400,
		RD: 30,
		V:  0.06,
	}
	p3, _ := gicko.Update(p, p2, 0, 0.5)
	assert.Equal(t, round(1387.2576445904383, place), p3.R, "they should be equal")
	assert.Equal(t, round(175.40266927535413, place), p3.RD, "they should be equal")
	assert.Equal(t, round(0.06000085304430591, place), p3.V, "they should be equal")
}
