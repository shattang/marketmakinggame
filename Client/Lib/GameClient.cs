using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using MarketMakingGame.Shared.Messages;
using System.Threading.Tasks;

namespace MarketMakingGame.Client.Lib
{
  public class GameClient : BaseViewModel
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
      _hubConnection.On<GameUpdateResponse>("OnGameUpdateResponse", HandleGameUpdateResponse);
      _hubConnection.On<PlayerUpdateResponse>("OnPlayerUpdateResponse", HandlePlayerUpdateResponse);
      _hubConnection.On<DealGameResponse>("OnDealGameResponse", HandleDealGameResponse);
      _hubConnection.On<UpdateQuoteResponse>("OnUpdateQuoteResponse", HandleUpdateQuoteResponse);
      _hubConnection.On<TradeResponse>("OnTradeResponse", HandleTradeResponse);
      _logger.LogDebug("GameClient Created");
    }

    private void HandleUpdateQuoteResponse(UpdateQuoteResponse obj)
    {
      _logger.LogInformation("HandleUpdateQuoteResponse Message={0}", obj);
      if (OnUpdateQuoteResponse != null)
        OnUpdateQuoteResponse(obj);
    }

    private void HandleTradeResponse(TradeResponse obj)
    {
      _logger.LogInformation("HandleTradeResponse Message={0}", obj);
      if (OnTradeResponse != null)
        OnTradeResponse(obj);
    }

    private void HandleDealGameResponse(DealGameResponse obj)
    {
      _logger.LogInformation("HandleDealGameResponse Message={0}", obj);
      if (OnDealGameResponse != null)
        OnDealGameResponse(obj);
    }

    private void HandlePlayerUpdateResponse(PlayerUpdateResponse obj)
    {
      _logger.LogInformation("HandlePlayerUpdateResponse Message={0}", obj);
      if (OnPlayerUpdateResponse != null)
        OnPlayerUpdateResponse(obj);
    }

    private void HandleGameUpdateResponse(GameUpdateResponse obj)
    {
      _logger.LogInformation("HandleGameUpdateResponse Message={0}", obj);
      if (OnGameUpdateResponse != null)
        OnGameUpdateResponse(obj);
    }

    protected override async Task InitializeAsync()
    {
      await _hubConnection.StartAsync();
      state = STATE_INITIALIZED;
      InvokeOnIsConnectedChanged();
      _logger.LogInformation("Init!");
    }

    public Task SendRequestAsync(string methodName, BaseRequest request)
    {
      _logger.LogInformation("Sending Request: Method={0}, Request={1}", methodName, request);
      return _hubConnection.SendAsync(methodName, request);
    }

    public async Task<RESP> InvokeRequestAsync<RESP>(string methodName, BaseRequest request)
    where RESP : BaseResponse, new()
    {
      _logger.LogInformation("Invoking Request: Method={0}, Request={1}", methodName, request);
      try
      {
        return await _hubConnection.InvokeAsync<RESP>(methodName, request);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"{nameof(InvokeRequestAsync)} failed");
        var resp = new RESP();
        resp.ErrorMessage = ex.Message;
        resp.IsSuccess = false;
        return resp;
      }
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
      _logger.LogInformation("HandleCreateGameResponse Message={0}", response);
      if (OnCreateGameResponse != null)
        OnCreateGameResponse(response);
    }

    private void HandleJoinGameResponse(JoinGameResponse response)
    {
      _logger.LogInformation("HandleJoinGameResponse Message={0}", response);
      if (OnJoinGameResponse != null)
        OnJoinGameResponse(response);
    }

    public override void Dispose()
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
    public event Action<GameUpdateResponse> OnGameUpdateResponse;
    public event Action<PlayerUpdateResponse> OnPlayerUpdateResponse;
    public event Action<DealGameResponse> OnDealGameResponse;
    public event Action<UpdateQuoteResponse> OnUpdateQuoteResponse;
    public event Action<TradeResponse> OnTradeResponse;
    public event Action<bool> OnIsConnectedChanged;

    public override string ToString()
    {
      return _hubConnection.ConnectionId.ToString();
    }
  }
}