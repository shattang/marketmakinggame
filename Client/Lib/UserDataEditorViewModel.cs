using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MarketMakingGame.Client.Models;
using MarketMakingGame.Shared.Lib;
using System.ComponentModel.DataAnnotations;

namespace MarketMakingGame.Client.Lib
{
  public class UserDataEditorViewModel : BaseViewModel
  {
    private const String USER_DATA_KEY = "MMG.UserData";
    private readonly ILocalStorageService _localStorage;
    private UserData _data = new UserData();
    private bool _isUserDataEditorOpen = false;

    [Required]
    [MaxLength(20, ErrorMessage = "Max 20 characters")]
    [MinLength(3, ErrorMessage = "Min 3 characters")]
    public String DisplayName
    {
      get { return _data.DisplayName; }
      set
      {
        if (_data.DisplayName != value)
        {
          _data.DisplayName = value;
          _ = SaveUserDataAsync();
        }
      }
    }

    public String UserId => _data.UserId;

    public String AvatarSeed => _data.AvatarSeed;

    public String AvatarUrl =>
      $"https://avatars.dicebear.com/api/gridy/{AvatarSeed}.svg";

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
      return ValidationHelpers.ValidateObject(this);
    }

    public override async Task InitializeAsync()
    {
      var data = await _localStorage.GetItemAsync<UserData>(USER_DATA_KEY);
      if (data == null)
      {
        data = new UserData()
        {
          AvatarSeed = Guid.NewGuid().ToBase62(),
          UserId = Guid.NewGuid().ToBase62(),
          DisplayName = String.Empty
        };
        await _localStorage.SetItemAsync(USER_DATA_KEY, data);
      }
      _data = data;
      InvokeStateChanged(EventArgs.Empty);
    }

    public void RefreshAvatar()
    {
      _ = RefreshAvatarAsync();
    }

    private async Task RefreshAvatarAsync()
    {
      if (_data == null)
        await InitializeAsync();
      _data.AvatarSeed = Guid.NewGuid().ToBase62();
      await SaveUserDataAsync();
    }

    private async Task SaveUserDataAsync()
    {
      var res = CheckValid();
      if (!res.Success)
        return;
      await _localStorage.SetItemAsync<UserData>(USER_DATA_KEY, _data);
      InvokeStateChanged(EventArgs.Empty);
    }

    public override void Dispose()
    {
    }
  }
}