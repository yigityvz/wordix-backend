using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportJobsProviderLogsAndExternalCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalContentCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CacheKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedInput = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceLanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TargetLanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CachedPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentSource = table.Column<int>(type: "int", nullable: false),
                    QualityStatus = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HitCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalContentCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SourceVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TriggeredByKeycloakUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DryRun = table.Column<bool>(type: "bit", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ProcessedRows = table.Column<int>(type: "int", nullable: false),
                    CreatedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SummaryMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderRequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KeycloakUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NormalizedInput = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceLanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TargetLanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WasServedFromCache = table.Column<bool>(type: "bit", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderRequestLogs_ImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalTable: "ImportJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProviderRequestLogs_LearningItems_LearningItemId",
                        column: x => x.LearningItemId,
                        principalTable: "LearningItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_CacheKey",
                table: "ExternalContentCaches",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_ExpiresAt",
                table: "ExternalContentCaches",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_HitCount",
                table: "ExternalContentCaches",
                column: "HitCount");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_NormalizedInput",
                table: "ExternalContentCaches",
                column: "NormalizedInput");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_ProviderName",
                table: "ExternalContentCaches",
                column: "ProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_ProviderName_OperationName_CacheKey",
                table: "ExternalContentCaches",
                columns: new[] { "ProviderName", "OperationName", "CacheKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_ProviderType",
                table: "ExternalContentCaches",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_SourceLanguageCode_TargetLanguageCode",
                table: "ExternalContentCaches",
                columns: new[] { "SourceLanguageCode", "TargetLanguageCode" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalContentCaches_Status",
                table: "ExternalContentCaches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_CreatedAt",
                table: "ImportJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_DryRun",
                table: "ImportJobs",
                column: "DryRun");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_JobType",
                table: "ImportJobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_JobType_Status_CreatedAt",
                table: "ImportJobs",
                columns: new[] { "JobType", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_SourceName",
                table: "ImportJobs",
                column: "SourceName");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_Status",
                table: "ImportJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_TriggeredByKeycloakUserId",
                table: "ImportJobs",
                column: "TriggeredByKeycloakUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_CreatedAt",
                table: "ProviderRequestLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_HttpStatusCode",
                table: "ProviderRequestLogs",
                column: "HttpStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_ImportJobId",
                table: "ProviderRequestLogs",
                column: "ImportJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_KeycloakUserId",
                table: "ProviderRequestLogs",
                column: "KeycloakUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_LearningItemId",
                table: "ProviderRequestLogs",
                column: "LearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_ProviderName",
                table: "ProviderRequestLogs",
                column: "ProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_ProviderName_OperationName_Status_CreatedAt",
                table: "ProviderRequestLogs",
                columns: new[] { "ProviderName", "OperationName", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_ProviderType",
                table: "ProviderRequestLogs",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_RequestKey",
                table: "ProviderRequestLogs",
                column: "RequestKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_Status",
                table: "ProviderRequestLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestLogs_WasServedFromCache",
                table: "ProviderRequestLogs",
                column: "WasServedFromCache");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalContentCaches");

            migrationBuilder.DropTable(
                name: "ProviderRequestLogs");

            migrationBuilder.DropTable(
                name: "ImportJobs");
        }
    }
}
