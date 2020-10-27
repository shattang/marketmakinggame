using System;
using System.Threading.Tasks;
using MarketMakingGame.Shared.Lib;
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
    private readonly AppViewModel AppService;
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

    public GameManagerViewModel(AppViewModel appService, ILocalStorageService localStorage)
    {
      this.AppService = appService;
      this._localStorage = localStorage;
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return ValidationHelpers.ValidateObject(this);
    }

    public override async Task InitializeAsync()
    {
      AppService.GameClient.OnCreateGameResponse += OnCreateGameResponse;
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
      if (response.RequestId == _request.RequestId)
      {
        Console.WriteLine("Received Response: " + response);
        _request = null;
        //NavigationManager.NavigateTo("/console?joingameid=" + response.RequestId);
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
        UserAvatar = AppService.UserDataEditorViewModel.AvatarSeed,
        UserId = AppService.UserDataEditorViewModel.UserId,
        UserName = AppService.UserDataEditorViewModel.DisplayName
      };

      SubmitButtonText = "Waiting ...";
      SubmitButtonIcon = "update";
      await AppService.GameClient.SendRequestAsync("CreateGame", _request);
      InvokeStateChanged(EventArgs.Empty);

      await Task.Delay(REQUEST_DELAY_MILLIS);

      if (_request != null)
      {
        IsCreateGameFailedDialogVisible = true;
        _request = null;
        SubmitButtonText = DEFAULT_SUBMIT_BUTTON_TEXT;
        SubmitButtonIcon = DEFAULT_SUBMIT_BUTTON_ICON;
        InvokeStateChanged(EventArgs.Empty);
      }
    }
  }
}