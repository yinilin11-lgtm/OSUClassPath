using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSUClassPath.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendedPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecommendedPlanTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    YearNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TermName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RecommendedCredits = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedPlanTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecommendedPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecommendedPlanTermId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Credits = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendedPlanItems_RecommendedPlanTerms_RecommendedPlanTermId",
                        column: x => x.RecommendedPlanTermId,
                        principalTable: "RecommendedPlanTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RecommendedPlanTerms",
                columns: new[] { "Id", "YearNumber", "TermName", "DisplayName", "RecommendedCredits", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, "Autumn", "Year 1 Autumn", 16, 1 },
                    { 2, 1, "Spring", "Year 1 Spring", 15, 2 },
                    { 3, 2, "Autumn", "Year 2 Autumn", 17, 3 },
                    { 4, 2, "Spring", "Year 2 Spring", 16, 4 },
                    { 5, 3, "Autumn", "Year 3 Autumn", 17, 5 },
                    { 6, 3, "Spring", "Year 3 Spring", 18, 6 },
                    { 7, 4, "Autumn", "Year 4 Autumn", 15, 7 },
                    { 8, 4, "Spring", "Year 4 Spring", 16, 8 }
                });

            migrationBuilder.InsertData(
                table: "RecommendedPlanItems",
                columns: new[] { "Id", "RecommendedPlanTermId", "CourseCode", "Title", "Credits", "ItemType", "Notes", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, "ENGR 1100", "Engineering Survey", 1, 0, null, 1 },
                    { 2, 1, "ENGR 1181", "Fundamentals of Engineering I", 2, 0, null, 2 },
                    { 3, 1, "MATH 1151", "Calculus I", 5, 0, null, 3 },
                    { 4, 1, "PHYSICS 1250", "Mechanics, Thermal Physics, Waves", 5, 0, null, 4 },
                    { 5, 1, "CSE 1223", "Introduction to Computer Programming in Java", 3, 0, null, 5 },
                    { 6, 2, "CSE 2221", "Software I", 4, 0, "Need C or better.", 1 },
                    { 7, 2, "ENGR 1182", "Fundamentals of Engineering II", 2, 0, null, 2 },
                    { 8, 2, "MATH 1172", "Engineering Mathematics A", 5, 0, null, 3 },
                    { 9, 2, "GE-Writing", "General Education: Writing", 3, 1, "Overall GE placeholder.", 4 },
                    { 10, 2, "GENED 1201", "GE Launch Seminar", 1, 1, null, 5 },
                    { 11, 3, "CSE 2231", "Software II", 4, 0, null, 1 },
                    { 12, 3, "CSE 2321", "Foundations I", 3, 0, null, 2 },
                    { 13, 3, "STAT 3470", "Introduction to Probability and Statistics for Engineers", 3, 0, null, 3 },
                    { 14, 3, "MATH/SCI Elective", "Math or Science Elective", 4, 2, "Choose from the approved list.", 4 },
                    { 15, 3, "GE-Social", "General Education: Social and Behavioral Sciences", 3, 1, "Overall GE placeholder.", 5 },
                    { 16, 4, "CSE 2331", "Foundations II", 3, 0, null, 1 },
                    { 17, 4, "CSE 2421", "Systems I", 4, 0, null, 2 },
                    { 18, 4, "ECE 2060", "Introduction to Digital Logic", 3, 0, null, 3 },
                    { 19, 4, "MATH 3345", "Foundations of Higher Mathematics", 3, 0, null, 4 },
                    { 20, 4, "GE-Diversity", "General Education: Race, Ethnic and Gender Diversity", 3, 1, "Overall GE placeholder.", 5 },
                    { 21, 5, "CSE 2431", "Systems II", 3, 0, null, 1 },
                    { 22, 5, "CSE 390X", "CSE Project Course", 4, 0, "Choose CSE 3901, 3902, or 3903.", 2 },
                    { 23, 5, "ECE 2020", "Introduction to Analog Systems and Circuits", 3, 0, null, 3 },
                    { 24, 5, "MATH 2568", "Linear Algebra", 4, 0, null, 4 },
                    { 25, 5, "GE-History", "General Education: Historical and Cultural Studies", 3, 1, "Overall GE placeholder.", 5 },
                    { 26, 6, "CSE 32X1", "Software Engineering or Databases Core Choice", 3, 3, "Choose CSE 3231 or CSE 3241.", 1 },
                    { 27, 6, "CSE 34X1", "Architecture or Networking Core Choice", 3, 3, "Choose CSE 3421 or CSE 3461.", 2 },
                    { 28, 6, "CSE 35X1", "AI or Graphics Core Choice", 3, 3, "Choose CSE 3521 or CSE 3541.", 3 },
                    { 29, 6, "CSE 2501/PHILOS 2338", "Computing Ethics Requirement", 1, 3, "CSE 2501 is 1 credit; PHILOS 2338 is 4 credits.", 4 },
                    { 30, 6, "GE-Theme", "General Education: Theme", 4, 1, "Required if PHILOS 2338 is not used for the GE theme.", 5 },
                    { 31, 6, "MATH/SCI Elective", "Math or Science Elective", 4, 2, "Choose from the approved list.", 6 },
                    { 32, 7, "CSE 3341", "Principles of Programming Languages", 3, 0, null, 1 },
                    { 33, 7, "Technical Elective", "Technical Elective", 3, 2, "CSE 3000-level or higher, or approved non-CSE elective.", 2 },
                    { 34, 7, "Technical Elective", "Technical Elective", 3, 2, "CSE 3000-level or higher, or approved non-CSE elective.", 3 },
                    { 35, 7, "Technical Elective", "Technical Elective", 3, 2, "CSE 3000-level or higher, or approved non-CSE elective.", 4 },
                    { 36, 7, "GE-Lit/VPA", "General Education: Literary, Visual and Performing Arts", 3, 1, "Overall GE placeholder.", 5 },
                    { 37, 8, "CSE 591X", "Capstone Experience", 4, 0, "Choose CSE 5911, 5912, 5913, 5914, 5915, or 5916.", 1 },
                    { 38, 8, "Technical Elective", "Technical Elective", 3, 2, "CSE 3000-level or higher, or approved non-CSE elective.", 2 },
                    { 39, 8, "Technical Elective", "Technical Elective", 3, 2, "CSE 3000-level or higher, or approved non-CSE elective.", 3 },
                    { 40, 8, "Technical Elective", "Technical Elective", 2, 2, "CSE 3000-level or higher, or approved non-CSE elective.", 4 },
                    { 41, 8, "GE-Theme", "General Education: Theme", 4, 1, "Overall GE placeholder.", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPlanItems_RecommendedPlanTermId_SortOrder",
                table: "RecommendedPlanItems",
                columns: new[] { "RecommendedPlanTermId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPlanTerms_SortOrder",
                table: "RecommendedPlanTerms",
                column: "SortOrder",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommendedPlanItems");

            migrationBuilder.DropTable(
                name: "RecommendedPlanTerms");
        }
    }
}
