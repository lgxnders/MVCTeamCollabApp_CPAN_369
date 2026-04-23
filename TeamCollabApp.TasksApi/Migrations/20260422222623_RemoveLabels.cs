using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamCollabApp.TasksApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardTaskLabels");

            migrationBuilder.DropTable(
                name: "TaskLabels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardTaskLabels",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    LabelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardTaskLabels", x => new { x.TaskId, x.LabelId });
                    table.ForeignKey(
                        name: "FK_BoardTaskLabels_BoardTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "BoardTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardTaskLabels_TaskLabels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "TaskLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardTaskLabels_LabelId",
                table: "BoardTaskLabels",
                column: "LabelId");
        }
    }
}
