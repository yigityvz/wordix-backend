using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentSourceAndExampleSentences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentSource",
                table: "SentenceTranslations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QualityStatus",
                table: "SentenceTranslations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContentSource",
                table: "Sentences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QualityStatus",
                table: "Sentences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContentSource",
                table: "Meanings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "License",
                table: "Meanings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QualityStatus",
                table: "Meanings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceProvider",
                table: "Meanings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContentSource",
                table: "LearningItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSourceKey",
                table: "LearningItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedAt",
                table: "LearningItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QualityStatus",
                table: "LearningItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LearningItemExampleSentences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentenceTranslationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningItemExampleSentences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningItemExampleSentences_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LearningItemExampleSentences_SentenceTranslations_SentenceTranslationId",
                        column: x => x.SentenceTranslationId,
                        principalTable: "SentenceTranslations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LearningItemExampleSentences_Sentences_SentenceId",
                        column: x => x.SentenceId,
                        principalTable: "Sentences",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "LearningItems",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                columns: new[] { "ContentSource", "ExternalSourceKey", "ImportedAt", "QualityStatus" },
                values: new object[] { 1, null, null, 1 });

            migrationBuilder.UpdateData(
                table: "LearningItems",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                columns: new[] { "ContentSource", "ExternalSourceKey", "ImportedAt", "QualityStatus" },
                values: new object[] { 1, null, null, 1 });

            migrationBuilder.UpdateData(
                table: "LearningItems",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                columns: new[] { "ContentSource", "ExternalSourceKey", "ImportedAt", "QualityStatus" },
                values: new object[] { 1, null, null, 1 });

            migrationBuilder.UpdateData(
                table: "Meanings",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"),
                columns: new[] { "ContentSource", "License", "QualityStatus", "SourceProvider" },
                values: new object[] { 1, null, 1, "PrototypeSeed" });

            migrationBuilder.UpdateData(
                table: "Meanings",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc2"),
                columns: new[] { "ContentSource", "License", "QualityStatus", "SourceProvider" },
                values: new object[] { 1, null, 1, "PrototypeSeed" });

            migrationBuilder.UpdateData(
                table: "Meanings",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc3"),
                columns: new[] { "ContentSource", "License", "QualityStatus", "SourceProvider" },
                values: new object[] { 1, null, 1, "PrototypeSeed" });

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_ContentSource",
                table: "SentenceTranslations",
                column: "ContentSource");

            migrationBuilder.CreateIndex(
                name: "IX_SentenceTranslations_QualityStatus",
                table: "SentenceTranslations",
                column: "QualityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_ContentSource",
                table: "Sentences",
                column: "ContentSource");

            migrationBuilder.CreateIndex(
                name: "IX_Sentences_QualityStatus",
                table: "Sentences",
                column: "QualityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Meanings_ContentSource",
                table: "Meanings",
                column: "ContentSource");

            migrationBuilder.CreateIndex(
                name: "IX_Meanings_QualityStatus",
                table: "Meanings",
                column: "QualityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Meanings_SourceProvider",
                table: "Meanings",
                column: "SourceProvider");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_ContentSource",
                table: "LearningItems",
                column: "ContentSource");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_ExternalSourceKey",
                table: "LearningItems",
                column: "ExternalSourceKey");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_QualityStatus",
                table: "LearningItems",
                column: "QualityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItemExampleSentences_LearningItemId",
                table: "LearningItemExampleSentences",
                column: "LearningItemId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItemExampleSentences_LearningItemId_DisplayOrder",
                table: "LearningItemExampleSentences",
                columns: new[] { "LearningItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningItemExampleSentences_LearningItemId_SentenceId",
                table: "LearningItemExampleSentences",
                columns: new[] { "LearningItemId", "SentenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningItemExampleSentences_SentenceId",
                table: "LearningItemExampleSentences",
                column: "SentenceId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItemExampleSentences_SentenceTranslationId",
                table: "LearningItemExampleSentences",
                column: "SentenceTranslationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningItemExampleSentences");

            migrationBuilder.DropIndex(
                name: "IX_SentenceTranslations_ContentSource",
                table: "SentenceTranslations");

            migrationBuilder.DropIndex(
                name: "IX_SentenceTranslations_QualityStatus",
                table: "SentenceTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Sentences_ContentSource",
                table: "Sentences");

            migrationBuilder.DropIndex(
                name: "IX_Sentences_QualityStatus",
                table: "Sentences");

            migrationBuilder.DropIndex(
                name: "IX_Meanings_ContentSource",
                table: "Meanings");

            migrationBuilder.DropIndex(
                name: "IX_Meanings_QualityStatus",
                table: "Meanings");

            migrationBuilder.DropIndex(
                name: "IX_Meanings_SourceProvider",
                table: "Meanings");

            migrationBuilder.DropIndex(
                name: "IX_LearningItems_ContentSource",
                table: "LearningItems");

            migrationBuilder.DropIndex(
                name: "IX_LearningItems_ExternalSourceKey",
                table: "LearningItems");

            migrationBuilder.DropIndex(
                name: "IX_LearningItems_QualityStatus",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "ContentSource",
                table: "SentenceTranslations");

            migrationBuilder.DropColumn(
                name: "QualityStatus",
                table: "SentenceTranslations");

            migrationBuilder.DropColumn(
                name: "ContentSource",
                table: "Sentences");

            migrationBuilder.DropColumn(
                name: "QualityStatus",
                table: "Sentences");

            migrationBuilder.DropColumn(
                name: "ContentSource",
                table: "Meanings");

            migrationBuilder.DropColumn(
                name: "License",
                table: "Meanings");

            migrationBuilder.DropColumn(
                name: "QualityStatus",
                table: "Meanings");

            migrationBuilder.DropColumn(
                name: "SourceProvider",
                table: "Meanings");

            migrationBuilder.DropColumn(
                name: "ContentSource",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "ExternalSourceKey",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "ImportedAt",
                table: "LearningItems");

            migrationBuilder.DropColumn(
                name: "QualityStatus",
                table: "LearningItems");
        }
    }
}
