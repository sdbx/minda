use std::iter::FromIterator;
use std::collections::VecDeque;
use std::cmp::max;
use std::ops;

#[derive(Serialize, Deserialize, Copy, Clone, PartialEq, Debug)]
pub struct Cord(pub isize, pub isize, pub isize);

impl Cord {
    const zero: Cord = Cord(0,0,0);

    pub fn is_linear_to(self, other: Cord) -> bool {
        let mut v = [(self.0-other.0).abs(),
            (self.1-other.1).abs(),
            (self.2-other.2).abs()];
        v.sort();
        v[0] == 0 && v[1] == v[2]
    }

    pub fn distance(self, other: Cord) -> usize {
        max(max((self.0-other.0).abs(), (self.1-other.1).abs()), (self.2-other.2).abs()) as usize
    }

    pub fn dir(self, other: Cord) -> Cord {
        let n = self.distance(other) as isize;
        let t = other - self;
        t / n
    }

    pub fn vec_size(self) -> usize {
        Cord::zero.distance(self)
    }

    pub fn is_linear_vec(self) -> bool {
        Cord::zero.is_linear_to(self)
    }

    pub fn linedraw(self, other: Cord) -> Vec<Cord> {
        let n = self.distance(other);
        let mut out: VecDeque<Cord> = VecDeque::new();
        for i in 0..n+1 {
            out.push_back(self.cube_lerp(other, 1.0 / n as f32 * i as f32));
        }
        Vec::from_iter(out.into_iter())
    }

    fn lerp(a: isize, b: isize, t: f32) -> isize {
        a + ((b - a) as f32 * t) as isize
    }

    fn cube_lerp(self, other: Cord, t: f32) -> Cord {
        Cord(Cord::lerp(self.0, other.0, t), 
                Cord::lerp(self.1, other.1, t),
                Cord::lerp(self.2, other.2, t))
    }

}

impl ops::Add<Cord> for Cord{
    type Output = Cord;

    fn add(self, _rhs: Cord) -> Cord {
        Cord(self.0 + _rhs.0, self.1 + _rhs.1, self.2 + _rhs.2)
    }
}

impl ops::Sub<Cord> for Cord{
    type Output = Cord;

    fn sub(self, _rhs: Cord) -> Cord {
        Cord(self.0 - _rhs.0, self.1 - _rhs.1, self.2 - _rhs.2)
    }
}

impl ops::Mul<isize> for Cord{
    type Output = Cord;

    fn mul(self, _rhs: isize) -> Cord {
        Cord(self.0 * _rhs, self.1 * _rhs, self.2 * _rhs)
    }
}

impl ops::Div<isize> for Cord{
    type Output = Cord;

    fn div(self, _rhs: isize) -> Cord {
        Cord(self.0 / _rhs, self.1 / _rhs, self.2 / _rhs)
    }
}

impl ops::Neg for Cord {
    type Output = Cord;

    fn neg(self) -> Cord {
        self * -1
    }
}

