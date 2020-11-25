using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Server.Data;
using MarketMakingGame.Server.Models;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Lib
{
  public class CardRepository
  {
    public List<Card> Cards { get; set; }

    public Card UnopenedCard { get; set; }

    public ConcurrentDictionary<int, CardDeck> GameStateToCardDeck { get; set; }

    public CardRepository()
    {
      Cards = new List<Card>();
      GameStateToCardDeck = new ConcurrentDictionary<int, CardDeck>();
    }

    public void Initialize()
    {
      Cards.Clear();
      using (var dbContext = new GameDbContext())
      {
        foreach (var card in dbContext.Cards)
        {
          if (Math.Abs(card.CardValue) < 1E-6)
          {
            UnopenedCard = card;
          }
          else
          {
            Cards.Add(card);
          }
        }
      }
    }
  }
}