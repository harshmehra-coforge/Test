
from src.calculator import add, subtract, calcu

def main():
    print("Simple Claude POC Calculator")
    print("2 + 3 =", add(2, 3))
    print("10 - 4 =", subtract(10, 4))
    print("calcu(10, 5, '+') =", calcu(10, 5, "+"))
    print("calcu(10, 5, '-') =", calcu(10, 5, "-"))
    print("calcu(10, 5, '*') =", calcu(10, 5, "*"))
    print("calcu(10, 5, '/') =", calcu(10, 5, "/"))

if __name__ == "__main__":
    main()
