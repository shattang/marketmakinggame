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
using Microsoft.Extensions.Configuration;

namespace MarketMakingGame.Server.Lib
{
  public class DeleteGameService : IHostedService, IDisposable
  {
    private IHost _host;
    private ILogger _logger;
    private readonly IConfiguration _configuration;
    Timer _timer;

    public DeleteGameService(ILoggerProvider provider, IHost host, IConfiguration configuration)
    {
      _host = host;
      _logger = provider.CreateLogger(nameof(DeleteGameService));
      _configuration = configuration;
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
      _logger.LogInformation("Started {}", nameof(DeleteGameService));
      var freqMins = _configuration.GetValue<int>("DeleteGameService:FrequencyMinutes");
      _timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(freqMins));
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