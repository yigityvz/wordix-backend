using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSentenceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sentences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExternalSentenceId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SourceProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    License = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sentences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sentences_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sentences_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentenceTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSentenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TranslatedText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NormalizedTranslatedText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SourceProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    License = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentenceTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentenceTranslations_Languages_TargetLanguageId",
                        column: x => x.TargetLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SentenceTranslations_Sentences_SourceSentenceId",
                        column: x => x.SourceSentenceId,
                        principalTable: "Sentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_ExternalSentenceId",
                table: "Sentences",
                column: "ExternalSentenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_LanguageId_NormalizedText",
                table: "Sentences",
                columns: new[] { "LanguageId", "NormalizedText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_LearningItemId",
                table: "Sentences",
                column: "LearningItemId",
                unique: true,
                filter: "[LearningItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_SourceProvider",
                table: "Sentences",
                column: "SourceProvider");

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_IsPrimary",
                table: "SentenceTranslations",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_SourceProvider",
                table: "SentenceTranslations",
                column: "SourceProvider");

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_SourceSentenceId_TargetLanguageId",
                table: "SentenceTranslations",
                columns: new[] { "SourceSentenceId", "TargetLanguageId" });

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_SourceSentenceId_TargetLanguageId_NormalizedTranslatedText",
                table: "SentenceTranslations",
                columns: new[] { "SourceSentenceId", "TargetLanguageId", "NormalizedTranslatedText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_TargetLanguageId",
                table: "SentenceTranslations",
                column: "TargetLanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentenceTranslations");

            migrationBuilder.DropTable(
                name: "Sentences");
        }
    }
}
