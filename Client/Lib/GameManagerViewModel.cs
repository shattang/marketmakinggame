using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;

namespace MarketMakingGame.Client.Lib
{
  public class GameManagerViewModel : BaseViewModel
  {
    private const string CREATED_GAMES_KEY = "MMG.CreatedGames";
    private const int REQUEST_DELAY_MILLIS = 10000;
    private const string DEFAULT_SUBMIT_BUTTON_TEXT = "Go!";
    private const string DEFAULT_SUBMIT_BUTTON_ICON = "checked";
    private readonly MainViewModel MainViewModel;
    private readonly ILocalStorageService _localStorage;

    [Required]
    [MinLength(3, ErrorMessage = "Min 3 characters.")]
    [MaxLength(20, ErrorMessage = "Max 20 characters.")]
    public String GameName { get; set; }
    private CreateGameRequest _request = null;
    public string SubmitButtonText { get; set; } = DEFAULT_SUBMIT_BUTTON_TEXT;
    public string SubmitButtonIcon { get; set; } = DEFAULT_SUBMIT_BUTTON_ICON;
    public bool IsCreateGameFailedDialogVisible { get; set; } = false;
    public bool IsSubmitButtonDisabled => !CheckValid().Success;

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
      var createdGames = await _localStorage.GetItemAsync<List<string>>(CREATED_GAMES_KEY);
      if (createdGames == null)
      {
        createdGames = new List<string>();
        await _localStorage.SetItemAsync(CREATED_GAMES_KEY, createdGames);
      }
      InvokeStateChanged(EventArgs.Empty);
    }

    void OnCreateGameResponse(CreateGameResponse response)
    {
      if (_request != null && response.RequestId == _request.RequestId)
      {
        Console.WriteLine("Received Response: " + response);
        ResetRequest();

        if (response.IsSuccess)
        {
          MainViewModel.ShowGamePlayer(response);
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
      await MainViewModel.GameClient.SendRequestAsync("CreateGame", _request);
      InvokeStateChanged(EventArgs.Empty);

      await Task.Delay(REQUEST_DELAY_MILLIS);

      if (_request != null)
      {
        ResetRequest();
        IsCreateGameFailedDialogVisible = true;
        InvokeStateChanged(EventArgs.Empty);
      }
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