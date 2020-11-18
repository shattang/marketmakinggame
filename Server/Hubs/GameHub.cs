using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MarketMakingGame.Server.Lib;
using System.Collections.Concurrent;

namespace MarketMakingGame.Server.Hubs
{
  public class GameHub : Hub, IDisposable
  {
    private readonly ILogger _logger;
    private readonly GameService _gameService;
    private readonly GameHubEventManager _gameHubEventManager;

    public GameHub(ILogger<GameHub> logger, GameService gameService, GameHubEventManager gameHubEventManager)
    {
      _logger = logger;
      _gameService = gameService;
      _gameHubEventManager = gameHubEventManager;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await _gameHubEventManager.RemoveConnection(Context.ConnectionId);
    }

    public GetCardsResponse GetCards(GetCardsRequest request)
    {
      return _gameService.GetCards(request);
    }

    public Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request)
    {
      return _gameService.GetGameInfo(request);
    }

    public async Task CreateGame(CreateGameRequest request)
    {
      var connectionId = Context.ConnectionId;
      async Task InvokeOnCreateGameResponse(CreateGameResponse resp)
      {
        if (resp.IsSuccess)
        {
          await _gameHubEventManager.AddConnection(resp.Game.GameId, request.Player.PlayerId, connectionId);
        }
        _logger.LogInformation("Sending Response: {}", resp);
        await Clients.Caller.SendAsync("OnCreateGameResponse", resp);
      }

      try
      {
        await _gameService.CreateGameAsync(request, InvokeOnCreateGameResponse);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(CreateGame));
        await InvokeOnCreateGameResponse(new CreateGameResponse()
        { ErrorMessage = $"{ex.GetType()}: {ex.Message}" });
      }
    }

    public async Task JoinGame(JoinGameRequest request)
    {
      var connectionId = Context.ConnectionId;
      async Task InvokeOnJoinGameResponse(JoinGameResponse resp)
      {
        if (resp.IsSuccess)
        {
          await _gameHubEventManager.AddConnection(request.GameId, request.Player.PlayerId, Context.ConnectionId);
        }
        _logger.LogInformation("Sending Response: {}", resp);
        await Clients.Caller.SendAsync("OnJoinGameResponse", resp);
      }

      try
      {
        await _gameService.JoinGameAsync(request, InvokeOnJoinGameResponse);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(JoinGame));
        await InvokeOnJoinGameResponse(new JoinGameResponse()
        { ErrorMessage = $"{ex.GetType()}: {ex.Message}" });
      }
    }

    public async Task DealGame(DealGameRequest request)
    {
      async Task InvokeOnDealGameResponse(DealGameResponse resp)
      {
        _logger.LogInformation("Sending Response: {}", resp);
        await Clients.Caller.SendAsync("OnDealGameResponse", resp);
      }

      try
      {
        await _gameService.DealGameAsync(request, InvokeOnDealGameResponse);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(JoinGame));
        await InvokeOnDealGameResponse(new DealGameResponse()
        { ErrorMessage = $"{ex.GetType()}: {ex.Message}" });
      }
    }

    public async Task UpdateQuote(UpdateQuoteRequest request)
    {
      async Task InvokeOnUpdateQuoteResponse(UpdateQuoteResponse resp)
      {
        _logger.LogInformation("Sending Response: {}", resp);
        await Clients.Caller.SendAsync("OnUpdateQuoteResponse", resp);
      }

      try
      {
        await _gameService.UpdateQuoteAsync(request, InvokeOnUpdateQuoteResponse);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(JoinGame));
        await InvokeOnUpdateQuoteResponse(new UpdateQuoteResponse()
        { ErrorMessage = $"{ex.GetType()}: {ex.Message}" });
      }
    }

    public async Task Trade(TradeRequest request)
    {
      async Task InvokeOnTradeResponse(TradeResponse resp)
      {
        _logger.LogInformation("Sending Response: {}", resp);
        await Clients.Caller.SendAsync("OnTradeResponse", resp);
      }

      try
      {
        await _gameService.TradeAsync(request, InvokeOnTradeResponse);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, nameof(JoinGame));
        await InvokeOnTradeResponse(new TradeResponse()
        { ErrorMessage = $"{ex.GetType()}: {ex.Message}" });
      }
    }
  }
}