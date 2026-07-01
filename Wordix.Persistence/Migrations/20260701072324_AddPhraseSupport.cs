using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhraseSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Phrases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PhraseType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phrases_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_LearningItemId",
                table: "Phrases",
                column: "LearningItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_NormalizedText",
                table: "Phrases",
                column: "NormalizedText");

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_PhraseType",
                table: "Phrases",
                column: "PhraseType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Phrases");
        }
    }
}
