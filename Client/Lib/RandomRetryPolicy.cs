using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace MarketMakingGame.Client.Lib
{
  public class RandomRetryPolicy : IRetryPolicy
  {
    private readonly Random _random = new Random();
    private readonly int _maxDelaySeconds = 120;

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
      TimeSpan ret;
      if (retryContext.ElapsedTime < TimeSpan.FromSeconds(1))
        ret = TimeSpan.FromSeconds(1);
      else
        ret = TimeSpan.FromSeconds(Math.Min(retryContext.ElapsedTime.TotalSeconds, _maxDelaySeconds) * _random.NextDouble());
      return ret;
    }
  }
}