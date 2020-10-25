using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MarketMakingGame.Client.Models;
using MarketMakingGame.Shared.Lib;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace MarketMakingGame.Client.Lib
{
  public class UserDataViewModel
  {
    private const String _userDataKey = "MMG.UserData";
    private ILocalStorageService _localStorage;
    private UserData _data;

    [Required]
    [MaxLength(20, ErrorMessage = "Max 20 characters")]
    [MinLength(3, ErrorMessage = "Min 3 characters")]
    public String DisplayName
    {
      get { return _data.DisplayName; }
      set
      {
        _data.DisplayName = value;
        _ = SaveUserDataAsync();
      }
    }

    public String UserId => _data.UserId;

    public String AvatarSeed => _data.AvatarSeed;

    public UserDataViewModel(ILocalStorageService localStorage)
    {
      _data = new UserData();
      _localStorage = localStorage;
    }

    public (bool Success, string ErrorMessages) CheckValid()
    {
      return ValidationHelpers.ValidateObject(this);
    }

    public async Task InitializeAsync()
    {
      var data = await _localStorage.GetItemAsync<UserData>(_userDataKey);
      if (data == null)
      {
        data = new UserData()
        {
          AvatarSeed = Guid.NewGuid().ToBase62(),
          UserId = Guid.NewGuid().ToBase62(),
          DisplayName = String.Empty
        };
        await _localStorage.SetItemAsync(_userDataKey, data);
      }
      _data = data;
      StateChanged(EventArgs.Empty);
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
      await _localStorage.SetItemAsync<UserData>(_userDataKey, _data);
      StateChanged(EventArgs.Empty);
    }

    public event Action<EventArgs> StateChanged;
  }
}