using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class Player
  {
    public String PlayerId { get; set; }

    public String DisplayName { get; set; }

    public String AvatarSeed { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}