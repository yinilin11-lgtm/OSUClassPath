# OSU CoursePath CSE Data Audit

Last checked: 2026-08-10

## Sources

- OSU CSE course catalog: https://cse.osu.edu/courses
- OSU BS CSE GOLD curriculum sheet: https://cse.osu.edu/sites/default/files/uploads/gold_bs_cse_requirements_and_sample_schedule_rev_071720_gold.pdf
- OSU BS CSE prerequisites description: https://cse.osu.edu/bs-cse-prerequisites-description

## Current Seed Data Summary

The tracked seed file is `OSUClassPath/Data/SeedCourses.json`. The local SQLite database can be recreated from this file.

| Category | Course Count | Sum of Listed Course Credits |
|---|---:|---:|
| General College of Engineering Requirements | 6 | 20 |
| Computer Science Core | 8 | 26 |
| Non-Computer Science Core | 5 | 15 |
| Computer Science Core Choices | 17 | 60 |
| CSE Math and Science Electives | 19 | 70 |
| CSE Technical Elective | 77 | 178 |
| Sample Schedule Additional Course | 1 | 3 |

Total courses in seed data: 133

## Technical Elective Audit

The GOLD curriculum sheet defines CSE technical electives as CSE courses at the 3000 level or above that are not already used for another degree requirement.

Current seed data includes:

- 94 CSE courses numbered 3000-5999.
- 77 courses categorized as `CSE Technical Elective`.
- 0 courses with a blank `PrerequisiteText` field.

The 77 technical electives exclude CSE courses already represented as core choices or capstone/core-choice courses in the GOLD curriculum sheet.

## Important Credit Note

The credit totals above are sums of all listed course options in a category. They are not the number of credits a student must take for that category.

For example, the technical elective category currently contains 77 possible courses with 178 total listed credits, but a student would only choose the required amount from that category according to the degree requirements.

## Program Requirement Data Gap

The app currently stores course-level information:

- course code
- title
- description
- credits
- prerequisite text
- category
- track
- source URL

The app does not yet have a structured model for broader BS CSE program requirements, such as:

- total credits required for the major/degree
- required credits by category
- minimum technical elective credits
- minimum math and science elective credits
- GE credit requirements
- residency, grade, or enrollment rules

Recommended next step: add a small tracked program requirement seed file, such as `OSUClassPath/Data/CseProgramRequirements.json`, and load it into the chat context so the advisor can answer questions like "How many technical elective credits do I need?" without confusing course-option credit sums with graduation requirements.
