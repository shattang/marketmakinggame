using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Client.Lib
{
  public class GamePlayerViewModel : BaseViewModel
  {
    public MainViewModel MainViewModel { get; }
    public ILocalStorageService LocalStorageService { get; }
    public Game CurrentGame { get; set; }

    public List<Card> Cards { get; set; }

    public Card UnopenedCard { get; set; }

    public GamePlayerViewModel(MainViewModel mainViewModel, ILocalStorageService localStorage)
    {
      MainViewModel = mainViewModel;
      LocalStorageService = localStorage;
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return (true, string.Empty);
    }

    public override async Task InitializeAsync()
    {
      var resp = await MainViewModel.GameClient.InvokeRequestAsync<GetCardsResponse>("GetCards", new GetCardsRequest());
      
    }

    public override void Dispose()
    {
    }
  }
}