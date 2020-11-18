using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketMakingGame.Shared.Lib;
using MarketMakingGame.Shared.Models;

namespace MarketMakingGame.Server.Models
{
  public class PlayerState
  {
    public int PlayerStateId { get; set; }

    [Required]
    [MaxLength(36)]
    [Column(TypeName = "char")]
    public string PlayerId { get; set; }
    public virtual Player Player { get; set; }

    public int GameStateId { get; set; }
    public virtual GameState GameState { get; set; }

    public int PlayerCardCardId { get; set; }
    public virtual Card PlayerCard { get; set; }

    public double? CurrentBid { get; set; }

    public double? CurrentAsk { get; set; }

    public double? PositionCashFlow { get; set; }

    public double? PositionQty { get; set; }

    public double? SettlementPnl { get; set; }

    public override string ToString()
    {
      return this.ToStringWithProperties();
    }
  }
}