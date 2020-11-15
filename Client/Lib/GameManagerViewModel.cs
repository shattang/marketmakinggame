using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Client.Lib
{
  public class GameManagerViewModel : BaseViewModel
  {
    private const string CREATED_GAMES_KEY = "MMG.CreatedGames";
    private const int REQUEST_DELAY_MILLIS = 10000;
    private const string DEFAULT_SUBMIT_BUTTON_TEXT = "Go!";
    private const string DEFAULT_SUBMIT_BUTTON_ICON = "checked";
    private const string DEFAULT_REQUEST_FAILED_MESSAGE = "Request timed out";

    private readonly MainViewModel MainViewModel;
    private readonly ILocalStorageService _localStorage;
    private CreateGameRequest _request = null;

    public Game Data { get; set; } = new Game();
    public List<Game> CreatedGames { get; set; }
    public string SubmitButtonText { get; private set; } = DEFAULT_SUBMIT_BUTTON_TEXT;
    public string SubmitButtonIcon { get; private set; } = DEFAULT_SUBMIT_BUTTON_ICON;
    public bool IsCreateGameFailedDialogVisible { get; set; } = false;
    public string CreateGameFailedDialogMessage { get; private set; } = DEFAULT_REQUEST_FAILED_MESSAGE;
    public bool IsSubmitButtonDisabled => MainViewModel.GameClient.IsConnected && !CheckValid().Success;

    public GameManagerViewModel(MainViewModel mainView, ILocalStorageService localStorage)
    {
      this.MainViewModel = mainView;
      this._localStorage = localStorage;
      this.Data.GameId = Guid.NewGuid().ToBase62();
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return ValidationHelpers.ValidateObject(this.Data);
    }

    public override async Task InitializeAsync()
    {
      MainViewModel.GameClient.OnCreateGameResponse += OnCreateGameResponse;
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

        var resp = await MainViewModel.GameClient.InvokeRequestAsync<GetGameInfoResponse>("GetGameInfo", req);
        CreatedGames = resp.IsSuccess ? resp.Games : new List<Game>();
      }
      await _localStorage.SetItemAsync(CREATED_GAMES_KEY, CreatedGames);
      InvokeStateChanged(EventArgs.Empty);
    }

    void OnCreateGameResponse(CreateGameResponse response)
    {
      if (_request != null && response.RequestId == _request.RequestId)
      {
        var gameInfo = _request.Game;
        gameInfo.GameId = response.GameId;
        ResetRequest();

        if (response.IsSuccess)
        {
          CreatedGames.Add(gameInfo);
          _ = _localStorage.SetItemAsync(CREATED_GAMES_KEY, CreatedGames);
          MainViewModel.ShowGamePlayer(gameInfo);
        }
        else
        {
          IsCreateGameFailedDialogVisible = true;
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
        Game = new Game()
        {
          GameName = Data.GameName
        },
        Player = new Player()
        {
          AvatarSeed = MainViewModel.UserDataEditorViewModel.Data.AvatarSeed,
          PlayerId = MainViewModel.UserDataEditorViewModel.Data.PlayerId,
          DisplayName = MainViewModel.UserDataEditorViewModel.Data.DisplayName
        }
      };

      SubmitButtonText = "Waiting ...";
      SubmitButtonIcon = "update";
      InvokeStateChanged(EventArgs.Empty);

      try
      {
        await MainViewModel.GameClient.SendRequestAsync("CreateGame", _request);
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
        CreateGameFailedDialogMessage = DEFAULT_REQUEST_FAILED_MESSAGE;
        IsCreateGameFailedDialogVisible = true;
        InvokeStateChanged(EventArgs.Empty);
      }
    }

    public void OnJoinGameButtonClicked(int index)
    {
      var info = CreatedGames[index];
      MainViewModel.ShowGamePlayer(info);
    }

    private void ResetRequest()
    {
      _request = null;
      SubmitButtonText = DEFAULT_SUBMIT_BUTTON_TEXT;
      SubmitButtonIcon = DEFAULT_SUBMIT_BUTTON_ICON;
    }

    public override void Dispose()
    {
      MainViewModel.GameClient.OnCreateGameResponse -= OnCreateGameResponse;
    }
  }
}