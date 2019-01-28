from typing import List

class Cord:
    def __init__(self, x: int, y: int, z: int):
        self.x = x
        self.y = y
        self.z = z

    def __add__(self, other: 'Cord') -> 'Cord':
        return Cord(self.x + other.x, self.y + other.y, self.z + other.z)

    def __neg__(self) -> 'Cord':
        return Cord(-self.x, -self.y, -self.z)

    def __sub__(self, other: 'Cord') -> 'Cord':
        return self + (-other)

    def __floordiv__(self, t: int) -> 'Cord':
        return Cord(self.x // t, self.y // t, self.z // t)

    def is_linear_to(self, other: 'Cord') -> bool:
        v = [abs(self.x - other.x), abs(self.y - other.y), abs(self.z - other.z)]
        v = sorted(v)
        return v[0] == 0 and v[1] == v[2]
    
    def distance(self, other: 'Cord') -> int:
        return max(abs(self.x-other.x), abs(self.y-other.y), abs(self.z-other.z))
    
    def dir(self, other: 'Cord') -> 'Cord':
        n = self.distance(other)
        t = other - self
        return t // n

    def vec_size(self) -> int:
        return Cord(0,0,0).distance(self)

    def is_linear_vec(self) -> bool:
        return Cord(0,0,0).is_linear_to(self)

    def linedraw(self, other: 'Cord') -> List['Cord']:
        n = self.distance(other)
        return [self.cube_lerp(other, 1.0 / n * i) for i in range(n+1)]

    @staticmethod
    def lerp(a: int, b: int, t: float) -> int: 
        return int(a + ((b - a) * t))

    def cube_lerp(self, other: 'Cord', t: float) -> 'Cord':
        return Cord(Cord.lerp(self.x, other.x, t), 
                Cord.lerp(self.y, other.y, t),
                Cord.lerp(self.z, other.z, t))