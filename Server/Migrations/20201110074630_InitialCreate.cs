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
                    GameId = table.Column<string>(nullable: false),
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
                    PlayerId = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    AvatarSeed = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
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
                values: new object[] { 28, "2 of Spades", "/images/cards/2_of_spades.svg", 2.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 39, "King of Spades", "/images/cards/king_of_spades.svg", 13.0 });

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
                values: new object[] { 40, "Ace of Clubs", "/images/cards/ace_of_clubs.svg", 1.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 27, "Ace of Spades", "/images/cards/ace_of_spades.svg", 1.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 26, "King of Hearts", "/images/cards/king_of_hearts.svg", -13.0 });

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
                values: new object[] { 51, "Queen of Clubs", "/images/cards/queen_of_clubs.svg", 12.0 });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "CardId", "CardDescription", "CardImageUrl", "CardValue" },
                values: new object[] { 52, "King of Clubs", "/images/cards/king_of_clubs.svg", 13.0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
