
import math

def add(a, b):
    """Return the sum of two numbers."""
    return a + b

def subtract(a, b):
    """Return the difference between two numbers."""
    return a - b

def multiply(a, b):
    """Return the product of two numbers."""
    return a * b

def divide(a, b):
    """Return the quotient of two numbers. Raises ZeroDivisionError if b is zero."""
    if b == 0:
        raise ZeroDivisionError("division by zero")
    return a / b

def calculate(a: float, b: float, operator: str) -> float:
    """
    Perform an arithmetic operation on two numbers.

    Args:
        a: Left operand.
        b: Right operand.
        operator: One of '+', '-', '*', '/'.

    Returns:
        Result of the operation as a float.

    Raises:
        ValueError: If the operator is not supported.
        ZeroDivisionError: If dividing by zero.
    """
    ops = {
        "+": add,
        "-": subtract,
        "*": multiply,
        "/": divide,
    }
    if operator not in ops:
        raise ValueError(f"Unsupported operator: '{operator}'. Must be one of: +, -, *, /")
    return float(ops[operator](a, b))


_BINARY_OPS = {"+", "-", "*", "/", "**", "%", "//"}
_UNARY_OPS  = {"sqrt", "abs", "log", "ln"}

def calcu(a, b=None, operator="+") -> float:
    """
    Perform a basic or advanced calculation.

    Two-operand operators (+, -, *, /, **, %, //):
        calcu(a, b, operator)

    Single-operand operators (sqrt, abs, log, ln):
        calcu(a, operator='sqrt')

    Args:
        a:        First operand.
        b:        Second operand (required for binary ops, omit for unary).
        operator: Operation string. Default '+'.

    Returns:
        Result as float.

    Raises:
        ValueError:        Unsupported operator, missing b for binary op,
                           sqrt of negative, log of non-positive.
        ZeroDivisionError: Division, modulus, or floor-division by zero.

    Examples:
        calcu(2, 3, '**')          -> 8.0
        calcu(10, 3, '%')          -> 1.0
        calcu(10, 3, '//')         -> 3.0
        calcu(16, operator='sqrt') -> 4.0
        calcu(-5, operator='abs')  -> 5.0
        calcu(100, operator='log') -> 2.0
    """
    if operator in _UNARY_OPS:
        if operator == "sqrt":
            if a < 0:
                raise ValueError(f"Cannot take sqrt of negative number: {a}")
            return float(math.sqrt(a))
        if operator == "abs":
            return float(abs(a))
        if operator == "log":
            if a <= 0:
                raise ValueError(f"log requires a positive number, got: {a}")
            return float(math.log10(a))
        if operator == "ln":
            if a <= 0:
                raise ValueError(f"ln requires a positive number, got: {a}")
            return float(math.log(a))

    if operator in _BINARY_OPS:
        if b is None:
            raise ValueError(f"b is required for binary operator '{operator}'")
        if operator == "+":
            return float(a + b)
        if operator == "-":
            return float(a - b)
        if operator == "*":
            return float(a * b)
        if operator == "/":
            if b == 0:
                raise ZeroDivisionError("division by zero")
            return float(a / b)
        if operator == "**":
            return float(a ** b)
        if operator == "%":
            if b == 0:
                raise ZeroDivisionError("modulo by zero")
            return float(a % b)
        if operator == "//":
            if b == 0:
                raise ZeroDivisionError("floor division by zero")
            return float(a // b)

    valid = sorted(_BINARY_OPS | _UNARY_OPS)
    raise ValueError(f"Unsupported operator: '{operator}'. Valid operators: {valid}")
