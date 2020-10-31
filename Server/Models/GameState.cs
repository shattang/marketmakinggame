using System;
using System.Collections.Generic;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Models
{
  public class GameState
  {
    public GameInfo GameInfo { get; set;}

    public String OwnerUserId { get; set; }

    public Dictionary<String, PlayerState> PlayerStates { get; set; }

    public List<int> CommunityCards { get; set; }
  }
}