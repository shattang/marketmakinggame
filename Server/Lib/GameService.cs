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
  public class GameService : IDisposable
  {
    private readonly ILoggerProvider _loggerProvider;
    private readonly ILogger _logger;
    internal GameDbContext DBContext { get; }

    private ConcurrentDictionary<string, GameEngine> GameEngines { get; }

    internal Card UnopenedCard { get; set; }

    internal List<Card> Cards { get; set; }

    public event Action<string, BaseResponse> OnGameUpdate;

    public event Action<string, string, BaseResponse> OnPlayerUpdate;

    public GameService(ILoggerProvider loggerProvider)
    {
      _loggerProvider = loggerProvider;
      _logger = loggerProvider.CreateLogger(nameof(GameService));
      DBContext = new GameDbContext();
      GameEngines = new ConcurrentDictionary<string, GameEngine>();
      Cards = new List<Card>();
    }

    public void Initialize()
    {
      _logger.LogInformation("Initializing");
      foreach (var card in DBContext.Cards)
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
      _logger.LogInformation("Initialized");
    }

    public GetGameInfoResponse GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("GetGameInfo {}", request);

      var lookup = DBContext.Games
        .Where(x => request.GameIds.Contains(x.GameId)).ToList();

      return new GetGameInfoResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        Games = lookup
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

    public async Task<CreateGameResponse> CreateGame(CreateGameRequest request)
    {
      _logger.LogInformation("CreateGame: Request={}", request);
      var resp = new CreateGameResponse() { RequestId = request.RequestId };
      
      if (String.IsNullOrWhiteSpace(request.Game.GameName))
      {
        resp.ErrorMessage = "Invalid GameName";
        return resp;
      }

      var gameEngine = new GameEngine(_loggerProvider, this);
      try
      {
        await gameEngine.InitializeAsync(request);
        GameEngines[gameEngine.Game.GameId] = gameEngine;
        resp.IsSuccess = true;
        resp.GameId = gameEngine.Game.GameId;
      }
      catch (Exception ex)
      {
        resp.ErrorMessage = ex.Message;
      }

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

      var game = GameEngines.GetValueOrDefault(request.GameId);
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

    public void Dispose()
    {
      DBContext.Dispose();
    }
  }
}