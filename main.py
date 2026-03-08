
from src.calculator import add, subtract

def main():
    print("Simple Claude POC Calculator")
    print("2 + 3 =", add(2, 3))
    print("10 - 4 =", subtract(10, 4))

if __name__ == "__main__":
    main()
