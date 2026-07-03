using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizRecommendationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecommendationReason = table.Column<int>(type: "int", nullable: false),
                    DifficultyGroup = table.Column<int>(type: "int", nullable: false),
                    WasAnsweredCorrectly = table.Column<bool>(type: "bit", nullable: true),
                    WasAddedToDictionary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizRecommendationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizRecommendationItems_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizRecommendationItems_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizRecommendationItems_QuizSessions_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SearchSuggestionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeycloakUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuizRecommendationItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SuggestionReason = table.Column<int>(type: "int", nullable: false),
                    WasAccepted = table.Column<bool>(type: "bit", nullable: false),
                    WasSaved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchSuggestionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchSuggestionLogs_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SearchSuggestionLogs_QuizRecommendationItems_QuizRecommendationItemId",
                        column: x => x.QuizRecommendationItemId,
                        principalTable: "QuizRecommendationItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SearchSuggestionLogs_QuizSessions_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizRecommendationItems_LearningItemId",
                table: "QuizRecommendationItems",
                column: "LearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizRecommendationItems_QuizQuestionId",
                table: "QuizRecommendationItems",
                column: "QuizQuestionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizRecommendationItems_QuizSessionId",
                table: "QuizRecommendationItems",
                column: "QuizSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchSuggestionLogs_KeycloakUserId",
                table: "SearchSuggestionLogs",
                column: "KeycloakUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchSuggestionLogs_KeycloakUserId_CreatedAt",
                table: "SearchSuggestionLogs",
                columns: new[] { "KeycloakUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchSuggestionLogs_LearningItemId",
                table: "SearchSuggestionLogs",
                column: "LearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchSuggestionLogs_QuizRecommendationItemId",
                table: "SearchSuggestionLogs",
                column: "QuizRecommendationItemId",
                unique: true,
                filter: "[QuizRecommendationItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SearchSuggestionLogs_QuizSessionId",
                table: "SearchSuggestionLogs",
                column: "QuizSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchSuggestionLogs");

            migrationBuilder.DropTable(
                name: "QuizRecommendationItems");
        }
    }
}
