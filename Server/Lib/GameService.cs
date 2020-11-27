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
  public class GameService
  {
    private const int MAX_GAMES_PER_USER = 2;
    private static ConcurrentDictionary<string, string> _lockedGames = new ConcurrentDictionary<string, string>();
    private readonly CardRepository _cardRepo;
    public GameHubEventManager EventManager { get; }
    private readonly GameDbContext _dbContext;
    private readonly ILoggerProvider _loggerProvider;
    private readonly ILogger _logger;

    public GameService(ILoggerProvider loggerProvider, GameDbContext dbContext, CardRepository repository,
      GameHubEventManager eventManager)
    {
      _loggerProvider = loggerProvider;
      _logger = loggerProvider.CreateLogger(nameof(GameService));
      _dbContext = dbContext;
      _cardRepo = repository;
      EventManager = eventManager;
    }

    private static async Task<bool> AcquireGameLock(string gameId, string lockSecret, TimeSpan timeout)
    {
      DateTime start = DateTime.Now;
      while (_lockedGames.GetOrAdd(gameId, k => lockSecret) != lockSecret)
      {
        if ((DateTime.Now - start) > timeout)
          return false;
        await Task.Delay(50);
      }
      return true;
    }

    private static void ReleaseGameLock(string gameId, string lockSecret)
    {
      var coll = ((ICollection<KeyValuePair<string, string>>)_lockedGames);
      coll.Remove(new KeyValuePair<string, string>(gameId, lockSecret));
    }

    public async Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request)
    {
      _logger.LogInformation("GetGameInfo {}", request);

      var lookup = await _dbContext.GameStates
        .Where(x => request.GameIds.Contains(x.GameId)).ToListAsync();

      return new GetGameInfoResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        Games = lookup.Select(x => x.Game).ToList()
      };

    }

    public GetCardsResponse GetCards(GetCardsRequest request)
    {
      _logger.LogInformation("GetCards {}", request);

      return new GetCardsResponse()
      {
        RequestId = request.RequestId,
        IsSuccess = true,
        Cards = _cardRepo.Cards,
        UnopenedCard = _cardRepo.UnopenedCard
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

      var numPlayerGames = await _dbContext.GameStates
        .CountAsync(x => x.PlayerId == request.Player.PlayerId && !x.IsFinished);
      _logger.LogInformation($"numPlayerGames={numPlayerGames}");
      if (numPlayerGames > MAX_GAMES_PER_USER)
      {
        resp.ErrorMessage = "Too many active games for player";
        await responseHandler(resp);
        return;
      }

      var gameEngine = new GameEngine(_loggerProvider, _dbContext, _cardRepo);
      var playerState = await gameEngine.CreateGameAsync(request);
      resp.IsSuccess = true;
      resp.Game = gameEngine.GameState.Game;
      await responseHandler(resp);

      InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
      InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
      InvokeOnTradeUpdate(null, MakeTradeUpdateResponse(gameEngine.GameState.GameId, gameEngine.GameState.Trades));
    }

    public async Task JoinGameAsync(JoinGameRequest request, Func<JoinGameResponse, Task> responseHandler)
    {
      _logger.LogInformation("JoinGame {}", request);

      var resp = new JoinGameResponse() { RequestId = request.RequestId };

      try
      {
        if (!await AcquireGameLock(request.GameId, request.RequestId, TimeSpan.FromSeconds(30)))
        {
          resp.ErrorMessage = "Timed out while acquiring game lock";
          await responseHandler(resp);
          return;
        }

        var gameState = await _dbContext.GameStates
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

        var gameEngine = new GameEngine(_loggerProvider, _dbContext, _cardRepo);
        gameEngine.GameState = gameState;

        var playerState = await gameEngine.JoinGameAsync(request);
        resp.IsSuccess = true;
        resp.Game = gameEngine.GameState.Game;
        await responseHandler(resp);

        InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
        InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
        InvokeOnTradeUpdate(playerState.PlayerId,
          MakeTradeUpdateResponse(gameEngine.GameState.GameId, gameEngine.GameState.Trades));
      }
      finally
      {
        ReleaseGameLock(request.GameId, request.RequestId);
      }
    }

    public async Task DealGameAsync(DealGameRequest request, Func<DealGameResponse, Task> responseHandler)
    {
      _logger.LogInformation("DealGame {}", request);

      var resp = new DealGameResponse() { RequestId = request.RequestId };

      try
      {
        if (!await AcquireGameLock(request.GameId, request.RequestId, TimeSpan.FromSeconds(30)))
        {
          resp.ErrorMessage = "Timed out while acquiring game lock";
          await responseHandler(resp);
          return;
        }

        if (request.RequestType == DealGameRequest.RequestTypes.DeleteGame)
        {
          (resp.IsSuccess, resp.ErrorMessage) = await DeleteGameIfFinished(request.GameId, request.PlayerId);
          await responseHandler(resp);
          return;
        }

        var gameState = await _dbContext.GameStates
          .FirstOrDefaultAsync(x => x.GameId == request.GameId);
        if (gameState == null)
        {
          resp.ErrorMessage = "GameId not found";
          await responseHandler(resp);
          return;
        }

        var gameEngine = new GameEngine(_loggerProvider, _dbContext, _cardRepo);
        gameEngine.GameState = gameState;

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

        if (request.RequestType == DealGameRequest.RequestTypes.DealCard)
        {
          var (success, err, updatedPlayerStates) = await gameEngine.DealPlayerCards();
          resp.IsSuccess = success;
          resp.ErrorMessage = err;
          await responseHandler(resp);

          if (resp.IsSuccess)
          {
            foreach (var playerState in updatedPlayerStates)
            {
              InvokeOnPlayerUpdate(MakePlayerUpdateResponse(playerState));
            }
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
      finally
      {
        ReleaseGameLock(request.GameId, request.RequestId);
      }
    }

    public async Task UpdateQuoteAsync(UpdateQuoteRequest request, Func<UpdateQuoteResponse, Task> responseHandler)
    {
      _logger.LogInformation("UpdateQuote {}", request);

      var resp = new UpdateQuoteResponse() { RequestId = request.RequestId };

      try
      {
        if (!await AcquireGameLock(request.GameId, request.RequestId, TimeSpan.FromSeconds(30)))
        {
          resp.ErrorMessage = "Timed out while acquiring game lock";
          await responseHandler(resp);
          return;
        }

        var gameState = await _dbContext.GameStates
          .FirstOrDefaultAsync(x => x.GameId == request.GameId);
        if (gameState == null)
        {
          resp.ErrorMessage = "GameId not found";
          await responseHandler(resp);
          return;
        }

        var gameEngine = new GameEngine(_loggerProvider, _dbContext, _cardRepo);
        gameEngine.GameState = gameState;

        if (gameEngine.GameState.IsFinished)
        {
          resp.ErrorMessage = "Game is finished";
          await responseHandler(resp);
          return;
        }

        var (engineResp, trades) = await gameEngine.UpdateQuote(request);
        await responseHandler(engineResp);

        if (engineResp.IsSuccess)
        {
          InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
          InvokeOnTradeUpdate(null, MakeTradeUpdateResponse(gameEngine.GameState.GameId, trades));
        }
      }
      finally
      {
        ReleaseGameLock(request.GameId, request.RequestId);
      }
    }

    public async Task TradeAsync(TradeRequest request, Func<TradeResponse, Task> responseHandler)
    {
      _logger.LogInformation("Trade {}", request);

      var resp = new TradeResponse() { RequestId = request.RequestId };

      try
      {
        if (!await AcquireGameLock(request.GameId, request.RequestId, TimeSpan.FromSeconds(30)))
        {
          resp.ErrorMessage = "Timed out while acquiring game lock";
          await responseHandler(resp);
          return;
        }

        var gameState = await _dbContext.GameStates
          .FirstOrDefaultAsync(x => x.GameId == request.GameId);
        if (gameState == null)
        {
          resp.ErrorMessage = "GameId not found";
          await responseHandler(resp);
          return;
        }

        var gameEngine = new GameEngine(_loggerProvider, _dbContext, _cardRepo);
        gameEngine.GameState = gameState;

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
          InvokeOnGameUpdate(MakeGameUpdateResponse(gameEngine.GameState));
          InvokeOnTradeUpdate(null, MakeTradeUpdateResponse(gameEngine.GameState.GameId, trades));
        }
      }
      finally
      {
        ReleaseGameLock(request.GameId, request.RequestId);
      }
    }

    private GameUpdateResponse MakeGameUpdateResponse(Models.GameState gameState)
    {
      var ret = new GameUpdateResponse() { GameId = gameState.GameId };
      ret.BestCurrentAsk = gameState.BestCurrentAsk;
      ret.BestCurrentBid = gameState.BestCurrentBid;
      if (gameState.RoundStates != null)
        ret.CommunityCardIds = gameState.RoundStates.Select(x => x.CommunityCardCardId).ToList();
      if (gameState.PlayerStates != null)
        ret.PlayerPublicStates = gameState.PlayerStates.Select(x => MakePlayerPublicState(x)).ToList();
      ret.IsFinished = gameState.IsFinished;
      ret.IsTradingLocked = gameState.IsTradingLocked;
      ret.IsSuccess = true;
      ret.SettlementPrice = gameState.SettlementPrice;
      ret.AllRoundsFinished = gameState.RoundStates.Count >= gameState.Game.NumberOfRounds;
      return ret;
    }

    private TradeUpdateResponse MakeTradeUpdateResponse(string gameId, List<Models.Trade> trades)
    {
      var ret = new TradeUpdateResponse() { GameId = gameId, IsSuccess = true };
      ret.TradeUpdates = trades == null ? null : trades.Select(x => MakeTradeUpdate(x)).ToList();
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
        TradeQty = x.TradeQty,
        TradeId = x.TradeId
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
        IsPlayerCardDealt = x.PlayerCardCardId != _cardRepo.UnopenedCard.CardId
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
      if (EventManager != null)
      {
        try
        {
          EventManager.HandleGameUpdate(response);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, nameof(InvokeOnGameUpdate));
        }
      }
    }

    private void InvokeOnPlayerUpdate(PlayerUpdateResponse response)
    {
      if (EventManager != null)
      {
        try
        {
          EventManager.HandlePlayerUpdate(response);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, nameof(InvokeOnPlayerUpdate));
        }
      }
    }

    private void InvokeOnTradeUpdate(string playerId, TradeUpdateResponse response)
    {
      if (EventManager != null)
      {
        try
        {
          EventManager.HandleTradeUpdate(playerId, response);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, nameof(InvokeOnTradeUpdate));
        }
      }
    }

    public async Task OnPlayerDisconnectedAsync(string gameId, string playerId)
    {
      var lockSecret = Guid.NewGuid().ToString();
      try
      {
        if (!await AcquireGameLock(gameId, lockSecret, TimeSpan.FromSeconds(30)))
        {
          return;
        }

        var gameState = await _dbContext.GameStates
              .FirstOrDefaultAsync(x => x.GameId == gameId);
        if (gameState != null)
        {
          var playerState = gameState.PlayerStates
            .FirstOrDefault(x => x.PlayerId == playerId);
          if (playerState != null)
          {
            playerState.CurrentAsk = null;
            playerState.CurrentBid = null;
            gameState.BestCurrentAsk = gameState.PlayerStates.Select(x => x.CurrentAsk).Min();
            gameState.BestCurrentBid = gameState.PlayerStates.Select(x => x.CurrentBid).Max();
            playerState.IsConnected = false;
            await _dbContext.SaveChangesAsync();
            InvokeOnGameUpdate(MakeGameUpdateResponse(gameState));
          }
        }
      }
      finally
      {
        ReleaseGameLock(gameId, lockSecret);
      }
    }

    private async Task<(bool IsSuccess, string ErrorMessage)> DeleteGameIfFinished(string gameId, string requestingPlayerId)
    {
      var gameState = await _dbContext.GameStates
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

      _dbContext.Remove(gameState);
      await _dbContext.SaveChangesAsync();

      return (true, string.Empty);
    }
  }
}