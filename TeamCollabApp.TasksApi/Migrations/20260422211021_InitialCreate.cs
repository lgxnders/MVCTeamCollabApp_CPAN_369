using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamCollabApp.TasksApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardTasks_BoardColumns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "TaskAssignees",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignees", x => new { x.TaskId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TaskAssignees_BoardTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "BoardTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChecklistItems_BoardTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "BoardTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardTaskLabels_LabelId",
                table: "BoardTaskLabels",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardTasks_ColumnId",
                table: "BoardTasks",
                column: "ColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_TaskId",
                table: "TaskChecklistItems",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardTaskLabels");

            migrationBuilder.DropTable(
                name: "TaskAssignees");

            migrationBuilder.DropTable(
                name: "TaskChecklistItems");

            migrationBuilder.DropTable(
                name: "TaskLabels");

            migrationBuilder.DropTable(
                name: "BoardTasks");

            migrationBuilder.DropTable(
                name: "BoardColumns");
        }
    }
}
