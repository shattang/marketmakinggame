using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Client.Lib
{
  public class CardRepositoryViewModel: BaseViewModel
  {
    private readonly MainViewModel _mainViewModel;
    private List<Card> _cards;

    public CardRepositoryViewModel(MainViewModel mainViewModel)
    {
      _mainViewModel = mainViewModel;
    }

    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return (true, string.Empty);
    }

    public override async Task InitializeAsync()
    {
      var resp = await _mainViewModel.GameClient.InvokeRequestAsync<GetCardsResponse>("GetCards", new GetCardsRequest());
      _cards = resp.Cards;
      
    }

    public override void Dispose()
    {
    }
  }
}