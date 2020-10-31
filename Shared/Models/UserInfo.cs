using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Lib;

namespace MarketMakingGame.Shared.Models
{
  public class UserInfo
  {
    public String DisplayName { get; set; }

    public String UserId { get; set; }

    public String AvatarSeed { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}