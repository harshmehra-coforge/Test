
import unittest
from src.calculator import add, subtract, multiply, divide, calculate


def test_add():
    assert add(2, 3) == 5

def test_subtract():
    assert subtract(10, 4) == 6


class TestCalculate(unittest.TestCase):

    def test_addition(self):
        self.assertEqual(calculate(10, 3, "+"), 13.0)

    def test_subtraction(self):
        self.assertEqual(calculate(10, 3, "-"), 7.0)

    def test_multiplication(self):
        self.assertEqual(calculate(10, 3, "*"), 30.0)

    def test_division(self):
        self.assertAlmostEqual(calculate(10, 3, "/"), 3.3333333333333335)

    def test_float_inputs(self):
        self.assertAlmostEqual(calculate(1.5, 2.5, "+"), 4.0)

    def test_division_by_zero(self):
        with self.assertRaises(ZeroDivisionError):
            calculate(1, 0, "/")

    def test_invalid_operator(self):
        with self.assertRaises(ValueError) as ctx:
            calculate(1, 2, "^")
        self.assertIn("^", str(ctx.exception))

    def test_returns_float(self):
        result = calculate(4, 2, "+")
        self.assertIsInstance(result, float)


if __name__ == "__main__":
    unittest.main()
