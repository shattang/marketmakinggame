using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Blazored.LocalStorage;
using System.Collections.Generic;
using MarketMakingGame.Shared.Messages;

namespace MarketMakingGame.Client.Lib
{
  public class GamePlayerViewModel : BaseViewModel
  {
    public override (bool Success, string ErrorMessages) CheckValid()
    {
      return (true, string.Empty);
    }

    public override Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public override void Dispose()
    {
    }

    public string GameId { get; internal set; }
  }
}