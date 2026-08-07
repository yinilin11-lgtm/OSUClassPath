# Course Import Preview

This preview is based on the OSU BS CSE GOLD requirements PDF and official OSU course pages. It is not imported into the SQLite database yet.

Primary curriculum source:
https://cse.osu.edu/sites/default/files/uploads/gold_bs_cse_requirements_and_sample_schedule_rev_071720_gold.pdf

Last prepared: 2026-08-06

## Important Database Note

The current `Courses` table does not have a `Category` column. Before importing this dataset, it would be better to add a category field so the app can separate courses such as `Computer Science Core`, `Core Choices`, and `Math and Science Electives`.

## Confirmed Course Scope

The list below includes courses explicitly shown in non-GE categories of the GOLD BS CSE curriculum sheet, plus `CSE 1223` from the sample schedule.

| Category | CourseCode | Title | Credits | Source Status |
|---|---:|---|---:|---|
| General College of Engineering Requirements | ENGR 1100 | Introduction to Ohio State and Computer Science and Engineering | 1 | From curriculum PDF; needs course-page verification |
| General College of Engineering Requirements | ENGR 1181 | Fundamentals of Engineering I | 2 | From curriculum PDF; needs course-page verification |
| General College of Engineering Requirements | ENGR 1182 | Fundamentals of Engineering II | 2 | From curriculum PDF; needs course-page verification |
| General College of Engineering Requirements | MATH 1151 | Calculus I | 5 | Verified from OSU Math course page |
| General College of Engineering Requirements | MATH 1172 | Engineering Mathematics A | 5 | Verified from OSU Math course page |
| General College of Engineering Requirements | PHYSICS 1250 | Mechanics, Work and Energy, Thermal Physics | 5 | Verified from OSU Physics course page |
| Sample Schedule Additional Course | CSE 1223 | Introduction to Computer Programming in Java | 3 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2221 | Software I: Software Components | 4 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2231 | Software II: Software Development and Design | 4 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2321 | Foundations I: Discrete Structures | 3 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2331 | Foundations II: Data Structures and Algorithms | 3 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2421 | Systems I: Introduction to Low-Level Programming and Computer Organization | 4 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2431 | Systems II: Introduction to Operating Systems | 3 | Verified from OSU CSE course page |
| Computer Science Core | CSE 2501 | Social, Ethical, and Professional Issues in Computing | 1 | Verified from OSU CSE course page |
| Computer Science Core | PHILOS 1338 | Ethics in the Professions: Introduction to Computing Ethics and Effective Presentation | 4 | From curriculum PDF; needs course-page verification |
| Non-Computer Science Core | ECE 2020 | Introduction to Analog Systems and Circuits | 3 | From curriculum PDF; needs course-page verification |
| Non-Computer Science Core | ECE 2060 | Introduction to Digital Logic | 3 | From curriculum PDF; needs course-page verification |
| Non-Computer Science Core | MATH 2568 | Linear Algebra | 3 | From curriculum PDF; needs course-page verification |
| Non-Computer Science Core | MATH 3345 | Foundations of Higher Mathematics | 3 | Verified from OSU Math course page |
| Non-Computer Science Core | STAT 3470 | Introduction to Probability and Statistics for Engineers | 3 | From curriculum PDF; needs course-page verification |
| Computer Science Core Choices | CSE 3231 | Software Engineering Techniques | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3241 | Introduction to Database Systems | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3321 | Automata and Formal Languages | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3341 | Principles of Programming Languages | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3421 | Introduction to Computer Architecture | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3461 | Computer Networking and Internet Technologies | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3521 | Survey of Artificial Intelligence I: Basic Techniques | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3541 | Computer Game and Animation Techniques | 3 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3901 | Project: Design, Development, and Documentation of Web Applications | 4 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3902 | Project: Design, Development, and Documentation of Interactive Systems | 4 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 3903 | Project: Design, Development, and Documentation of System Software | 4 | Verified from OSU CSE course page |
| Computer Science Core Choices | CSE 5911 | Capstone Design: Software Applications | 4 | Needs course-page verification |
| Computer Science Core Choices | CSE 5912 | Capstone Design: Game Design and Development | 4 | Needs course-page verification |
| Computer Science Core Choices | CSE 5913 | Capstone Design: Computer Animation | 4 | Needs course-page verification |
| Computer Science Core Choices | CSE 5914 | Capstone Design: Knowledge-Based Systems | 4 | Needs course-page verification |
| Computer Science Core Choices | CSE 5915 | Capstone Design: Information Systems | 4 | Needs course-page verification |
| Computer Science Core Choices | CSE 5916 | Capstone Design: Research-Focused Projects | 4 | Verified from OSU CSE course page |
| CSE Math and Science Electives | MATH 2153 | Calculus III | 4 | Verified from OSU Math course page |
| CSE Math and Science Electives | MATH 2255 | Differential Equations and Their Applications | 3 | Verified from OSU Math course page |
| CSE Math and Science Electives | MATH 2415 | Ordinary and Partial Differential Equations | 3 | Verified from OSU Math course page |
| CSE Math and Science Electives | STAT 4201 | Introduction to Mathematical Statistics I | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | STAT 5301 | Intermediate Data Analysis I | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | ANTHROP 2200 | Introduction to Physical Anthropology | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | BIOLOGY 1113 | Biological Sciences: Energy Transfer and Development | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | BIOLOGY 1114 | Biological Sciences: Form, Function, Diversity, and Ecology | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | CHEM 1210 | General Chemistry I | 5 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | CHEM 1250 | General Chemistry for Engineers | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | EARTHSC 1121 | The Dynamic Earth | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | EARTHSC 1122 | Earth Through Time | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | ENR 2100 | Introduction to Environmental Science | 3 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | ENR 3000 | Soil Science | 3 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | ENR 3001 | Soil Science Laboratory | 1 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | FDSCTE 2200 | The Science of Food | 3 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | HCS 2201 | Ecology of Managed Plant Systems | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | HCS 2202 | Form and Function in Cultivated Plants | 4 | From curriculum PDF; needs course-page verification |
| CSE Math and Science Electives | PHYSICS 1251 | E&M, Optics, Modern Physics | 5 | From curriculum PDF; needs course-page verification |

## Next Review Questions

1. Should the database keep only courses explicitly listed in this GOLD PDF?
2. Should I add a `Category` column to the `Courses` table before importing?
3. Should I use the GOLD curriculum version as the main source even though it is older than the Autumn 2022 New GE sheet?
