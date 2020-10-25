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
      _hubConnection.On<CreateGameResponse>("OnCreateGameResponse", HandleCreateGameResponse);
      _hubConnection.On<JoinGameResponse>("OnJoinGameResponse", HandleJoinGameResponse);
      _logger.LogDebug("GameClient Created");
    }

    public async Task InitializeAsync()
    {
      await _hubConnection.StartAsync();
      InvokeOnIsConnectedChanged();
      _logger.LogDebug("GameClient Started!");
    }

    public Task SendRequestAsync(string methodName, BaseRequest message)
    {
      _logger.LogDebug("Sending Request: Method={0}, Message={1}", methodName, message);
      return _hubConnection.SendAsync(methodName, message);
    }

    public bool IsConnected =>
        _hubConnection.State == HubConnectionState.Connected;

    private Task OnHubReconnected(string arg)
    {
      _logger.LogWarning($"GameHub reconnected: Reason={arg}");
      InvokeOnIsConnectedChanged();
      return Task.CompletedTask;
    }

    private Task OnHubReconnecting(Exception arg)
    {
      _logger.LogWarning($"GameHub reconnecting: Type={arg?.GetType()?.FullName} Reason={arg?.Message}");
      InvokeOnIsConnectedChanged();
      return Task.CompletedTask;
    }

    private Task OnHubClosed(Exception arg)
    {
      _logger.LogWarning($"GameHub closed: Type={arg?.GetType()?.FullName} Reason={arg?.Message}");
      InvokeOnIsConnectedChanged();
      return Task.CompletedTask;
    }

    private void HandleCreateGameResponse(CreateGameResponse response)
    {
      _logger.LogDebug("HandleCreateGameResponse Message={0}", response);
      if (OnCreateGameResponse != null)
        OnCreateGameResponse(response);
    }

    private void HandleJoinGameResponse(JoinGameResponse response)
    {
      _logger.LogDebug("HandleJoinGameResponse Message={0}", response);
      if (OnJoinGameResponse != null)
        OnJoinGameResponse(response);
    }

    public void Dispose()
    {
      _ = _hubConnection.DisposeAsync();
    }

    void InvokeOnIsConnectedChanged()
    {
      if (OnIsConnectedChanged != null)
        OnIsConnectedChanged(IsConnected);
    }

    public event Action<CreateGameResponse> OnCreateGameResponse;
    public event Action<JoinGameResponse> OnJoinGameResponse;

    public event Action<bool> OnIsConnectedChanged;

    public override string ToString()
    {
      return _hubConnection.ConnectionId.ToString();
    }
  }
}