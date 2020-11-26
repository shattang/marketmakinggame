using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace MarketMakingGame.Client.Lib
{
  public class GameManagerViewModel : BaseViewModel
  {
    private const string CREATED_GAMES_KEY = "MMG.CreatedGames";
    private const int REQUEST_DELAY_MILLIS = 10000;
    private const string DEFAULT_SUBMIT_BUTTON_TEXT = "Go!";
    private const string DEFAULT_SUBMIT_BUTTON_ICON = "checked";
    private const string DEFAULT_REQUEST_FAILED_MESSAGE = "Request timed out";

    private readonly ILogger _logger;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navManager;
    private readonly GameClient _gameClient;
    private CreateGameRequest _request = null;

    public Game Data { get; set; } = new Game();
    public List<Game> CreatedGames { get; set; }
    public string SubmitButtonText { get; private set; } = DEFAULT_SUBMIT_BUTTON_TEXT;
    public string SubmitButtonIcon { get; private set; } = DEFAULT_SUBMIT_BUTTON_ICON;
    public bool IsCreateGameFailedDialogVisible { get; set; } = false;
    public string CreateGameFailedDialogMessage { get; private set; } = DEFAULT_REQUEST_FAILED_MESSAGE;
    public bool IsSubmitButtonDisabled => _gameClient.IsConnected && !CheckValid().Success;

    public GameManagerViewModel(GameClient gameClient, NavigationManager navManager, 
      ILocalStorageService localStorage, ILoggerProvider loggerProvider)
    {
      this._logger = loggerProvider.CreateLogger(nameof(GameManagerViewModel));
      this._gameClient = gameClient;
      this._localStorage = localStorage;
      this._navManager = navManager;
      this.Data.GameId = Guid.NewGuid().ToBase62();
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return ValidationHelpers.ValidateObject(this.Data);
    }

    protected override async Task InitializeAsync()
    {
      await _gameClient.InitViewModelAsync();
      _gameClient.OnCreateGameResponse += OnCreateGameResponse;
      var createdGames = await _localStorage.GetItemAsync<List<Game>>(CREATED_GAMES_KEY);
      if (createdGames == null || createdGames.Count == 0)
      {
        CreatedGames = new List<Game>();
      }
      else
      {
        var req = new GetGameInfoRequest()
        {
          GameIds = createdGames.Select(x => x.GameId).ToList()
        };
        var resp = await _gameClient.InvokeRequestAsync<GetGameInfoResponse>("GetGameInfo", req);
        CreatedGames = resp.IsSuccess ? resp.Games : new List<Game>();
      }
      await _localStorage.SetItemAsync(CREATED_GAMES_KEY, CreatedGames);
      state = STATE_INITIALIZED;
      _logger.LogInformation("Init!");
    }

    void OnCreateGameResponse(CreateGameResponse response)
    {
      if (_request != null && response.RequestId == _request.RequestId)
      {
        ResetRequest();

        if (response.IsSuccess)
        {
          CreatedGames.Add(response.Game);
          _ = _localStorage.SetItemAsync(CREATED_GAMES_KEY, CreatedGames);
          _ = ShowGamePlayer(response.Game);
        }
        else
        {
          IsCreateGameFailedDialogVisible = true;
          CreateGameFailedDialogMessage = response.ErrorMessage;
          InvokeStateChanged(EventArgs.Empty);
        }
      }
    }

    public async void OnSubmitButtonClicked()
    {
      if (_request != null)
        return;

      if (!CheckValid().Success)
        return;

      _request = new CreateGameRequest()
      {
        Game = new Game(){ GameName = Data.GameName },
        Player = await _localStorage.GetItemAsync<Player>(UserDataEditorViewModel.USER_DATA_KEY)
      };

      SubmitButtonText = "Waiting ...";
      SubmitButtonIcon = "update";
      InvokeStateChanged(EventArgs.Empty);

      try
      {
        await _gameClient.SendRequestAsync("CreateGame", _request);
      }
      catch (Exception ex)
      {
        ResetRequest();
        CreateGameFailedDialogMessage = $"Request Failed: {ex.Message}";
        IsCreateGameFailedDialogVisible = true;
        InvokeStateChanged(EventArgs.Empty);
        return;
      }

      await Task.Delay(REQUEST_DELAY_MILLIS);
      if (_request != null)
      {
        ResetRequest();
        IsCreateGameFailedDialogVisible = true;
        CreateGameFailedDialogMessage = DEFAULT_REQUEST_FAILED_MESSAGE;
        InvokeStateChanged(EventArgs.Empty);
      }
    }

    public void OnJoinGameButtonClicked(int index)
    {
      var info = CreatedGames[index];
      _ = ShowGamePlayer(info);
    }

    internal async Task ShowGamePlayer(Game game)
    {
      await Task.Delay(3000);
      _navManager.NavigateTo($"/playgame/{game.GameId}");
    }

    private void ResetRequest()
    {
      _request = null;
      SubmitButtonText = DEFAULT_SUBMIT_BUTTON_TEXT;
      SubmitButtonIcon = DEFAULT_SUBMIT_BUTTON_ICON;
    }

    public override void Dispose()
    {
      _gameClient.OnCreateGameResponse -= OnCreateGameResponse;
    }
  }
}