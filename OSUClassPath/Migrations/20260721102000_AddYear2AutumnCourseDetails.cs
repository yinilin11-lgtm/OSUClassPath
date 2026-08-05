using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSUClassPath.Migrations
{
    /// <inheritdoc />
    public partial class AddYear2AutumnCourseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            InsertCourse(
                migrationBuilder,
                "CSE 2231",
                "Software II: Software Development and Design",
                4,
                "Data representation using hashing, search trees, and linked data structures; algorithms for sorting; using trees for language processing; component interface design; best practices in Java.",
                "CSE 2221 with a C- or above. Concur: CSE 2321. Not open to students with credit for CSE 2231.01.");

            InsertCourse(
                migrationBuilder,
                "CSE 2321",
                "Foundations I: Discrete Structures",
                3,
                "Propositional and first-order logic; basic proof techniques; graphs, trees; analysis of algorithms; asymptotic analysis; recurrence relations.",
                "CSE 2122, 2123, 2124 or 2221; and MATH 1151 or 1161. Concur for students with credit for CSE 2221: CSE 2231.");

            InsertCourse(
                migrationBuilder,
                "STAT 3470",
                "Introduction to Probability and Statistics for Engineers",
                3,
                "Introduction to probability, Bayes theorem; discrete and continuous random variables, expected value, probability distributions; point and interval estimation; hypotheses tests for means and proportions; least squares regression.",
                "MATH 1152, 1161.xx, 1172, 1181H, or equivalent, or permission of instructor. Not open to students with credit for STAT 3440, 3450, 3450.01, 3450.02, 3460, 3470, or 3470.02.");

            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2231", "Software II: Software Development and Design", null);
            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2321", "Foundations I: Discrete Structures", null);
            UpdateRecommendedPlanItem(migrationBuilder, "STAT 3470", "Introduction to Probability and Statistics for Engineers", null);
            UpdateRecommendedPlanItem(migrationBuilder, "MATH/SCI Elective", "Math or Science Elective", "Choose from the approved list.");
            UpdateRecommendedPlanItem(migrationBuilder, "GE-Social", "General Education: Social and Behavioral Sciences", "Overall GE placeholder.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Courses
                WHERE CourseCode IN ('CSE 2231', 'CSE 2321', 'STAT 3470');
                """);

            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2231", "Software II", null);
            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2321", "Foundations I", null);
            UpdateRecommendedPlanItem(migrationBuilder, "STAT 3470", "Introduction to Probability and Statistics for Engineers", null);
            UpdateRecommendedPlanItem(migrationBuilder, "MATH/SCI Elective", "Math or Science Elective", "Choose from the approved list.");
            UpdateRecommendedPlanItem(migrationBuilder, "GE-Social", "General Education: Social and Behavioral Sciences", "Overall GE placeholder.");
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
