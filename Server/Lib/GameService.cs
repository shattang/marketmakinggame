using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Server.Data;
using MarketMakingGame.Shared.Models;
using Microsoft.Extensions.Logging;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Lib;
using System.Collections.Concurrent;

namespace MarketMakingGame.Server.Lib
{
  public class GameService
  {
    private readonly ILogger<GameService> _logger;
    private readonly GameDbContext _dbContext;

    public ConcurrentDictionary<string, GameEngine> Games { get; }

    public Card UnopenedCard { get; private set; }

    public List<Card> Cards { get; private set; }

    public event Action<string, BaseResponse> OnGameUpdate;

    public event Action<string ,string, BaseResponse> OnPlayerUpdate;

    public GameService(ILogger<GameService> logger)
    {
      _logger = logger;
      _dbContext = new GameDbContext();
      Games = new ConcurrentDictionary<string, GameEngine>();
      Cards = new List<Card>();
    }

    public void Initialize()
    {
      _logger.LogInformation("Loading cards");
      foreach (var card in _dbContext.Cards)
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
      _logger.LogInformation($"Cards Loaded {UnopenedCard} {Cards.Count}");
    }

    public GetGameInfoResponse GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("GetGameInfo {}", request);

      var lists = request.GameIds
        .Select(x => Games.GetValueOrDefault(x, null))
        .Where(x => x != null).Select(x => x.Game).ToList();

      return new GetGameInfoResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        Games = lists
      };
    }

    public GetCardsResponse GetCards(GetCardsRequest request)
    {
      _logger.LogInformation("GetCards {}", request);

      return new GetCardsResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        Cards = Cards,
        UnopenedCard = UnopenedCard
      };
    }

    public CreateGameResponse CreateGame(CreateGameRequest request)
    {
      _logger.LogInformation("CreateGame {}", request);

      var resp = new CreateGameResponse() { RequestId = request.RequestId };

      if (String.IsNullOrWhiteSpace(request.Game.GameName))
      {
        resp.ErrorMessage = "Invalid GameName";
        return resp;
      }

      var gameInfo = request.Game;
      gameInfo.GameId = Guid.NewGuid().ToBase62();
      Games[gameInfo.GameId] = new GameEngine(gameInfo);

      resp.IsSuccess = true;
      resp.GameId = gameInfo.GameId;
      return resp;
    }

    public JoinGameResponse JoinGame(JoinGameRequest request)
    {
      _logger.LogInformation("JoinGame {}", request);

      var resp = new JoinGameResponse() { RequestId = request.RequestId };

      if (String.IsNullOrWhiteSpace(request.GameId))
      {
        resp.ErrorMessage = "Invalid GameId";
        return resp;
      }

      var game = Games.GetValueOrDefault(request.GameId);
      if (game == null)
      {
        resp.ErrorMessage = "Game not found";
        return resp;
      }

      resp.IsSuccess = true;
      //TODO
      return resp;
    }

    private void InvokeOnGameUpdate(string gameId, BaseResponse response)
    {
      if (OnGameUpdate != null)
      {
        OnGameUpdate(gameId, response);
      }
    }

    private void InvokeOnGameUpdate(string gameId, string playerId, BaseResponse response)
    {
      if (OnPlayerUpdate != null)
      {
        OnPlayerUpdate(gameId, playerId, response);
      }
    }
  }
}