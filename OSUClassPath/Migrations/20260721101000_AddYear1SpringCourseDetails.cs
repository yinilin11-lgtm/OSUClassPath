using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSUClassPath.Migrations
{
    /// <inheritdoc />
    public partial class AddYear1SpringCourseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            InsertCourse(
                migrationBuilder,
                "CSE 2221",
                "Software I: Software Components",
                4,
                "Intellectual foundations of software engineering; design-by-contract principles; mathematical modeling of software functionality; component-based software from client perspective; layered data representation.",
                "CSE 1212, 1221, 1222, 1223, 1224, ENGR 1221, 1281.01H, 1281.02H, or CSE Placement Level A. Prereq or concur: MATH 1151, 1161.01, or 1161.02. Not open to students with credit for CSE 5022.");

            InsertCourse(
                migrationBuilder,
                "ENGR 1182",
                "Fundamentals of Engineering II",
                2,
                "Introduction to 3D visualization and CAD; engineering design-build process; teamwork; written, oral and visual communications; project management.",
                "ENGR 1181.01, 1181.02, 1281.01H, 1281.02H, or 1281.03H. Prereq or concur: MATH 1141, or MATH 1151 or above. Not open to students with credit for ENGR 1186.01, 1187, or 1188; not repeatable if credit earned for ENGR 1182.01, 1182.02, 1182.03, 1182.04, 1282.01H, 1282.02H, 1282.03H, or 1282.04H.");

            InsertCourse(
                migrationBuilder,
                "MATH 1172",
                "Engineering Mathematics A",
                5,
                "Techniques of integration, Taylor series, differential calculus of several variables. Applications.",
                "A grade of C- or above in MATH 1114, 1151, 1156, 1161.xx, 152.xx, 161.xx, or 161.01H. Not open to students with credit for MATH 1152, 1534, 1544, any Math class numbered 1172 or above, or any quarter-system Math class numbered 254.xx or above. Not open to students majoring in Math, pre-Actuarial Science, or Actuarial Science.");

            InsertCourse(
                migrationBuilder,
                "GE-Writing",
                "General Education: Writing",
                3,
                "Overall General Education writing placeholder. OSU GE writing requirements include foundational information literacy and advanced writing courses.",
                "Choose an approved General Education writing course that satisfies the student's catalog requirements.");

            InsertCourse(
                migrationBuilder,
                "GENED 1201",
                "Launch Seminar for First-Year Students",
                1,
                "Introduces students to the broad goals of the General Education program and the skills needed to succeed.",
                "First-year student standing or advisor placement.");

            UpdateRecommendedPlanItem(
                migrationBuilder,
                "CSE 2221",
                "Software I: Software Components",
                "Need C or better.");

            UpdateRecommendedPlanItem(
                migrationBuilder,
                "GE-Writing",
                "General Education: Writing",
                "Overall GE placeholder.");

            UpdateRecommendedPlanItem(
                migrationBuilder,
                "GENED 1201",
                "Launch Seminar for First-Year Students",
                null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Courses
                WHERE CourseCode IN ('CSE 2221', 'ENGR 1182', 'MATH 1172', 'GE-Writing', 'GENED 1201');
                """);

            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2221", "Software I", "Need C or better.");
            UpdateRecommendedPlanItem(migrationBuilder, "GE-Writing", "General Education: Writing", "Overall GE placeholder.");
            UpdateRecommendedPlanItem(migrationBuilder, "GENED 1201", "GE Launch Seminar", null);
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

        private static void UpdateRecommendedPlanItem(
            MigrationBuilder migrationBuilder,
            string courseCode,
            string title,
            string notes)
        {
            var escapedNotes = notes is null ? "NULL" : $"'{Escape(notes)}'";

            migrationBuilder.Sql($$"""
                UPDATE RecommendedPlanItems
                SET Title = '{{Escape(title)}}',
                    Notes = {{escapedNotes}}
                WHERE CourseCode = '{{courseCode}}';
                """);
        }

        private static string Escape(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
