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
    public UserData Data { get; set; }

    public UserDataViewModel(ILocalStorageService localStorage)
    {
      Data = new UserData();
      _localStorage = localStorage;
      Console.WriteLine("UserDataViewModel Created!");
    }

    public (bool Success, string ErrorMessages) IsDataValid()
    {
      var validationResults = new List<ValidationResult>();
      var b = Validator.TryValidateObject(Data, new ValidationContext(Data, null, null),
        validationResults, true);
      return (b, String.Join(Environment.NewLine, validationResults.Select(x => x.ErrorMessage)));
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
          DisplayName = ""
        };
        Console.WriteLine($"Created new key-value Key={_userDataKey}, Value={data}");
        await _localStorage.SetItemAsync(_userDataKey, data);
      }
      Data = data;
      Data.OnChanged += OnDataChanged;
      Console.WriteLine("UserDataViewModel Initialized!");
      StateChanged(EventArgs.Empty);
    }

    private void OnDataChanged(EventArgs obj)
    {
      _ = SaveUserDataAsync();
    }

    public void RefreshAvatar()
    {
      _ = RefreshAvatarAsync();
    }

    private async Task RefreshAvatarAsync()
    {
      if (Data == null)
        await InitializeAsync();
      Data.AvatarSeed = Guid.NewGuid().ToBase62();
      await SaveUserDataAsync();
    }

    public async Task SaveUserDataAsync()
    {
      if (!IsDataValid().Success)
        return;
      await _localStorage.SetItemAsync<UserData>(_userDataKey, Data);
      StateChanged(EventArgs.Empty);
    }

    public event Action<EventArgs> StateChanged;
  }
}