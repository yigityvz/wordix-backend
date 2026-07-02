using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLearningNotesAndFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLearningFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserLearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlagType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningFlags_UserLearningItems_UserLearningItemId",
                        column: x => x.UserLearningItemId,
                        principalTable: "UserLearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLearningNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserLearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningNotes_UserLearningItems_UserLearningItemId",
                        column: x => x.UserLearningItemId,
                        principalTable: "UserLearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningFlags_UserLearningItemId",
                table: "UserLearningFlags",
                column: "UserLearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningFlags_UserLearningItemId_FlagType",
                table: "UserLearningFlags",
                columns: new[] { "UserLearningItemId", "FlagType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningNotes_UserLearningItemId",
                table: "UserLearningNotes",
                column: "UserLearningItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLearningFlags");

            migrationBuilder.DropTable(
                name: "UserLearningNotes");
        }
    }
}
