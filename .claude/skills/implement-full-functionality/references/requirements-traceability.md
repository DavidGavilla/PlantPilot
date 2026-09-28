# Requirements → acceptance criteria → evidence

Before implementing, turn the feature request into a short table with stable IDs. Keep it in the coordinator's own working notes for the feature (not a new file per feature, unless the user asks for one) and paste the final state into the delivery report (Section H of `SKILL.md`).

## Table

| ID | Expected behavior | Affected components | Test/check planned | Evidence | Status |
|----|--------------------|----------------------|----------------------|----------|--------|
| R1 | ... | Controller/Service/Model | Unit / integration / browser / inspection | link or description of what actually ran | pending / passed / failed / blocked |

## Rules

- **Only explicit requirements and their necessary technical corollaries get an ID** — don't invent extra functionality the user didn't ask for and give it an ID as if it were requested. A technical decision required to satisfy an explicit requirement (e.g. "must filter by `userId`" as a corollary of "users must only see their own plants") is fine to include — a feature nobody asked for is not.
- **Evidence must be real**, not implied: "existing test suite has 28 tests" does not demonstrate a *new* requirement was exercised or that it passed. Evidence is: the specific test name that covers it and its actual pass/fail result, the actual HTTP response from a manual/integration check, or — only when nothing else applies — a stated, justified inspection (e.g. "confirmed by reading `X.cs:12`, no runtime check possible because Y").
- **Negative cases are required, not optional**, wherever relevant: insufficient permission (in this codebase: wrong/missing `userId`), invalid input, a resource belonging to another user. Give these their own row — don't fold them silently into the happy-path row.
- **No arbitrary coverage percentage.** Don't gate on "80% coverage" or similar; gate on "every row in this table has real evidence."
- **No tests that just restate the implementation** — a test that asserts the exact SQL/LINQ the service already generates, with no independent expectation, doesn't count as evidence.
- Distinguish, per row, in the final status: **implemented**, **tested/verified**, or **pending** — don't collapse "I wrote the code" into "I verified the behavior."

## Example

| ID | Expected behavior | Components | Test planned | Evidence | Status |
|----|---|---|---|---|---|
| R1 | `POST /api/users/{userId}/plants` creates a plant owned by `userId` | `PlantsController`, `PlantsService`, `Plant` | Integration | `CreatePlant_ReturnsCreated_ForOwningUser` — passed | passed |
| R2 | `GET /api/users/{userId}/plants/{plantId}` for a plant owned by another user returns 404, not the plant | `PlantsController`, `PlantsService` | Integration, negative case | `GetPlant_ForAnotherUsersPlant_ReturnsNotFound` — passed | passed |
| R3 | Invalid `Name` (empty) is rejected before hitting the service | `CreatePlantDtoValidator` | Unit | `Should_Have_Error_When_Name_Is_Empty` — passed | passed |
