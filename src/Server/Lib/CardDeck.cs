using System;
using System.Collections.Generic;
using System.Linq;
using MarketMakingGame.Server.Data;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Server.Models;

namespace MarketMakingGame.Server.Lib
{
  public class CardDeck
  {
    private string Hash { get; set; }
    private Bag<int> Deck { get; set; }
    private CardRepository Repository { get; }

    public CardDeck(CardRepository repository)
    {
      this.Repository = repository;
      Hash = Guid.NewGuid().ToBase62();
    }

    public string RebuildDeck(GameState gameState)
    {
      var minCardsNeeded = (gameState.Game.NumberOfRounds ?? 0) + gameState.PlayerStates.Count;
      var minDecksNeeded = Math.Max(minCardsNeeded, 1) / Repository.Cards.Count + 1;
      var dealtCards = gameState.RoundStates
        .Select(x => x.CommunityCard)
        .Concat(gameState.PlayerStates.Select(x => x.PlayerCard))
        .Where(x => x != null && x.CardId != Repository.UnopenedCard.CardId)
        .Select(x => x.CardId);
      Deck = new Bag<int>(Repository.Cards.Select(x => x.CardId), dealtCards,
        Repository.UnopenedCard.CardId, minDecksNeeded);
      Hash = Guid.NewGuid().ToBase62();
      return Hash;
    }

    public bool NeedsRebuild(string knownHash)
    {
      return Deck == null || knownHash == null || Hash != knownHash;
    }

    public (string Hash, int CardId) PickCard(string knownHash)
    {
      if (NeedsRebuild(knownHash))
        throw new InvalidOperationException("Deck needs to rebuild");
      
      var ret = Deck.Draw();
      Hash = Guid.NewGuid().ToBase62();
      return (Hash, ret);
    }

    public override string ToString()
    {
      return $"CardDeck(Deck={Deck})";
    }
  }
}