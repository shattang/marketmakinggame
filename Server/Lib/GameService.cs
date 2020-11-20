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

    public async Task OnPlayerDisconnectedAsync(string gameId, string playerId)
    {
      var (success, errMessage) = await DeleteGameIfFinished(gameId, playerId);
      if (success)
      {
        _logger.LogInformation("Game deleted on disconnect: GameId={}", gameId);
        return;
      }
      else
      {
        Models.GameState gameState;
        var gameEngine= GameEngines.GetValueOrDefault(gameId);
        if (gameEngine != null)
        {
          gameState = gameEngine.GameState;
        }
        else
        {
          gameState = await DBContext.GameStates
            .FirstOrDefaultAsync(x => x.GameId == gameId);
        }
        
        if (gameState != null)
        {
          var playerState = gameState.PlayerStates
            .FirstOrDefault(x => x.PlayerId == playerId);
          if (playerState != null)
          {
            playerState.IsConnected = false;
            await DBContext.SaveChangesAsync();
            InvokeOnGameUpdate(MakeGameUpdateResponse(gameState));
          }
        }
      }
    }

    public async Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("GetGameInfo {}", request);

      var lookup = await DBContext.Games
        .Where(x => request.GameIds.Contains(x.GameId)).ToListAsync();

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

      var numPlayerGames = await DBContext.GameStates
        .CountAsync(x => x.PlayerId == request.Player.PlayerId && !x.IsFinished);
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
      resp.Game = gameEngine.GameState.Game;
      await responseHandler(resp);

      InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
      InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState, gameEngine.GameState.Trades));
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
        var gameState = await DBContext.GameStates
          .FirstOrDefaultAsync(x => x.GameId == request.GameId);
        if (gameState == null)
        {
          resp.ErrorMessage = "GameId not found";
          await responseHandler(resp);
          return;
        }

        if (gameState.IsFinished)
        {
          resp.ErrorMessage = "Game is finished";
          await responseHandler(resp);
          return;
        }

        gameEngine = new GameEngine(_loggerProvider, this, gameState);
        GameEngines[gameEngine.GameState.GameId] = gameEngine;
      }

      var playerState = await gameEngine.JoinGameAsync(request);
      resp.IsSuccess = true;
      resp.Game = gameEngine.GameState.Game;
      await responseHandler(resp);

      InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
      InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState, gameEngine.GameState.Trades));
    }

    public async Task DealGameAsync(DealGameRequest request, Func<DealGameResponse, Task> responseHandler)
    {
      _logger.LogInformation("DealGame {}", request);

      var resp = new DealGameResponse() { RequestId = request.RequestId };

      if (request.RequestType == DealGameRequest.RequestTypes.DeleteGame)
      {
        (resp.IsSuccess, resp.ErrorMessage) = await DeleteGameIfFinished(request.GameId, request.PlayerId);
        await responseHandler(resp);
        return;
      }

      var gameEngine = GameEngines.GetValueOrDefault(request.GameId);
      if (gameEngine == null)
      {
        resp.ErrorMessage = "GameId not found";
        await responseHandler(resp);
        return;
      }

      if (gameEngine.GameState.IsFinished)
      {
        resp.ErrorMessage = "Game is finished";
        await responseHandler(resp);
        return;
      }

      if (gameEngine.GameState.PlayerId != request.PlayerId)
      {
        resp.ErrorMessage = "Only dealer can initiate this request";
        await responseHandler(resp);
        return;
      }

      if (request.RequestType == DealGameRequest.RequestTypes.DealPlayerCards)
      {
        var updatedPlayers = await gameEngine.DealPlayerCards();
        resp.IsSuccess = true;
        await responseHandler(resp);

        foreach (var playerState in updatedPlayers)
        {
          InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
        }
      }
      else if (request.RequestType == DealGameRequest.RequestTypes.DealNextCommunityCard)
      {
        (resp.IsSuccess, resp.ErrorMessage) = await gameEngine.DealNextCommunityCard();
        await responseHandler(resp);

        if (resp.IsSuccess)
        {
          InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
        }
      }
      else if (request.RequestType == DealGameRequest.RequestTypes.LockTrading ||
        request.RequestType == DealGameRequest.RequestTypes.UnlockTrading)
      {
        var locked = request.RequestType == DealGameRequest.RequestTypes.LockTrading ? true : false;
        (resp.IsSuccess, resp.ErrorMessage) = await gameEngine.LockTrading(locked);
        await responseHandler(resp);

        if (resp.IsSuccess)
        {
          InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
        }
      }
      else if (request.RequestType == DealGameRequest.RequestTypes.FinishGame)
      {
        (resp.IsSuccess, resp.ErrorMessage) = await gameEngine.FinishGame();
        await responseHandler(resp);

        if (resp.IsSuccess)
        {
          InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
        }
      }
      else
      {
        resp.ErrorMessage = "Unknown deal request type";
        await responseHandler(resp);
      }
    }

    public async Task UpdateQuoteAsync(UpdateQuoteRequest request, Func<UpdateQuoteResponse, Task> responseHandler)
    {
      _logger.LogInformation("UpdateQuote {}", request);

      var resp = new UpdateQuoteResponse() { RequestId = request.RequestId };

      var gameEngine = GameEngines.GetValueOrDefault(request.GameId);
      if (gameEngine == null)
      {
        resp.ErrorMessage = "GameId not found";
        await responseHandler(resp);
        return;
      }

      if (gameEngine.GameState.IsFinished)
      {
        resp.ErrorMessage = "Game is finished";
        await responseHandler(resp);
        return;
      }

      (resp.IsSuccess, resp.ErrorMessage) = await gameEngine.UpdateQuote(request);
      await responseHandler(resp);

      if (resp.IsSuccess)
      {
        InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
      }
    }

    public async Task TradeAsync(TradeRequest request, Func<TradeResponse, Task> responseHandler)
    {
      _logger.LogInformation("Trade {}", request);

      var resp = new TradeResponse() { RequestId = request.RequestId };

      var gameEngine = GameEngines.GetValueOrDefault(request.GameId);
      if (gameEngine == null)
      {
        resp.ErrorMessage = "GameId not found";
        await responseHandler(resp);
        return;
      }

      if (gameEngine.GameState.IsFinished)
      {
        resp.ErrorMessage = "Game is finished";
        await responseHandler(resp);
        return;
      }

      List<Models.Trade> trades;
      (resp.IsSuccess, resp.ErrorMessage, trades) = await gameEngine.Trade(request);
      await responseHandler(resp);

      if (resp.IsSuccess)
      {
        InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState, trades));
      }
    }

    private GameUpdateResponse MakeGameUpdateResponse(Models.GameState gameState, List<Models.Trade> trades = null)
    {
      var ret = new GameUpdateResponse() { GameId = gameState.GameId };
      ret.BestCurrentAsk = gameState.BestCurrentAsk;
      ret.BestCurrentBid = gameState.BestCurrentBid;
      if (gameState.RoundStates != null)
        ret.CommunityCardIds = gameState.RoundStates.Select(x => x.CommunityCardCardId).ToList();
      if (gameState.PlayerStates != null)
        ret.PlayerPublicStates = gameState.PlayerStates.Select(x => MakePlayerPublicState(x)).ToList();
      ret.TradeUpdates = trades == null ? null : trades.Select(x => MakeTradeUpdate(x)).ToList();
      ret.IsFinished = gameState.IsFinished;
      ret.IsTradingLocked = gameState.IsTradingLocked;
      ret.IsSuccess = true;
      ret.SettlementPrice = gameState.SettlementPrice;
      return ret;
    }

    private TradeUpdate MakeTradeUpdate(Models.Trade x)
    {
      return new TradeUpdate()
      {
        InitiatorPlayerPublicId = x.InitiatorPlayerStateId,
        TargetPlayerPublicId = x.TargetPlayerStateId,
        IsBuy = x.IsBuy,
        TradePrice = x.TradePrice,
        TradeQty = x.TradeQty
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
        PositionCashFlow = x.PositionCashFlow,
        PositionQty = x.PositionQty,
        SettlementPnl = x.SettlementPnl,
        SettlementCardId = x.GameState.IsFinished ? (int?)x.PlayerCardCardId : null,
        IsConnected = x.IsConnected,
        IsPlayerCardDealt = x.PlayerCardCardId != UnopenedCard.CardId
      };
    }

    private PlayerUpdateResponse MakePlayerUpdateResponse(Models.PlayerState playerState)
    {
      var ret = new PlayerUpdateResponse() { GameId = playerState.GameState.GameId, PlayerId = playerState.PlayerId };
      ret.CardId = playerState.PlayerCardCardId;
      ret.PlayerPublicId = playerState.PlayerStateId;
      ret.IsDealer = playerState.PlayerId == playerState.GameState.PlayerId;
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

    private async Task<(bool IsSuccess, string ErrorMessage)> DeleteGameIfFinished(string gameId, string requestingPlayerId)
    {
      var gameState = await DBContext.GameStates
          .FirstOrDefaultAsync(x => x.GameId == gameId);
      if (gameState == null)
      {
        return (false, "GameId not found");
      }

      if (gameState.PlayerId != requestingPlayerId)
      {
        return (false, "Only dealer can initiate this request");
      }

      if (!gameState.IsFinished)
      {
        return (false, "Game is in progress");
      }

      GameEngines.Remove(gameId, out var _);
      DBContext.Remove(gameState);
      await DBContext.SaveChangesAsync();
      return (true, string.Empty);
    }

    public void Dispose()
    {
      DBContext.Dispose();
    }
  }
}