using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Shared.Lib;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMakingGame.Server.Models
{
  public class GameState
  {
    public int GameStateId { get; set; }

    public bool IsFinished { get; set; }

    [Required]
    [MaxLength(36)]
    [Column(TypeName = "char")]
    public string GameId { get; set; }
    public virtual Game Game { get; set; }

    [Required]
    [MaxLength(36)]
    [Column(TypeName = "char")]
    public string PlayerId { get; set; }
    public virtual Player Player { get; set; }

    public virtual List<PlayerState> PlayerStates { get; set; } = new List<PlayerState>();

    public virtual List<RoundState> RoundStates { get; set; } = new List<RoundState>();

    public virtual List<Trade> Trades { get; set; } = new List<Trade>();

    public double? BestCurrentAsk { get; set; }

    public double? BestCurrentBid { get; set; }

    public bool IsTradingLocked { get; set; }

    public double? SettlementPrice { get; set; }

    public string CardDeckHash { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}