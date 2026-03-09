
from src.calculator import add, subtract, calcu

def main():
    print("Simple Claude POC Calculator")
    print("2 + 3 =", add(2, 3))
    print("10 - 4 =", subtract(10, 4))
    print("calcu(10, 5, '+') =", calcu(10, 5, "+"))
    print("calcu(10, 5, '-') =", calcu(10, 5, "-"))
    print("calcu(10, 5, '*') =", calcu(10, 5, "*"))
    print("calcu(10, 5, '/') =", calcu(10, 5, "/"))
    print("calcu(2, 3, '**') =", calcu(2, 3, "**"))
    print("calcu(10, 3, '%') =", calcu(10, 3, "%"))
    print("calcu(10, 3, '//') =", calcu(10, 3, "//"))
    print("calcu(16, operator='sqrt') =", calcu(16, operator="sqrt"))
    print("calcu(-5, operator='abs') =", calcu(-5, operator="abs"))
    print("calcu(100, operator='log') =", calcu(100, operator="log"))

if __name__ == "__main__":
    main()
