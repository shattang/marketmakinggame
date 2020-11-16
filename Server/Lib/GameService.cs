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
using Microsoft.EntityFrameworkCore;

namespace MarketMakingGame.Server.Lib
{
  public class GameService : IDisposable
  {
    private const int MAX_GAMES_PER_USER = 2;
    private readonly ILoggerProvider _loggerProvider;
    private readonly ILogger _logger;
    internal GameDbContext DBContext { get; }

    private ConcurrentDictionary<string, GameEngine> GameEngines { get; }

    internal Card UnopenedCard { get; set; }

    internal List<Card> Cards { get; set; }

    public event Action<GameUpdateResponse> OnGameUpdate;

    public event Action<PlayerUpdateResponse> OnPlayerUpdate;

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

    public async Task CreateGameAsync(CreateGameRequest request, Func<CreateGameResponse, Task> responseHandler)
    {
      _logger.LogInformation("CreateGame: Request={}", request);
      var resp = new CreateGameResponse() { RequestId = request.RequestId };

      if (String.IsNullOrWhiteSpace(request.Game.GameName))
      {
        resp.ErrorMessage = "Invalid GameName";
        await responseHandler(resp);
        return;
      }

      var numPlayerGames = DBContext.GameStates
        .Count(x => x.PlayerId == request.Player.PlayerId && !x.IsFinished);
      _logger.LogInformation($"numPlayerGames={numPlayerGames}");
      if (numPlayerGames > MAX_GAMES_PER_USER)
      {
        resp.ErrorMessage = "Too many active games for player";
        await responseHandler(resp);
        return;
      }

      var gameEngine = new GameEngine(_loggerProvider, this);
      var playerState = await gameEngine.CreateGameAsync(request);
      GameEngines[gameEngine.GameState.GameId] = gameEngine;
      resp.IsSuccess = true;
      resp.GameId = gameEngine.GameState.GameId;
      await responseHandler(resp);

      InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
      InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
    }

    public async Task JoinGameAsync(JoinGameRequest request, Func<JoinGameResponse, Task> responseHandler)
    {
      _logger.LogInformation("JoinGame {}", request);

      var resp = new JoinGameResponse() { RequestId = request.RequestId };

      if (String.IsNullOrWhiteSpace(request.GameId))
      {
        resp.ErrorMessage = "Invalid GameId";
        await responseHandler(resp);
        return;
      }

      var gameEngine = GameEngines.GetValueOrDefault(request.GameId);
      if (gameEngine == null)
      {
        var gameState = DBContext.GameStates
          .FirstOrDefault(x => x.PlayerId == request.Player.PlayerId && x.GameId == request.GameId);
        if (gameState == null)
        {
          resp.ErrorMessage = "GameId not found";
          await responseHandler(resp);
          return;
        }

        gameEngine = new GameEngine(_loggerProvider, this, gameState);
      }

      var playerState = await gameEngine.JoinGameAsync(request);
      resp.IsSuccess = true;
      resp.Game = gameEngine.GameState.Game;
      resp.PlayerPublicId = playerState.PlayerStateId;
      await responseHandler(resp);

      InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
      InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
    }

    public async Task DealPlayerCards(DealPlayerCardsRequest request, Func<BaseResponse, Task> responseHandler)
    {
      _logger.LogInformation("DealPlayerCards {}", request);
      
      var resp = new BaseResponse() { RequestId = request.RequestId };

      var gameEngine = GameEngines.GetValueOrDefault(request.GameId);
      if (gameEngine == null)
      {
        resp.ErrorMessage = "GameId not found";
        await responseHandler(resp);
        return;
      }

      if (gameEngine.GameState.PlayerId != request.PlayerId)
      {
        resp.ErrorMessage = "Only dealer can initiate this request";
        await responseHandler(resp);
        return;
      }

      var changed = await gameEngine.DealPlayerCards();
      resp.IsSuccess = true;
      await responseHandler(resp);

      if (changed)
      {
        foreach(var playerState in gameEngine.GameState.PlayerStates)
        {
          InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
        }
      }   
    }

    public async Task DealCommunityCard(DealCommunityCardRequest request, Func<BaseResponse, Task> responseHandler)
    {
      _logger.LogInformation("DealCommunityCard {}", request);
      
      var resp = new BaseResponse() { RequestId = request.RequestId };

      var gameEngine = GameEngines.GetValueOrDefault(request.GameId);
      if (gameEngine == null)
      {
        resp.ErrorMessage = "GameId not found";
        await responseHandler(resp);
        return;
      }

      if (gameEngine.GameState.PlayerId != request.PlayerId)
      {
        resp.ErrorMessage = "Only dealer can initiate this request";
        await responseHandler(resp);
        return;
      }

      var changed = await gameEngine.DealCommunityCard();
      resp.IsSuccess = true;
      await responseHandler(resp);

      if (changed)
      {
        InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
      }   
    }

    private GameUpdateResponse MakeGameUpdateResponse(Models.GameState gameState)
    {
      var ret = new GameUpdateResponse() { GameId = gameState.GameId };
      ret.BestCurrentAsk = gameState.BestCurrentAsk;
      ret.BestCurrentBid = gameState.BestCurrentBid;
      ret.CommunityCardIds = gameState.RoundStates.Select(x => x.CommunityCardCardId).ToList();
      ret.PlayerPublicStates = gameState.PlayerStates.Select(x => MakePlayerPublicState(x)).ToList();
      ret.TradeUpdates = gameState.Trades.Select(x => MakeTradeUpdate(x)).ToList();
      ret.IsFinished = gameState.IsFinished;
      ret.IsSuccess = true;
      return ret;
    }

    private TradeUpdate MakeTradeUpdate(Models.Trade x)
    {
      return new TradeUpdate()
      {
        InitiatorPlayerPublicId = x.InitiatorPlayerStateId,
        TargetPlayerPublicId = x.TargetPlayerStateId,
        IsBuy = x.IsBuy,
        TradePrice = x.TradePrice
      };
    }

    private PlayerPublicState MakePlayerPublicState(Models.PlayerState x)
    {
      return new PlayerPublicState()
      {
        AvatarSeed = x.Player.AvatarSeed,
        CurrentAsk = x.CurrentAsk,
        CurrentBid = x.CurrentBid,
        DisplayName = x.Player.DisplayName,
        PlayerPublicId = x.PlayerStateId,
        PositionAvgPrice = x.PositionAvgPrice,
        PositionQty = x.PositionQty
      };
    }

    private PlayerUpdateResponse MakePlayerUpdateResponse(Models.PlayerState playerState)
    {
      var ret = new PlayerUpdateResponse() { GameId = playerState.GameState.GameId, PlayerId = playerState.PlayerId };
      ret.CardId = playerState.PlayerCardCardId;
      ret.IsSuccess = true;
      return ret;
    }

    private void InvokeOnGameUpdate(GameUpdateResponse response)
    {
      if (OnGameUpdate != null)
      {
        try
        {
          OnGameUpdate(response);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, nameof(InvokeOnGameUpdate));
        }
      }
    }

    private void InvokeOnPlayerUpdate(PlayerUpdateResponse response)
    {
      if (OnPlayerUpdate != null)
      {
        try
        {
          OnPlayerUpdate(response);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, nameof(InvokeOnPlayerUpdate));
        }
      }
    }

    public void Dispose()
    {
      DBContext.Dispose();
    }
  }
}