# User Stories - FNOL (First Notice of Loss) Insurance System

## Epic Breakdown

### Epic 1: Policy and Claims Reporting
**Description:** Generate comprehensive reports for policies and their associated claims to support business analysis and compliance.

### Epic 2: Claim Initiation and Management
**Description:** Enable efficient creation, updating, and management of insurance claims through the FNOL process.

### Epic 3: Coverage Assessment and Matching
**Description:** Facilitate accurate coverage assessment and matching for property claims to ensure proper liability determination.

### Epic 4: Loss Details Management
**Description:** Capture and manage detailed loss information including dates, descriptions, and cause analysis.

### Epic 5: Claim Workflow and Status Management
**Description:** Support claim examination workflow and status tracking throughout the claim lifecycle.

### Epic 6: Data Validation and Error Handling
**Description:** Ensure data integrity through comprehensive validation and user-friendly error handling.

---

## Feature Map

### Reporting Features
- Policy and Claims Report Generation
- Policy No Claims Report Generation
- Claim Adjudication Report Generation

### Claim Management Features
- Claim Creation and Updates
- Loss Details Entry and Modification
- Claim Status Tracking

### Coverage Features
- Coverage Type Selection
- Coverage Match Validation
- Property Address Management

### Workflow Features
- Claim Examination Process
- Adjuster Assignment
- Status Progression

### Validation Features
- Input Data Validation
- Business Rule Enforcement
- Error Message Display

---

## User Personas

### Primary Users
- **Business User**: Reviews policy and claim reports for operational oversight
- **Business Analyst**: Analyzes policy data and identifies data integrity issues
- **Manager**: Oversees claim operations and requires comprehensive reporting
- **Claims Adjuster**: Assesses claim liability and updates coverage details
- **Claims Representative**: Processes claims and categorizes property claims
- **Data Entry Clerk**: Records property location and coverage details
- **Claims Processor**: Handles claim detail updates and coverage matching
- **Underwriter**: Reviews claim timeliness and assesses risk

### System Users
- **Batch System**: Automated report generation and data processing

---

## Stories by Epic

### Epic 1: Policy and Claims Reporting

#### Story ID: US-001
**Epic:** Policy and Claims Reporting  |  **Feature:** Policy and Claims Report Generation
**User Story:** As a Business User, I want to generate a comprehensive report detailing all property and casualty policies and their associated claims, so that I can review coverage and claim history for operational oversight.
**Acceptance Criteria:**
- Given the Policy and Claim data files exist and are accessible
- When I initiate the Policy and Claims Report generation process
- Then the system generates a report with policy details (number, version, status, product type, policyholder name, coverage amount, deductible) and associated claim information (claim number, type, dates, estimated loss amount)
- And the report includes proper headers, current date, and sequential page numbering
- And policies without claims show "NO CLAIM FOUND FOR POLICY" in the notes field
- And the report implements pagination with new headers after every 20 detail lines
**Story Points:** 8  |  **Priority:** Must Have

#### Story ID: US-002
**Epic:** Policy and Claims Reporting  |  **Feature:** Policy No Claims Report Generation
**User Story:** As a Business Analyst, I want to generate a report that lists all policies and explicitly identifies those without any associated claims, so that I can investigate data accuracy and policy risk assessment.
**Acceptance Criteria:**
- Given the Policy Master File and Claim Master File are accessible
- When I generate the Policy and Claims Report
- Then the system displays all policies with their details
- And for policies with associated claims, claim details are populated
- And for policies without claims, the notes section clearly states "NO CLAIM FOUND FOR POLICY"
- And the report includes formatted headers with title, current date, and page numbers
- And the report handles empty policy files gracefully by showing headers only
**Story Points:** 5  |  **Priority:** Must Have

#### Story ID: US-003
**Epic:** Policy and Claims Reporting  |  **Feature:** Claim Adjudication Report Generation
**User Story:** As a Manager, I want to generate reports summarizing claim adjudication details including assigned adjusters and their claim assignments, so that I can monitor workload distribution and performance.
**Acceptance Criteria:**
- Given adjuster and claim assignment data exists
- When I request a claim adjudication report
- Then the system retrieves and displays adjuster details (ID, first name, last name, total claims assigned)
- And displays comprehensive claim details (claim number, date of loss, financial status, coverage match status, coverage type, claim file status, estimated loss amount)
- And the report includes proper formatting with headers and pagination
- And missing assignment data is handled gracefully without stopping report generation
**Story Points:** 8  |  **Priority:** Should Have

### Epic 2: Claim Initiation and Management

#### Story ID: US-004
**Epic:** Claim Initiation and Management  |  **Feature:** Claim Creation and Updates
**User Story:** As a Claims Representative, I want to record initial loss details for a claim including core characteristics, so that I can start the First Notice of Loss (FNOL) process efficiently.
**Acceptance Criteria:**
- Given I am logged into the insurance system with a valid policy context
- When I access the Loss Details screen
- Then the system displays current date and my user ID automatically
- And I can enter claim type, dates of loss and reporting, reported by/to, loss type, cause of loss, estimated amount, and loss description
- And I can indicate whether a claim adjuster should be assigned
- And upon successful validation, the system generates a unique claim number for new claims
- And the system confirms successful creation or update of the claim record
- And if adjuster assignment is requested, the system presents the next screen for assignment
**Story Points:** 13  |  **Priority:** Must Have

#### Story ID: US-005
**Epic:** Claim Initiation and Management  |  **Feature:** Loss Details Entry and Modification
**User Story:** As a Claims Representative, I want to update loss details for an existing claim, so that I can maintain accurate and current claim information.
**Acceptance Criteria:**
- Given I have access to an existing claim record
- When I access the Loss Details screen for that claim
- Then the system pre-fills existing claim details including the claim number
- And I can modify editable loss detail fields
- And the system validates all input according to business rules
- And upon successful validation, the system updates the corresponding claim record
- And the system displays confirmation of successful update
**Story Points:** 8  |  **Priority:** Must Have

#### Story ID: US-006
**Epic:** Claim Initiation and Management  |  **Feature:** Claim Status Tracking
**User Story:** As an Underwriter, I want to view key loss details for a specific claim including dates of loss and reporting, so that I can assess the timeliness of claim filing and ensure policy compliance.
**Acceptance Criteria:**
- Given I am logged into the system with access to Loss Summary function
- When I enter a valid claim number
- Then the system retrieves and displays detailed loss summary including claim number, type, line of business, cause of loss, file status, dates, reporting details, examination information, and loss description
- And I can review the Date of Loss and Date Reported to evaluate claim timeliness
- And I can update editable fields such as Last Examined Date, Loss Description, and Examination Completed status
- And if I mark Examination Completed as "YES", the system navigates to the Claim Coverage Match screen
**Story Points:** 8  |  **Priority:** Must Have

### Epic 3: Coverage Assessment and Matching

#### Story ID: US-007
**Epic:** Coverage Assessment and Matching  |  **Feature:** Coverage Type Selection
**User Story:** As a Claims Adjuster, I want to view existing coverage information and update specific coverage details for a selected claim, so that I can accurately assess claim liability and ensure compliant claim processing.
**Acceptance Criteria:**
- Given I am logged into the system and have navigated to the Claim Coverage Details screen
- When the system receives valid Policy Number and Claim Number
- Then the system displays policy property address details and existing claim coverage details
- And I can modify Coverage (P=Property Damage, B=Builders Risk) and Line Cause of Loss (P=Property Damage, D=Debris, C=Construction Defect, E=Escalator) fields
- And upon successful validation and submission, the system updates the claim's Financial Status to "VERIFIED" and Coverage Match to "COVERED"
- And the system displays confirmation message "Coverage match success [Claim Number]"
- And control returns to the Loss Summary screen
**Story Points:** 8  |  **Priority:** Must Have

#### Story ID: US-008
**Epic:** Coverage Assessment and Matching  |  **Feature:** Coverage Match Validation
**User Story:** As a Claims Representative, I want to accurately categorize a property claim by selecting appropriate coverage type and line cause of loss, so that claims are correctly classified for proper processing and policy adherence.
**Acceptance Criteria:**
- Given I am on the Coverage Match screen with pre-populated claim details
- When I enter Coverage type (P or B) and Line Cause of Loss (P, D, C, or E)
- Then the system validates the entered values against business rules
- And if validations are successful, the system updates the claim record with new Coverage Type and Line Cause of Loss
- And the system automatically updates Financial Status to "VERIFIED" and Coverage Match to "COVERED"
- And the system displays success message and returns to Loss Summary screen
- And if validations fail, clear error messages are displayed for correction
**Story Points:** 5  |  **Priority:** Must Have

#### Story ID: US-009
**Epic:** Coverage Assessment and Matching  |  **Feature:** Property Address Management
**User Story:** As a Data Entry Clerk, I want to accurately record property location details from policy and associate relevant coverage details for a claim, so that comprehensive claim details enable proper processing and risk assessment.
**Acceptance Criteria:**
- Given I have access to a claim with identified Policy Number and Version
- When I access the Coverage Match screen
- Then the system displays current date, user ID, and claim number
- And the system retrieves and displays property address details from the policy record as read-only fields
- And I can input or modify Coverage and Line Cause of Loss fields
- And upon successful validation, the system updates the claim record and sets Financial Status to "VERIFIED" and Coverage Match to "COVERED"
- And the system displays confirmation message and returns to Loss Summary screen
**Story Points:** 5  |  **Priority:** Must Have

### Epic 4: Loss Details Management

#### Story ID: US-010
**Epic:** Loss Details Management  |  **Feature:** Loss Information Capture
**User Story:** As a Claims Representative, I want to capture comprehensive loss information including dates, amounts, and descriptions, so that I can ensure complete documentation for claim processing.
**Acceptance Criteria:**
- Given I am creating or updating a claim
- When I enter loss details on the Loss Details screen
- Then I can input Date of Loss (MM/DD/YYYY format, cannot be future date)
- And I can input Date Reported (MM/DD/YYYY format, cannot be future date, must be on or after Date of Loss)
- And I can enter Reported By (mandatory field)
- And I can select Loss Type (T=Theft, F=Fire, E=Earthquake, H=Hail/Flood)
- And I can select Cause of Loss (S=Sabotage, A=Accident, R=Arson, N=Natural)
- And I can enter Estimated Loss Amount (numeric)
- And I can provide Loss Description (mandatory first line, up to 3 lines total)
**Story Points:** 8  |  **Priority:** Must Have

#### Story ID: US-011
**Epic:** Loss Details Management  |  **Feature:** Loss Description Management
**User Story:** As an Underwriter, I want to update and review loss descriptions during claim examination, so that I can maintain accurate documentation and assessment records.
**Acceptance Criteria:**
- Given I am reviewing a claim on the Loss Summary screen
- When I access the loss description fields
- Then I can view existing loss description across up to three lines
- And I can modify the loss description text
- And the first line of loss description is mandatory and cannot be blank
- And upon submission, the system validates and saves the updated description
- And the updated description is reflected in the claim record
**Story Points:** 3  |  **Priority:** Should Have

### Epic 5: Claim Workflow and Status Management

#### Story ID: US-012
**Epic:** Claim Workflow and Status Management  |  **Feature:** Claim Examination Process
**User Story:** As an Underwriter, I want to mark claim examination as complete and track examination progress, so that I can manage the claim review workflow effectively.
**Acceptance Criteria:**
- Given I am reviewing a claim on the Loss Summary screen
- When I update the Last Examined Date and mark Examination Completed as "YES"
- Then the system validates that the examined date is not in the future and not before the reported date
- And the system validates that Examination Completed field is not blank and is either "YES" or "NO"
- And if Examination Completed is "YES", the system automatically navigates to the Claim Coverage Match screen
- And if Examination Completed is "NO", the system redisplays the Loss Summary screen with updated details
- And the system saves all examination progress to the claim record
**Story Points:** 5  |  **Priority:** Must Have

#### Story ID: US-013
**Epic:** Claim Workflow and Status Management  |  **Feature:** Adjuster Assignment
**User Story:** As a Claims Representative, I want to indicate whether a claim adjuster should be assigned during claim creation, so that appropriate resources can be allocated for claim processing.
**Acceptance Criteria:**
- Given I am creating a new claim on the Loss Details screen
- When I reach the Assign Claim Adjuster field
- Then I can enter "Y", "N", or leave blank
- And if I enter "Y", the system prepares for adjuster assignment workflow
- And if I enter "N" or leave blank, the system proceeds without adjuster assignment
- And the system validates the input and provides appropriate error messages for invalid entries
- And the assignment preference is saved with the claim record
**Story Points:** 3  |  **Priority:** Should Have

#### Story ID: US-014
**Epic:** Claim Workflow and Status Management  |  **Feature:** Status Progression
**User Story:** As a Claims Processor, I want the system to automatically update claim financial status and coverage match status based on coverage validation, so that claim progression is tracked accurately.
**Acceptance Criteria:**
- Given I have successfully validated and submitted coverage details
- When the system processes the coverage match
- Then the claim's Financial Status is automatically updated to "VERIFIED"
- And the claim's Coverage Match status is automatically updated to "COVERED"
- And these status changes are immediately reflected in the claim record
- And the status updates are visible on subsequent screen displays
- And the system maintains an audit trail of status changes
**Story Points:** 5  |  **Priority:** Must Have

### Epic 6: Data Validation and Error Handling

#### Story ID: US-015
**Epic:** Data Validation and Error Handling  |  **Feature:** Input Data Validation
**User Story:** As a Claims Processor, I want the system to validate my input data and provide clear error messages, so that I can correct invalid entries and maintain data integrity.
**Acceptance Criteria:**
- Given I am entering data on any claim-related screen
- When I submit invalid data
- Then the system validates Coverage field to ensure it is "P" or "B" only
- And the system validates Line Cause of Loss to ensure it is "P", "D", "C", or "E" only
- And the system validates date fields to ensure proper format and logical dates
- And the system validates mandatory fields are not blank
- And for each validation failure, the system displays specific, business-friendly error messages
- And the system keeps me on the current screen to correct invalid input
**Story Points:** 8  |  **Priority:** Must Have

#### Story ID: US-016
**Epic:** Data Validation and Error Handling  |  **Feature:** Business Rule Enforcement
**User Story:** As a System Administrator, I want the system to enforce business rules consistently across all claim processing functions, so that data integrity and compliance are maintained.
**Acceptance Criteria:**
- Given any user is processing claim data
- When business rules are applied
- Then Date of Loss must fall within policy effective and expiration dates
- And Date Reported cannot precede Date of Loss
- And Policy Status code "A" is displayed as "ACTIVE", others as "INACTIVE"
- And Claim Type defaults to "PROPERTY CLM" for property claims
- And all mandatory fields must be populated before saving
- And unique claim numbers are generated for new claims
- And the system prevents processing when business rules are violated
**Story Points:** 13  |  **Priority:** Must Have

#### Story ID: US-017
**Epic:** Data Validation and Error Handling  |  **Feature:** Error Message Display
**User Story:** As a Claims Processor, I want to receive clear and specific error messages when I enter invalid data, so that I can quickly understand and correct my mistakes.
**Acceptance Criteria:**
- Given I enter invalid data in any field
- When the system validates my input
- Then specific error messages are displayed for Coverage field: "ENTER P=Prop damage B=Builders Risk"
- And specific error messages are displayed for Line Cause of Loss: "ENTER P=PR dam D=Debris C=Cons E=Esc"
- And date validation errors provide clear guidance on acceptable formats and ranges
- And mandatory field errors clearly indicate which fields require input
- And error messages are displayed in a dedicated Warning/Errors area on the screen
- And I remain on the current screen to make corrections
**Story Points:** 5  |  **Priority:** Must Have

#### Story ID: US-018
**Epic:** Data Validation and Error Handling  |  **Feature:** System Error Handling
**User Story:** As a Claims Processor, I want the system to handle technical errors gracefully and provide meaningful feedback, so that I understand when system issues prevent normal processing.
**Acceptance Criteria:**
- Given a technical system error occurs during processing
- When file access errors, database connection issues, or system communication problems arise
- Then the system displays appropriate error messages indicating the nature of the problem
- And the system prevents data corruption by not saving incomplete transactions
- And for file not found errors, the system indicates which specific file is unavailable
- And for claim or policy not found scenarios, the system provides clear "not found" messages
- And the system logs technical errors for administrative review
- And users are provided with guidance on next steps (retry, contact support, etc.)
**Story Points:** 8  |  **Priority:** Should Have

---

## Sprint Backlog Suggestion

### Sprint 1 (Foundation Sprint)
**Focus:** Core claim creation and basic reporting
- US-004: Claim Creation and Updates (13 pts)
- US-001: Policy and Claims Report Generation (8 pts)
- US-015: Input Data Validation (8 pts)
**Total:** 29 points

### Sprint 2 (Coverage Management Sprint)
**Focus:** Coverage assessment and matching functionality
- US-007: Coverage Type Selection (8 pts)
- US-008: Coverage Match Validation (5 pts)
- US-009: Property Address Management (5 pts)
- US-014: Status Progression (5 pts)
- US-017: Error Message Display (5 pts)
**Total:** 28 points

### Sprint 3 (Workflow and Examination Sprint)
**Focus:** Claim examination and workflow management
- US-006: Claim Status Tracking (8 pts)
- US-012: Claim Examination Process (5 pts)
- US-005: Loss Details Entry and Modification (8 pts)
- US-010: Loss Information Capture (8 pts)
**Total:** 29 points

### Sprint 4 (Reporting and Enhancement Sprint)
**Focus:** Advanced reporting and system enhancements
- US-002: Policy No Claims Report Generation (5 pts)
- US-003: Claim Adjudication Report Generation (8 pts)
- US-016: Business Rule Enforcement (13 pts)
**Total:** 26 points

### Sprint 5 (Polish and Integration Sprint)
**Focus:** Final features and system integration
- US-011: Loss Description Management (3 pts)
- US-013: Adjuster Assignment (3 pts)
- US-018: System Error Handling (8 pts)
**Total:** 14 points

---

## Story Dependency Map

### Critical Path Dependencies
1. **US-004** (Claim Creation) → **US-005** (Loss Details Modification)
2. **US-004** (Claim Creation) → **US-006** (Claim Status Tracking)
3. **US-006** (Claim Status Tracking) → **US-012** (Claim Examination Process)
4. **US-012** (Claim Examination Process) → **US-007** (Coverage Type Selection)
5. **US-007** (Coverage Type Selection) → **US-014** (Status Progression)

### Supporting Dependencies
- **US-015** (Input Data Validation) supports all data entry stories
- **US-016** (Business Rule Enforcement) supports all claim processing stories
- **US-017** (Error Message Display) supports all user interaction stories

### Reporting Dependencies
- **US-001**, **US-002**, **US-003** can be developed in parallel as they are independent reporting features

### Independent Features
- **US-011** (Loss Description Management)
- **US-013** (Adjuster Assignment)
- **US-018** (System Error Handling)

---

## Definition of Ready
- User story follows the standard format (As a... I want... So that...)
- Acceptance criteria are clearly defined using Given/When/Then format
- Story points are estimated using Fibonacci sequence
- MoSCoW priority is assigned
- Dependencies are identified and documented
- Business rules and validation requirements are specified

## Definition of Done
- All acceptance criteria are met and tested
- Code is reviewed and approved
- Unit tests are written and passing
- Integration tests are completed
- User acceptance testing is completed
- Documentation is updated
- No critical defects remain
- Product Owner approval is obtained