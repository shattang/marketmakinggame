using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;

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

    [Required]
    [MinLength(3, ErrorMessage = "Min 3 characters.")]
    [MaxLength(20, ErrorMessage = "Max 20 characters.")]
    public String GameName { get; set; }
    private CreateGameRequest _request = null;
    public List<GameInfo> CreatedGames { get; set; }
    public string SubmitButtonText { get; set; } = DEFAULT_SUBMIT_BUTTON_TEXT;
    public string SubmitButtonIcon { get; set; } = DEFAULT_SUBMIT_BUTTON_ICON;
    public bool IsCreateGameFailedDialogVisible { get; set; } = false;
    public string CreateGameFailedDialogMessage { get; set; } = DEFAULT_REQUEST_FAILED_MESSAGE;
    public bool IsSubmitButtonDisabled => MainViewModel.GameClient.IsConnected && !CheckValid().Success;

    public GameManagerViewModel(MainViewModel mainView, ILocalStorageService localStorage)
    {
      this.MainViewModel = mainView;
      this._localStorage = localStorage;
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return ValidationHelpers.ValidateObject(this);
    }

    public override async Task InitializeAsync()
    {
      MainViewModel.GameClient.OnCreateGameResponse += OnCreateGameResponse;
      var createdGames = await _localStorage.GetItemAsync<List<GameInfo>>(CREATED_GAMES_KEY);
      if (createdGames == null || createdGames.Count == 0)
      {
        CreatedGames = new List<GameInfo>();
      }
      else
      {
        var req = new GetGameInfoRequest()
        {
          GameIds = createdGames.Select(x => x.GameId).ToList()
        };

        var resp = await MainViewModel.GameClient.InvokeRequestAsync<GetGameInfoResponse>("GetGameInfo", req);
        CreatedGames = resp.IsSuccess ? resp.GameInfos : new List<GameInfo>();
      }
      await _localStorage.SetItemAsync(CREATED_GAMES_KEY, CreatedGames);
      InvokeStateChanged(EventArgs.Empty);
    }

    void OnCreateGameResponse(CreateGameResponse response)
    {
      if (_request != null && response.RequestId == _request.RequestId)
      {
        var gameInfo = new GameInfo() { GameId = response.GameId, GameName = _request.GameName };
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
        GameName = GameName,
        UserAvatar = MainViewModel.UserDataEditorViewModel.AvatarSeed,
        UserId = MainViewModel.UserDataEditorViewModel.UserId,
        UserName = MainViewModel.UserDataEditorViewModel.DisplayName
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