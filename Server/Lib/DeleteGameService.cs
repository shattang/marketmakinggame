using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using MarketMakingGame.Server.Data;
using MarketMakingGame.Shared.Models;
using Microsoft.Extensions.Logging;
using MarketMakingGame.Shared.Messages;
using MarketMakingGame.Shared.Lib;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarketMakingGame.Server.Lib
{
  public class DeleteGamesScheduledService : IHostedService, IDisposable
  {
    private IHost _host;
    private ILogger _logger;
    Timer _timer;

    public DeleteGamesScheduledService(ILoggerProvider provider, IHost host)
    {
      _host = host;
      _logger = provider.CreateLogger(nameof(DeleteGamesScheduledService));
    }

    private void OnTimer(object state)
    {
      DeleteGames().Wait();
    }

    private async Task DeleteGames()
    {
      var deletedIds = new List<string>();
      using (var scope = _host.Services.CreateScope())
      {
        using (var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>())
        {
          foreach (var gameState in dbContext.GameStates.Where(x => x.IsFinished))
          {
            dbContext.Remove(gameState);
            deletedIds.Add($"(GameId={gameState.GameId}, GameStateId={gameState.GameStateId})");
          }
          await dbContext.SaveChangesAsync();
        }
      }

      if (deletedIds.Count > 0)
        _logger.LogInformation($"Deleted Finished Games: {String.Join(",", deletedIds)}");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("Started {}", nameof(DeleteGamesScheduledService));
      _timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(10));
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}