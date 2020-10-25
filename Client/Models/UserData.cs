using System;
using System.ComponentModel.DataAnnotations;

namespace MarketMakingGame.Client.Models
{
  public class UserData
  {
    public String DisplayName { get; set; }

    public String UserId { get; set; }

    public String AvatarSeed { get; set; }

    public override string ToString()
    {
      return $"UserData(UserId={UserId}, DisplayName={DisplayName}, AvatarSeed={AvatarSeed})";
    }
  }
}