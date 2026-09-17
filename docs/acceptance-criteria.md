# Acceptance Criteria — Basic Python Calculator

## Scope and Traceability

| Story | Acceptance criteria |
|---|---|
| CALC-1 — Perform basic arithmetic | AC-CALC-1.1 to AC-CALC-1.6 |
| CALC-2 — Handle invalid input | AC-CALC-2.1 to AC-CALC-2.4 |
| CALC-3 — Prevent division by zero | AC-CALC-3.1 to AC-CALC-3.3 |

**Source:** `USER_STORIES (2).md`

## CALC-1 — Perform Basic Arithmetic

### AC-CALC-1.1 — Addition

**Given** two valid numeric inputs and the addition operation  
**When** the user submits the calculation  
**Then** the calculator displays the mathematically correct sum  
**And** no error is displayed.

### AC-CALC-1.2 — Subtraction

**Given** two valid numeric inputs and the subtraction operation  
**When** the user submits the calculation  
**Then** the calculator displays the mathematically correct difference  
**And** no error is displayed.

### AC-CALC-1.3 — Multiplication

**Given** two valid numeric inputs and the multiplication operation  
**When** the user submits the calculation  
**Then** the calculator displays the mathematically correct product  
**And** no error is displayed.

### AC-CALC-1.4 — Division with a non-zero divisor

**Given** two valid numeric inputs and a non-zero divisor  
**When** the user submits the division calculation  
**Then** the calculator displays the mathematically correct quotient  
**And** no error is displayed.

### AC-CALC-1.5 — Decimal arithmetic

**Given** valid decimal numeric inputs and any supported operation  
**When** the user submits the calculation  
**Then** the calculator displays the mathematically correct decimal result accurately  
**And** the result follows the defined numeric output-format and precision rules.

### AC-CALC-1.6 — Supported operation set

**Given** two valid numeric inputs  
**When** the user selects or submits an operation  
**Then** addition, subtraction, multiplication, and division are supported  
**And** no other operation is treated as a valid operation unless separately specified.

## CALC-2 — Handle Invalid Input

### AC-CALC-2.1 — Non-numeric first input

**Given** the first input is not numeric  
**When** the user submits the calculation  
**Then** the calculator displays a clear validation message  
**And** does not return a calculation result  
**And** does not crash.

### AC-CALC-2.2 — Non-numeric second input

**Given** the second input is not numeric  
**When** the user submits the calculation  
**Then** the calculator displays a clear validation message  
**And** does not return a calculation result  
**And** does not crash.

### AC-CALC-2.3 — Unsupported operation

**Given** the operation is not one of addition, subtraction, multiplication, or division  
**When** the user submits the calculation  
**Then** the calculator displays a clear unsupported-operation error  
**And** does not return a calculation result  
**And** does not crash.

### AC-CALC-2.4 — Safe handling of invalid requests

**Given** one or more submitted values are invalid  
**When** the user submits the calculation  
**Then** the application handles the request without an unhandled exception or crash  
**And** the response identifies that the input cannot be processed.

## CALC-3 — Prevent Division by Zero

### AC-CALC-3.1 — Division by zero is rejected

**Given** the selected operation is division and the divisor is zero  
**When** the user submits the calculation  
**Then** the calculator displays an explanatory division-by-zero error  
**And** does not return a numeric result  
**And** does not fail.

### AC-CALC-3.2 — Division-by-zero handling applies to decimal dividends

**Given** the dividend is numeric, including a decimal value, and the divisor is zero  
**When** the user submits the division calculation  
**Then** the calculator applies the same safe division-by-zero behavior  
**And** displays an explanatory error without returning a result.

### AC-CALC-3.3 — Application remains usable after the error

**Given** a division-by-zero request has been rejected  
**When** the user submits a subsequent valid calculation  
**Then** the calculator accepts and processes the subsequent calculation correctly  
**And** the user is not required to restart the application.

## Definition of Acceptance

The calculator meets the supplied stories when all criteria above pass, automated tests pass, and no unhandled crash occurs for the specified invalid-input and division-by-zero scenarios.
