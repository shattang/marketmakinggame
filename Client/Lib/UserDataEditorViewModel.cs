using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;
using System.ComponentModel.DataAnnotations;

namespace MarketMakingGame.Client.Lib
{
  public class UserDataEditorViewModel : BaseViewModel
  {
    public const String USER_DATA_KEY = "MMG.UserData";
    private readonly ILocalStorageService _localStorage;
    public Player Data { get; set; } = new Player();
    private bool _isUserDataEditorOpen = false;

    public String AvatarUrl =>
      $"https://avatars.dicebear.com/api/gridy/{Data.AvatarSeed}.svg";

    public bool IsUserDataEditorOpen
    {
      get => _isUserDataEditorOpen;
      set
      {
        if (_isUserDataEditorOpen != value)
        {
          _isUserDataEditorOpen = value;
          InvokeStateChanged(EventArgs.Empty);
        }
      }
    }

    public UserDataEditorViewModel(ILocalStorageService localStorage)
    {
      _localStorage = localStorage;
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return ValidationHelpers.ValidateObject(this.Data);
    }

    public override async Task InitializeAsync()
    {
      var data = await _localStorage.GetItemAsync<Player>(USER_DATA_KEY);
      if (data == null)
      {
        data = new Player()
        {
          AvatarSeed = Guid.NewGuid().ToBase62(),
          PlayerId = Guid.NewGuid().ToBase62(),
          DisplayName = String.Empty
        };
        await _localStorage.SetItemAsync(USER_DATA_KEY, data);
      }
      Data = data;
      InvokeStateChanged(EventArgs.Empty);
    }

    public void RefreshAvatar()
    {
      Data.AvatarSeed = Guid.NewGuid().ToBase62();
    }

    public void SaveUserData()
    {
      _ = SaveUserDataAsync();
    }

    private async Task SaveUserDataAsync()
    {
      var res = CheckValid();
      if (!res.Success)
        return;
      await _localStorage.SetItemAsync<Player>(USER_DATA_KEY, Data);
      InvokeStateChanged(EventArgs.Empty);
    }

    public override void Dispose()
    {
    }
  }
}