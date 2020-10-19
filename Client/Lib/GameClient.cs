using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using MarketMakingGame.Shared.Messages;
using System.Threading.Tasks;

namespace MarketMakingGame.Client.Lib
{
  public class GameClient : IDisposable
  {
    private const string GAMEHUB_NAME = "gamehub";
    private HubConnection _hubConnection;
    private ILogger _logger;

    public GameClient(ILoggerProvider loggerProvider, NavigationManager navigationManager)
    {
      _logger = loggerProvider.CreateLogger(nameof(GameClient));
      _hubConnection = new HubConnectionBuilder()
        .WithUrl(navigationManager.ToAbsoluteUri(GAMEHUB_NAME))
        .WithAutomaticReconnect(new RandomRetryPolicy())
        .ConfigureLogging(logging => logging.AddProvider(loggerProvider))
        .Build();

      _hubConnection.Closed += OnHubClosed;
      _hubConnection.Reconnecting += OnHubReconnecting;
      _hubConnection.Reconnected += OnHubReconnected;
      _hubConnection.On<CreateGameResponse>("OnCreateGameResponse", OnCreateGameResponse);
      _hubConnection.On<JoinGameResponse>("OnJoinGameResponse", OnJoinGameResponse);

    }

    public Task StartAsync()
    {
      return _hubConnection.StartAsync();
    }

    public Task SendRequestAsync(string requestName, BaseRequest message)
    {
      _logger.LogInformation("Sending Request: {}", message);
      return _hubConnection.SendAsync(requestName, message);
    }

    public bool IsConnected =>
        _hubConnection.State == HubConnectionState.Connected;

    private Task OnHubReconnected(string arg)
    {
      _logger.LogWarning("GameHub reconnected: Reason={}", arg);
      return Task.CompletedTask;
    }

    private Task OnHubReconnecting(Exception arg)
    {
      _logger.LogWarning("GameHub reconnecting: Type={} Reason={}", arg?.GetType()?.FullName, arg?.Message);
      return Task.CompletedTask;
    }

    private Task OnHubClosed(Exception arg)
    {
      _logger.LogWarning("GameHub closed: Type={} Reason={}", arg?.GetType()?.FullName, arg?.Message);
      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _ = _hubConnection.DisposeAsync();
    }

    public event Action<CreateGameResponse> OnCreateGameResponse;
    public event Action<JoinGameResponse> OnJoinGameResponse;
  }
}