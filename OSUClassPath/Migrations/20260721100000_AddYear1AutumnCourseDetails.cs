using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSUClassPath.Migrations
{
    /// <inheritdoc />
    public partial class AddYear1AutumnCourseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            InsertCourse(
                migrationBuilder,
                "ENGR 1100",
                "Introduction to Ohio State and Engineering",
                1,
                "Introduction to the University community and College of Engineering: Strategies for successful transition, academic requirements, University procedures, grading system, resources, overview of engineering academic areas of study and services.",
                "Enrollment in the College of Engineering. Not open to students with credit for FAES 1100, ArtsCol 1100, ArtsSci 1100, HumanEc 1100, and Exp 1100.");

            InsertCourse(
                migrationBuilder,
                "ENGR 1181",
                "Fundamentals of Engineering I",
                2,
                "Engineering problem solving utilizing computational tools such as Excel and MATLAB; hands-on experimentation; modeling; teamwork; written, oral and visual communications.",
                "Prereq or concur: Math 1140, or 1141; or Math 1150 or above. Not open to students with credit for ENGR 1182.01, 1182.02, 1182.03, 1182.04, 1282.01H, 1282.02H, 1282.03H, 1282.04H, 1186.01, 1187, or 1188.");

            InsertCourse(
                migrationBuilder,
                "MATH 1151",
                "Calculus I",
                5,
                "Differential and integral calculus of one real variable.",
                "A grade of C- or above in MATH 1148 and 1149, or in MATH 1144, 1150, or 150, or Math Placement Level L. Not open to students with credit for MATH 1152 or 152.xx, or above.");

            InsertCourse(
                migrationBuilder,
                "PHYSICS 1250",
                "Mechanics, Work and Energy, Thermal Physics",
                5,
                "Calculus-based introduction to classical physics: Newton's laws, work and energy, fluids, thermodynamics; for students in physical sciences, mathematics, and engineering.",
                "Prereq or concur: MATH 1141, 1151, 1154, 1156, 1161, 1181H, or 4181H.");

            InsertCourse(
                migrationBuilder,
                "CSE 1223",
                "Introduction to Computer Programming in Java",
                3,
                "Introduction to computer programming and to problem solving techniques using computer programs; programming lab experience.",
                "MATH 1120, 1130, 1140, 1148, 1149, 1150, or 1151, or Math Placement Level M.");

            UpdateRecommendedPlanItemTitle(migrationBuilder, "ENGR 1100", "Introduction to Ohio State and Engineering");
            UpdateRecommendedPlanItemTitle(migrationBuilder, "PHYSICS 1250", "Mechanics, Work and Energy, Thermal Physics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Courses
                WHERE CourseCode IN ('ENGR 1100', 'ENGR 1181', 'MATH 1151', 'PHYSICS 1250', 'CSE 1223');
                """);

            UpdateRecommendedPlanItemTitle(migrationBuilder, "ENGR 1100", "Engineering Survey");
            UpdateRecommendedPlanItemTitle(migrationBuilder, "PHYSICS 1250", "Mechanics, Thermal Physics, Waves");
        }

        private static void InsertCourse(
            MigrationBuilder migrationBuilder,
            string courseCode,
            string title,
            int credits,
            string description,
            string prerequisiteText)
        {
            migrationBuilder.Sql($$"""
                INSERT INTO Courses (CourseCode, Title, Description, Credits, PrerequisiteText, SourceUrl, LastVerified)
                SELECT '{{courseCode}}', '{{Escape(title)}}', '{{Escape(description)}}', {{credits}}, '{{Escape(prerequisiteText)}}', '', '2026-07-21 00:00:00'
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM Courses
                    WHERE CourseCode = '{{courseCode}}'
                );
                """);
        }

        private static void UpdateRecommendedPlanItemTitle(
            MigrationBuilder migrationBuilder,
            string courseCode,
            string title)
        {
            migrationBuilder.Sql($$"""
                UPDATE RecommendedPlanItems
                SET Title = '{{Escape(title)}}'
                WHERE CourseCode = '{{courseCode}}';
                """);
        }

        private static string Escape(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
