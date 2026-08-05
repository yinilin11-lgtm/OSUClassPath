using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSUClassPath.Migrations
{
    /// <inheritdoc />
    public partial class AddYear2SpringCourseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            InsertCourse(
                migrationBuilder,
                "CSE 2331",
                "Foundations II: Data Structures and Algorithms",
                3,
                "Design/analysis of algorithms and data structures; divide-and-conquer; sorting and selection, search trees, hashing, graph algorithms, string matching; probabilistic analysis; randomized algorithms; NP-completeness.",
                "CSE 2122, 2123, 2124 or 2231; and CSE 2321; and STAT 3460, 3470 or 3201; and enrollment in CSE, CIS, ECE, Data Analytics, Math majors or CS minor. Not open to students with credit for CSE 5331.");

            InsertCourse(
                migrationBuilder,
                "CSE 2421",
                "Systems I: Introduction to Low-Level Programming and Computer Organization",
                4,
                "Introduction to computer architecture at machine and assembly language level; pointers and addressing; C programming at machine level; computer organization.",
                "CSE 2122, 2123, or 2231; and CSE 2321 or MATH 2566; and enrollment in CSE, CIS, Data Analytics, Music (BS), Engineering Physics, or Math major.");

            InsertCourse(
                migrationBuilder,
                "ECE 2060",
                "Introduction to Digital Logic",
                3,
                "Introduction to the theory and practice of combinational and clocked sequential networks.",
                "MATH 1148 or 1151. Not open to students with credit for ECE 2000, 2000.02, 2000.07, 2001, 2010, or 2017.");

            InsertCourse(
                migrationBuilder,
                "MATH 3345",
                "Foundations of Higher Mathematics",
                3,
                "Introduction to logic, proof techniques, set theory, number theory, real numbers.",
                "Major or minor in Math, CSE, CIS, ECE, IMME, STAT, STEMED-PRE or STEMED-BS. If Math, IMME, STAT, STEMED-PRE or STEMED-BS: a grade of C- or above in MATH 2153, 2162.xx, 2173, or 2182H. If CIS, CSE or ECE: a grade of C- or above in CSE 2321; and a grade of C- or above in MATH 1161.xx, 1172, 1181H, 1534, 1544, 1152, or 4181H.");

            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2331", "Foundations II: Data Structures and Algorithms", null);
            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2421", "Systems I: Introduction to Low-Level Programming and Computer Organization", null);
            UpdateRecommendedPlanItem(migrationBuilder, "ECE 2060", "Introduction to Digital Logic", null);
            UpdateRecommendedPlanItem(migrationBuilder, "MATH 3345", "Foundations of Higher Mathematics", null);
            UpdateRecommendedPlanItem(migrationBuilder, "GE-Diversity", "General Education: Race, Ethnicity and Gender Diversity", "Overall GE placeholder.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Courses
                WHERE CourseCode IN ('CSE 2331', 'CSE 2421', 'ECE 2060', 'MATH 3345');
                """);

            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2331", "Foundations II", null);
            UpdateRecommendedPlanItem(migrationBuilder, "CSE 2421", "Systems I", null);
            UpdateRecommendedPlanItem(migrationBuilder, "ECE 2060", "Introduction to Digital Logic", null);
            UpdateRecommendedPlanItem(migrationBuilder, "MATH 3345", "Foundations of Higher Mathematics", null);
            UpdateRecommendedPlanItem(migrationBuilder, "GE-Diversity", "General Education: Race, Ethnic and Gender Diversity", "Overall GE placeholder.");
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
