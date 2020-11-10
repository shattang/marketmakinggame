using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Server.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;

namespace MarketMakingGame.Server.Data
{
  public class CardConfiguration : IEntityTypeConfiguration<Card>
  {
    public void Configure(EntityTypeBuilder<Card> builder)
    {
      builder.ToTable("Cards");

      var faceCards = new Dictionary<int, string>()
      {
        {1, "Ace"},
        {11, "Jack"},
        {12, "Queen"},
        {13, "King"}
      };

      var counter = 0;
      var cards = new List<Card>();
      foreach (var suit in new[] { ("Diamonds", -1), ("Hearts", -1), ("Spades", 1), ("Clubs", 1) })
      {
        foreach (var value in Enumerable.Range(1, 13))
        {
          var cardFace = (value > 1 && value < 11) ? value.ToString() : faceCards[value];
          var card = new Card()
          {
            CardId = ++counter,
            CardDescription = $"{cardFace} of {suit.Item1}",
            CardImageUrl = $"/images/cards/{cardFace.ToLower()}_of_{suit.Item1.ToLower()}.svg",
            CardValue = suit.Item2 * value
          };
          cards.Add(card);
        }
      }

      builder.HasData(cards.ToArray());
    }
  }
}