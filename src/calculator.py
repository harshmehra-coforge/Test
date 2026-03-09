

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


def calcu(a, b, operator):
    """Perform a calculation on two numbers using the given operator (+, -, *, /)."""
    if operator == "+":
        return a + b
    elif operator == "-":
        return a - b
    elif operator == "*":
        return a * b
    elif operator == "/":
        if b == 0:
            raise ZeroDivisionError("division by zero")
        return a / b
    else:
        raise ValueError(f"Unsupported operator: '{operator}'. Use +, -, *, /")
