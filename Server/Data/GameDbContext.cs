using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MarketMakingGame.Shared.Models;
using MarketMakingGame.Server.Models;
using System;

namespace MarketMakingGame.Server.Data
{
  public class GameDbContext : DbContext
  {
    public DbSet<Game> Games { get; set; }

    public DbSet<Player> Players { get; set; }

    public DbSet<Card> Cards { get; set; }

    public DbSet<GameState> GameStates { get; set; }

    public DbSet<PlayerState> PlayerStates { get; set; }

    public DbSet<RoundState> RoundStates { get; set; }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
      var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      options.UseSqlite($"Data Source={homeFolder}/data/marketmakinggame.db");
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new CardConfiguration());
    }
  }
}