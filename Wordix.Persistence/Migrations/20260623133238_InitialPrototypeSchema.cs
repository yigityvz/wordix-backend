using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPrototypeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CefrLevel = table.Column<int>(type: "int", nullable: false),
                    DifficultyGroup = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningItems_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeycloakUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    NativeLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Languages_NativeLanguageId",
                        column: x => x.NativeLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Languages_TargetLanguageId",
                        column: x => x.TargetLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Meanings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeaningText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ShortDefinition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meanings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meanings_Languages_TargetLanguageId",
                        column: x => x.TargetLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Meanings_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Pronunciation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Words_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LookupHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueryText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedQueryText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    SourceLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WasFoundInDatabase = table.Column<bool>(type: "bit", nullable: false),
                    WasCreatedFromProvider = table.Column<bool>(type: "bit", nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResultCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupHistories_Languages_SourceLanguageId",
                        column: x => x.SourceLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LookupHistories_Languages_TargetLanguageId",
                        column: x => x.TargetLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LookupHistories_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LookupHistories_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizType = table.Column<int>(type: "int", nullable: false),
                    QuizSourceType = table.Column<int>(type: "int", nullable: false),
                    QuizContentMode = table.Column<int>(type: "int", nullable: false),
                    DifficultyGroup = table.Column<int>(type: "int", nullable: false),
                    DeckId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncludeSystemRecommendations = table.Column<bool>(type: "bit", nullable: false),
                    QuestionCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessions_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultQuizType = table.Column<int>(type: "int", nullable: false),
                    DefaultDifficultyGroup = table.Column<int>(type: "int", nullable: false),
                    IncludeSystemRecommendations = table.Column<bool>(type: "bit", nullable: false),
                    MotivationMessagesEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreferredQuestionCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLearningItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedMeaningId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceLookupHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningItems_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLearningItems_LookupHistories_SourceLookupHistoryId",
                        column: x => x.SourceLookupHistoryId,
                        principalTable: "LookupHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLearningItems_Meanings_SelectedMeaningId",
                        column: x => x.SelectedMeaningId,
                        principalTable: "Meanings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLearningItems_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystemRecommended = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_QuizSessions_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLearningProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserLearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningStatus = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    WrongCount = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveCorrectCount = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveWrongCount = table.Column<int>(type: "int", nullable: false),
                    LearningConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    RepetitionLevel = table.Column<int>(type: "int", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLearningProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLearningProgresses_UserLearningItems_UserLearningItemId",
                        column: x => x.UserLearningItemId,
                        principalTable: "UserLearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizOptions_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningProgressHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserLearningProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldLearningStatus = table.Column<int>(type: "int", nullable: false),
                    NewLearningStatus = table.Column<int>(type: "int", nullable: false),
                    OldConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    NewConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningProgressHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningProgressHistories_UserLearningProgresses_UserLearningProgressId",
                        column: x => x.UserLearningProgressId,
                        principalTable: "UserLearningProgresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedQuizOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserAnswer = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnswerResult = table.Column<int>(type: "int", nullable: false),
                    ResponseTimeMilliseconds = table.Column<int>(type: "int", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedToDictionaryBecauseWrong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_QuizOptions_SelectedQuizOptionId",
                        column: x => x.SelectedQuizOptionId,
                        principalTable: "QuizOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAnswers_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Code", "CreatedAt", "IsActive", "Name", "NativeName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "en", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "English", "English", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "tr", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Turkish", "Türkçe", null }
                });

            migrationBuilder.InsertData(
                table: "LearningItems",
                columns: new[] { "Id", "CefrLevel", "CreatedAt", "DifficultyGroup", "IsActive", "ItemType", "LanguageId", "SourceType", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, 1, new Guid("11111111-1111-1111-1111-111111111111"), 1, null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, 1, new Guid("11111111-1111-1111-1111-111111111111"), 1, null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, 1, new Guid("11111111-1111-1111-1111-111111111111"), 1, null }
                });

            migrationBuilder.InsertData(
                table: "Meanings",
                columns: new[] { "Id", "Category", "CreatedAt", "DisplayOrder", "IsPrimary", "LearningItemId", "MeaningText", "PartOfSpeech", "ShortDefinition", "TargetLanguageId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"), "general", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "başarmak", "verb", "Bir hedefe ulaşmak veya istenen sonucu elde etmek.", new Guid("22222222-2222-2222-2222-222222222222"), null },
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc2"), "general", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), "mükemmel", "adjective", "Eksiksiz, kusursuz veya çok iyi olan.", new Guid("22222222-2222-2222-2222-222222222222"), null },
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc3"), "general", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), "geliştirmek", "verb", "Bir şeyi daha iyi hale getirmek.", new Guid("22222222-2222-2222-2222-222222222222"), null }
                });

            migrationBuilder.InsertData(
                table: "Words",
                columns: new[] { "Id", "CreatedAt", "LearningItemId", "NormalizedText", "PartOfSpeech", "Pronunciation", "Text", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "achieve", "verb", null, "achieve", null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), "perfect", "adjective", null, "perfect", null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), "improve", "verb", null, "improve", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_IsActive",
                table: "Languages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_DifficultyGroup",
                table: "LearningItems",
                column: "DifficultyGroup");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_IsActive",
                table: "LearningItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LearningItems_LanguageId_ItemType",
                table: "LearningItems",
                columns: new[] { "LanguageId", "ItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningProgressHistories_UserLearningProgressId_CreatedAt",
                table: "LearningProgressHistories",
                columns: new[] { "UserLearningProgressId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_InputType",
                table: "LookupHistories",
                column: "InputType");

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_LearningItemId",
                table: "LookupHistories",
                column: "LearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_NormalizedQueryText",
                table: "LookupHistories",
                column: "NormalizedQueryText");

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_ProviderType",
                table: "LookupHistories",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_SourceLanguageId",
                table: "LookupHistories",
                column: "SourceLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_TargetLanguageId",
                table: "LookupHistories",
                column: "TargetLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupHistories_UserProfileId",
                table: "LookupHistories",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Meanings_LearningItemId_TargetLanguageId_DisplayOrder",
                table: "Meanings",
                columns: new[] { "LearningItemId", "TargetLanguageId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Meanings_LearningItemId_TargetLanguageId_IsPrimary",
                table: "Meanings",
                columns: new[] { "LearningItemId", "TargetLanguageId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Meanings_TargetLanguageId",
                table: "Meanings",
                column: "TargetLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_AnswerResult",
                table: "QuizAnswers",
                column: "AnswerResult");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuizQuestionId",
                table: "QuizAnswers",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_SelectedQuizOptionId",
                table: "QuizAnswers",
                column: "SelectedQuizOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_UserProfileId",
                table: "QuizAnswers",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_UserProfileId_AnsweredAt",
                table: "QuizAnswers",
                columns: new[] { "UserProfileId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionId_DisplayOrder",
                table: "QuizOptions",
                columns: new[] { "QuizQuestionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionId_IsCorrect",
                table: "QuizOptions",
                columns: new[] { "QuizQuestionId", "IsCorrect" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_LearningItemId",
                table: "QuizQuestions",
                column: "LearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizSessionId_DisplayOrder",
                table: "QuizQuestions",
                columns: new[] { "QuizSessionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_Status",
                table: "QuizSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserProfileId",
                table: "QuizSessions",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserProfileId_StartedAt",
                table: "QuizSessions",
                columns: new[] { "UserProfileId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningItems_LearningItemId",
                table: "UserLearningItems",
                column: "LearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningItems_SelectedMeaningId",
                table: "UserLearningItems",
                column: "SelectedMeaningId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningItems_SourceLookupHistoryId",
                table: "UserLearningItems",
                column: "SourceLookupHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningItems_UserProfileId_IsActive",
                table: "UserLearningItems",
                columns: new[] { "UserProfileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningItems_UserProfileId_LearningItemId",
                table: "UserLearningItems",
                columns: new[] { "UserProfileId", "LearningItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningProgresses_LearningStatus",
                table: "UserLearningProgresses",
                column: "LearningStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningProgresses_NextReviewDate",
                table: "UserLearningProgresses",
                column: "NextReviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserLearningProgresses_UserLearningItemId",
                table: "UserLearningProgresses",
                column: "UserLearningItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserProfileId",
                table: "UserPreferences",
                column: "UserProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Email",
                table: "UserProfiles",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_KeycloakUserId",
                table: "UserProfiles",
                column: "KeycloakUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_NativeLanguageId",
                table: "UserProfiles",
                column: "NativeLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_TargetLanguageId",
                table: "UserProfiles",
                column: "TargetLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Username",
                table: "UserProfiles",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_Words_LearningItemId",
                table: "Words",
                column: "LearningItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Words_NormalizedText",
                table: "Words",
                column: "NormalizedText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningProgressHistories");

            migrationBuilder.DropTable(
                name: "QuizAnswers");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "UserLearningProgresses");

            migrationBuilder.DropTable(
                name: "QuizOptions");

            migrationBuilder.DropTable(
                name: "UserLearningItems");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "LookupHistories");

            migrationBuilder.DropTable(
                name: "Meanings");

            migrationBuilder.DropTable(
                name: "QuizSessions");

            migrationBuilder.DropTable(
                name: "LearningItems");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
