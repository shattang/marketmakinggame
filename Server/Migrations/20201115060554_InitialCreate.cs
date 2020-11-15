using Microsoft.EntityFrameworkCore.Migrations;

namespace MarketMakingGame.Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CardId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardImageUrl = table.Column<string>(nullable: true),
                    CardDescription = table.Column<string>(nullable: true),
                    CardValue = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CardId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<string>(type: "char", unicode: false, fixedLength: true, maxLength: 22, nullable: false),
                    GameName = table.Column<string>(nullable: true),
                    NumberOfRounds = table.Column<int>(nullable: true),
                    MinQuoteWidth = table.Column<double>(nullable: true),
                    MaxQuoteWidth = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "char", unicode: false, fixedLength: true, maxLength: 22, nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    AvatarSeed = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "GameStates",
                columns: table => new
                {
                    GameStateId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsFinished = table.Column<bool>(nullable: false),
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
                    GameStateId = table.Column<int>(nullable: false),
                    PlayerCardCardId = table.Column<int>(nullable: false),
                    CurrentBid = table.Column<double>(nullable: true),
                    CurrentAsk = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStates", x => x.PlayerStateId);
                    table.ForeignKey(
                        name: "FK_PlayerStates_GameStates_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameStates",
                        principalColumn: "GameStateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerStates_Cards_PlayerCardCardId",
                        column: x => x.PlayerCardCardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
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
                    GameStateId = table.Column<int>(nullable: false),
                    CommunityCardCardId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundStates", x => x.RoundStateId);
                    table.ForeignKey(
                        name: "FK_RoundStates_Cards_CommunityCardCardId",
                        column: x => x.CommunityCardCardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoundStates_GameStates_GameStateId",
                        column: x => x.GameStateId,
                        principalTable: "GameStates",
                        principalColumn: "GameStateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    TradeId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameStateId = table.Column<int>(nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 1, "Ace of Diamonds", "/images/cards/ace_of_diamonds.svg", -1.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 29, "3 of Spades", "/images/cards/3_of_spades.svg", 3.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 30, "4 of Spades", "/images/cards/4_of_spades.svg", 4.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 31, "5 of Spades", "/images/cards/5_of_spades.svg", 5.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 32, "6 of Spades", "/images/cards/6_of_spades.svg", 6.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 33, "7 of Spades", "/images/cards/7_of_spades.svg", 7.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 34, "8 of Spades", "/images/cards/8_of_spades.svg", 8.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 35, "9 of Spades", "/images/cards/9_of_spades.svg", 9.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 36, "10 of Spades", "/images/cards/10_of_spades.svg", 10.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 37, "Jack of Spades", "/images/cards/jack_of_spades.svg", 11.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 38, "Queen of Spades", "/images/cards/queen_of_spades.svg", 12.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 39, "King of Spades", "/images/cards/king_of_spades.svg", 13.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 40, "Ace of Clubs", "/images/cards/ace_of_clubs.svg", 1.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 41, "2 of Clubs", "/images/cards/2_of_clubs.svg", 2.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 42, "3 of Clubs", "/images/cards/3_of_clubs.svg", 3.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 43, "4 of Clubs", "/images/cards/4_of_clubs.svg", 4.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 44, "5 of Clubs", "/images/cards/5_of_clubs.svg", 5.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 45, "6 of Clubs", "/images/cards/6_of_clubs.svg", 6.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 46, "7 of Clubs", "/images/cards/7_of_clubs.svg", 7.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 47, "8 of Clubs", "/images/cards/8_of_clubs.svg", 8.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 48, "9 of Clubs", "/images/cards/9_of_clubs.svg", 9.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 49, "10 of Clubs", "/images/cards/10_of_clubs.svg", 10.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 50, "Jack of Clubs", "/images/cards/jack_of_clubs.svg", 11.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 51, "Queen of Clubs", "/images/cards/queen_of_clubs.svg", 12.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 28, "2 of Spades", "/images/cards/2_of_spades.svg", 2.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 52, "King of Clubs", "/images/cards/king_of_clubs.svg", 13.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 27, "Ace of Spades", "/images/cards/ace_of_spades.svg", 1.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 25, "Queen of Hearts", "/images/cards/queen_of_hearts.svg", -12.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 2, "2 of Diamonds", "/images/cards/2_of_diamonds.svg", -2.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 3, "3 of Diamonds", "/images/cards/3_of_diamonds.svg", -3.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 4, "4 of Diamonds", "/images/cards/4_of_diamonds.svg", -4.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 5, "5 of Diamonds", "/images/cards/5_of_diamonds.svg", -5.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 6, "6 of Diamonds", "/images/cards/6_of_diamonds.svg", -6.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 7, "7 of Diamonds", "/images/cards/7_of_diamonds.svg", -7.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 8, "8 of Diamonds", "/images/cards/8_of_diamonds.svg", -8.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 9, "9 of Diamonds", "/images/cards/9_of_diamonds.svg", -9.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 10, "10 of Diamonds", "/images/cards/10_of_diamonds.svg", -10.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 11, "Jack of Diamonds", "/images/cards/jack_of_diamonds.svg", -11.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 12, "Queen of Diamonds", "/images/cards/queen_of_diamonds.svg", -12.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 13, "King of Diamonds", "/images/cards/king_of_diamonds.svg", -13.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 14, "Ace of Hearts", "/images/cards/ace_of_hearts.svg", -1.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 15, "2 of Hearts", "/images/cards/2_of_hearts.svg", -2.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 16, "3 of Hearts", "/images/cards/3_of_hearts.svg", -3.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 17, "4 of Hearts", "/images/cards/4_of_hearts.svg", -4.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 18, "5 of Hearts", "/images/cards/5_of_hearts.svg", -5.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 19, "6 of Hearts", "/images/cards/6_of_hearts.svg", -6.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 20, "7 of Hearts", "/images/cards/7_of_hearts.svg", -7.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 21, "8 of Hearts", "/images/cards/8_of_hearts.svg", -8.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 22, "9 of Hearts", "/images/cards/9_of_hearts.svg", -9.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 23, "10 of Hearts", "/images/cards/10_of_hearts.svg", -10.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 24, "Jack of Hearts", "/images/cards/jack_of_hearts.svg", -11.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 26, "King of Hearts", "/images/cards/king_of_hearts.svg", -13.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 53, "Unopened", "/images/cards/back_card_red.svg", 0.0 });

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_PlayerId",
                table: "GameStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_GameId_PlayerId",
                table: "GameStates",
                columns: new[] { "GameId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStates_PlayerCardCardId",
                table: "PlayerStates",
                column: "PlayerCardCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStates_PlayerId",
                table: "PlayerStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStates_GameStateId_PlayerId",
                table: "PlayerStates",
                columns: new[] { "GameStateId", "PlayerId" },
                unique: true);

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
                name: "Cards");

            migrationBuilder.DropTable(
                name: "GameStates");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
