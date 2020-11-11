using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Server.Data;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Lib
{
  public class GameService
  {
    private const int STATE_CREATED = 0, STATE_INITIALIZING = 1, STATE_INITIALIZED = 2;
    private volatile int state = STATE_CREATED;

    private readonly GameDbContext _dbContext;

    public Dictionary<string, Game> Games { get; }

    public Card UnopenedCard { get; private set; }
    public List<Card> Cards { get; private set; }

    public GameService(GameDbContext dbContext)
    {
      _dbContext = dbContext ?? new GameDbContext();
      Games = new Dictionary<string, Game>();
    }

    public async Task InitializeAsync()
    {
      if (Interlocked.CompareExchange(ref state, STATE_INITIALIZING, STATE_CREATED) != STATE_CREATED)
        return;
      
      await foreach(var card in _dbContext.Cards.AsAsyncEnumerable())
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