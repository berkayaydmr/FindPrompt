using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnPrompt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailedReason",
                table: "CourseFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "CourseFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    RawText = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentChunks_CourseFiles_CourseFileId",
                        column: x => x.CourseFileId,
                        principalTable: "CourseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentChunks_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentChunks_CourseFileId",
                table: "ContentChunks",
                column: "CourseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentChunks_CourseId_CourseFileId_OrderIndex",
                table: "ContentChunks",
                columns: new[] { "CourseId", "CourseFileId", "OrderIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentChunks");

            migrationBuilder.DropColumn(
                name: "FailedReason",
                table: "CourseFiles");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "CourseFiles");
        }
    }
}

