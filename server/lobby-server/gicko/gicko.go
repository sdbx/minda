package gicko

import (
	. "math"
)

type Player struct {
	R  float64
	RD float64
	V  float64
}

func Update(p Player, opp Player, s float64, t float64) (Player, float64) {
	ca := &GickoCalculation{
		R:   p.R,
		R2:  opp.R,
		RD:  p.RD,
		RD2: opp.RD,
		V:   p.V,
		V2:  opp.V,
		S:   s,
		T:   t,
	}
	delta := ca.Calculate()
	return Player{
		R:  ca.R,
		RD: ca.RD,
		V:  ca.V,
	}, delta
}

type GickoCalculation struct {
	R   float64
	R2  float64
	RD  float64
	RD2 float64
	V   float64
	V2  float64
	S   float64
	T   float64

	ee float64

	a  float64
	t  float64
	m  float64
	m2 float64
	p  float64
	p2 float64
	d  float64
}

const place = 0.000000005

func (ca *GickoCalculation) Calculate() float64 {
	ca.m, ca.m2 = mu(ca.R), mu(ca.R2)
	ca.p, ca.p2 = phi(ca.RD), phi(ca.RD2)
	ca.ee = e(ca.m, ca.m2, ca.p2)
	ca.d = ca.delta()
	ca.a = Log(Pow(ca.V, 2))
	vpp := ca.vp()
	pstar := Sqrt(Pow(ca.p, 2) + Pow(vpp, 2))
	pp := 1 / (Sqrt(1/Pow(pstar, 2) + 1/ca.ev()))
	mp := ca.m + Pow(pp, 2)*g(ca.p2)*(ca.S-ca.ee)
	rp := 173.7178*mp + 1500
	rdp := 173.7178 * pp
	ca.R = round(rp, place)
	ca.RD = round(rdp, place)
	ca.V = round(vpp, place)
	return rp - ca.R
}

func (ca *GickoCalculation) vp() float64 {
	A := ca.a
	var B float64
	if ca.d > Pow(ca.p, 2)+ca.ev() {
		B = Log(Pow(ca.d, 2) - Pow(ca.p, 2) - ca.ev())
	} else {
		k := 1.0
		for ca.f(ca.a-k*ca.T) < 0 {
			k++
		}
		B = ca.a - k*ca.T
	}
	ep := 0.0000001
	fA, fB := ca.f(A), ca.f(B)
	for Abs(B-A) > ep {
		C := A + (A-B)*fA/(fB-fA)
		fC := ca.f(C)
		if fC*fB < 0 {
			A = B
			fA = fB
		} else {
			fA = fA / 2
		}
		B = C
		fB = fC
	}
	return Exp(A / 2)
}

func (ca *GickoCalculation) f(x float64) float64 {
	num := Exp(x) * (Pow(ca.d, 2) - Pow(ca.p, 2) - ca.ev() - Exp(x))
	den := 2 * Pow((Pow(ca.p, 2)+ca.ev()+Exp(x)), 2)
	return num*rep(den) - (x-ca.a)/Pow(ca.T, 2)
}

func (ca *GickoCalculation) delta() float64 {
	return ca.ev() * g(ca.p2) * (ca.S - ca.ee)
}

func (ca *GickoCalculation) ev() float64 {
	return rep(Pow(g(ca.p2), 2) * ca.ee * (1 - ca.ee))
}

func g(p float64) float64 {
	return rep(Sqrt(1 + 3*p*p/Pow(Pi, 2)))
}

func rep(n float64) float64 {
	if n == 0 {
		return 1 / 0.0000000001
	}
	return 1 / n
}

func e(m float64, m2 float64, p2 float64) float64 {
	return rep(1 + Exp(-g(p2)*(m-m2)))
}

func phi(rd float64) float64 {
	return rd / 173.7178
}

func mu(r float64) float64 {
	return (r - 1500.0) / 173.7178
}

func round(x, unit float64) float64 {
	return Round(x/unit) * unit
}
