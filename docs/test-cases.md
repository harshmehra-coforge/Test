# Test Cases — Basic Python Calculator

## Scope and Traceability

| Requirement | Coverage |
|---|---|
| CALC-1 — Perform basic arithmetic | TC-01 to TC-08 |
| CALC-2 — Handle invalid input | TC-09 to TC-12 |
| CALC-3 — Prevent division by zero | TC-13 to TC-15 |

**Source:** `USER_STORIES (2).md`  
**Input/interface note:** The supplied requirements do not define an exact command-line, API, or UI contract; test steps therefore refer to the documented calculator input/output interface without inventing field names or message text.

## Test Cases

### TC-01 — Add two positive integers

- **Requirement:** CALC-1
- **Type:** Functional / Positive
- **Priority:** High
- **Preconditions:** Calculator is available through the supported input/output interface.
- **Test data:** First number `7`; operation `addition`; second number `5`.
- **Steps:**
  1. Submit the two numbers and addition operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `12` as the result and does not display an error.

### TC-02 — Subtract two numbers

- **Requirement:** CALC-1
- **Type:** Functional / Positive
- **Priority:** High
- **Preconditions:** Calculator is available.
- **Test data:** First number `7`; operation `subtraction`; second number `5`.
- **Steps:**
  1. Submit the two numbers and subtraction operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `2` as the result and does not display an error.

### TC-03 — Multiply two numbers

- **Requirement:** CALC-1
- **Type:** Functional / Positive
- **Priority:** High
- **Preconditions:** Calculator is available.
- **Test data:** First number `7`; operation `multiplication`; second number `5`.
- **Steps:**
  1. Submit the two numbers and multiplication operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `35` as the result and does not display an error.

### TC-04 — Divide two non-zero numbers

- **Requirement:** CALC-1
- **Type:** Functional / Positive
- **Priority:** High
- **Preconditions:** Calculator is available; divisor is non-zero.
- **Test data:** First number `15`; operation `division`; second number `3`.
- **Steps:**
  1. Submit the two numbers and division operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `5` as the result and does not display an error.

### TC-05 — Add decimal numbers accurately

- **Requirement:** CALC-1
- **Type:** Functional / Decimal precision
- **Priority:** High
- **Preconditions:** Calculator is available; the supported decimal format is used.
- **Test data:** First number `1.25`; operation `addition`; second number `2.50`.
- **Steps:**
  1. Submit the decimal numbers and addition operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays the mathematically correct decimal result, `3.75`, accurately and without an error.

### TC-06 — Subtract decimal numbers accurately

- **Requirement:** CALC-1
- **Type:** Functional / Decimal precision
- **Priority:** Medium
- **Preconditions:** Calculator is available.
- **Test data:** First number `5.75`; operation `subtraction`; second number `2.25`.
- **Steps:**
  1. Submit the decimal numbers and subtraction operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `3.50` or the equivalent numeric representation `3.5`, consistent with the defined output-format rules. No error is displayed.

### TC-07 — Multiply decimal numbers accurately

- **Requirement:** CALC-1
- **Type:** Functional / Decimal precision
- **Priority:** Medium
- **Preconditions:** Calculator is available.
- **Test data:** First number `1.5`; operation `multiplication`; second number `2.4`.
- **Steps:**
  1. Submit the decimal numbers and multiplication operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `3.6` accurately and does not display an error.

### TC-08 — Divide decimal numbers accurately

- **Requirement:** CALC-1
- **Type:** Functional / Decimal precision
- **Priority:** Medium
- **Preconditions:** Calculator is available; divisor is non-zero.
- **Test data:** First number `7.5`; operation `division`; second number `2.5`.
- **Steps:**
  1. Submit the decimal numbers and division operation.
  2. Execute the calculation.
- **Expected result:** The calculator displays `3.0` or the equivalent numeric representation `3`, consistent with the defined output-format rules. No error is displayed.

### TC-09 — Reject non-numeric first input

- **Requirement:** CALC-2
- **Type:** Functional / Negative
- **Priority:** High
- **Preconditions:** Calculator is available.
- **Test data:** First number `abc`; supported operation `addition`; second number `5`.
- **Steps:**
  1. Submit the non-numeric first value, operation, and valid second value.
  2. Execute the calculation.
- **Expected result:** A clear validation message is shown, no calculation result is returned, and the application does not crash.

### TC-10 — Reject non-numeric second input

- **Requirement:** CALC-2
- **Type:** Functional / Negative
- **Priority:** High
- **Preconditions:** Calculator is available.
- **Test data:** First number `5`; supported operation `addition`; second number `abc`.
- **Steps:**
  1. Submit the valid first value, operation, and non-numeric second value.
  2. Execute the calculation.
- **Expected result:** A clear validation message is shown, no calculation result is returned, and the application does not crash.

### TC-11 — Reject unsupported operation

- **Requirement:** CALC-2
- **Type:** Functional / Negative
- **Priority:** High
- **Preconditions:** Calculator is available.
- **Test data:** First number `5`; operation `modulus` or another operation not supported by the requirements; second number `2`.
- **Steps:**
  1. Submit the two valid numbers with the unsupported operation.
  2. Execute the calculation.
- **Expected result:** A clear unsupported-operation error is shown, no result is returned, and the application does not crash.

### TC-12 — Handle multiple invalid fields without crashing

- **Requirement:** CALC-2
- **Type:** Functional / Negative / Robustness
- **Priority:** Medium
- **Preconditions:** Calculator is available.
- **Test data:** First number `abc`; unsupported operation `modulus`; second number `xyz`.
- **Steps:**
  1. Submit all invalid values.
  2. Execute the calculation.
- **Expected result:** The application handles the request safely and provides a clear validation/error response. It does not crash or return a misleading numeric result.

### TC-13 — Reject division by zero with integer dividend

- **Requirement:** CALC-3
- **Type:** Functional / Negative
- **Priority:** Critical
- **Preconditions:** Calculator is available.
- **Test data:** First number `10`; operation `division`; second number `0`.
- **Steps:**
  1. Submit the dividend, division operation, and zero divisor.
  2. Execute the calculation.
- **Expected result:** An explanatory division-by-zero error is shown, no result is returned, and the application does not fail.

### TC-14 — Reject division by zero with decimal dividend

- **Requirement:** CALC-3
- **Type:** Functional / Boundary / Negative
- **Priority:** High
- **Preconditions:** Calculator is available.
- **Test data:** First number `10.5`; operation `division`; second number `0`.
- **Steps:**
  1. Submit the decimal dividend, division operation, and zero divisor.
  2. Execute the calculation.
- **Expected result:** An explanatory division-by-zero error is shown, no result is returned, and the application does not fail.

### TC-15 — Continue accepting calculations after division-by-zero error

- **Requirement:** CALC-3
- **Type:** Functional / Recovery
- **Priority:** Critical
- **Preconditions:** Calculator is available.
- **Test data:** First calculation: `10 / 0`; subsequent calculation: `8 / 2`.
- **Steps:**
  1. Submit and execute `10 / 0`.
  2. Verify the explanatory error and absence of a result.
  3. Submit and execute `8 / 2` without restarting the application.
- **Expected result:** The first calculation is rejected safely. The application remains operational and returns `4` for the subsequent valid calculation.

## Coverage Notes

- Positive coverage includes all four required operations.
- Decimal coverage includes all four operations.
- Negative coverage includes non-numeric inputs, unsupported operations, and division by zero.
- Recovery coverage verifies that a handled division-by-zero error does not prevent later valid calculations.
- Exact interface syntax, accepted numeric range, precision/rounding policy, and exact error-message wording were not provided; these require confirmation before automating interface-level assertions.
