using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordix.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeycloakUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeckItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserLearningItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckItems_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckItems_UserLearningItems_UserLearningItemId",
                        column: x => x.UserLearningItemId,
                        principalTable: "UserLearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeckItems_DeckId",
                table: "DeckItems",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckItems_DeckId_UserLearningItemId",
                table: "DeckItems",
                columns: new[] { "DeckId", "UserLearningItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeckItems_UserLearningItemId",
                table: "DeckItems",
                column: "UserLearningItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_IsActive",
                table: "Decks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_KeycloakUserId",
                table: "Decks",
                column: "KeycloakUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_KeycloakUserId_NormalizedName",
                table: "Decks",
                columns: new[] { "KeycloakUserId", "NormalizedName" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeckItems");

            migrationBuilder.DropTable(
                name: "Decks");
        }
    }
}
