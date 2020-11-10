using Microsoft.EntityFrameworkCore.Migrations;

namespace MarketMakingGame.Server.Migrations
{
    public partial class AddStateTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameStates",
                columns: table => new
                {
                    GameStateId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<string>(nullable: true),
                    PlayerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStates", x => x.GameStateId);
                    table.ForeignKey(
                        name: "FK_GameStates_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStates",
                columns: table => new
                {
                    PlayerStateId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerId = table.Column<string>(nullable: true),
                    GameStateId = table.Column<int>(nullable: true),
                    PlayerCardCardId = table.Column<int>(nullable: true),
                    CurrentBid = table.Column<double>(nullable: false),
                    CurrentAsk = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStates", x => x.PlayerStateId);
                    table.ForeignKey(
                        name: "FK_PlayerStates_GameStates_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameStates",
                        principalColumn: "GameStateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerStates_Cards_PlayerCardCardId",
                        column: x => x.PlayerCardCardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoundStates",
                columns: table => new
                {
                    RoundStateId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameStateId = table.Column<int>(nullable: true),
                    CommunityCardCardId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundStates", x => x.RoundStateId);
                    table.ForeignKey(
                        name: "FK_RoundStates_Cards_CommunityCardCardId",
                        column: x => x.CommunityCardCardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoundStates_GameStates_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameStates",
                        principalColumn: "GameStateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    TradeId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameStateId = table.Column<int>(nullable: true),
                    InitiatingPlayerPlayerId = table.Column<string>(nullable: true),
                    TargetPlayerPlayerId = table.Column<string>(nullable: true),
                    IsBuy = table.Column<bool>(nullable: false),
                    TradePrice = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.TradeId);
                    table.ForeignKey(
                        name: "FK_Trades_GameStates_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameStates",
                        principalColumn: "GameStateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trades_Players_InitiatingPlayerPlayerId",
                        column: x => x.InitiatingPlayerPlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trades_Players_TargetPlayerPlayerId",
                        column: x => x.TargetPlayerPlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_GameId",
                table: "GameStates",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_PlayerId",
                table: "GameStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStates_GameStateId",
                table: "PlayerStates",
                column: "GameStateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStates_PlayerCardCardId",
                table: "PlayerStates",
                column: "PlayerCardCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStates_PlayerId",
                table: "PlayerStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundStates_CommunityCardCardId",
                table: "RoundStates",
                column: "CommunityCardCardId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundStates_GameStateId",
                table: "RoundStates",
                column: "GameStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_GameStateId",
                table: "Trades",
                column: "GameStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InitiatingPlayerPlayerId",
                table: "Trades",
                column: "InitiatingPlayerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TargetPlayerPlayerId",
                table: "Trades",
                column: "TargetPlayerPlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerStates");

            migrationBuilder.DropTable(
                name: "RoundStates");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "GameStates");
        }
    }
}
