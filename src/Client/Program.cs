using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MarketMakingGame.Client.Lib;
using Blazored.LocalStorage;
using BlazorStrap;
using Microsoft.JSInterop;
using MatBlazor;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace MarketMakingGame.Client
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebAssemblyHostBuilder.CreateDefault(args);
      builder.RootComponents.Add<App>("app");
      builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
      builder.Services.AddBlazoredLocalStorage();
      builder.Services.AddBootstrapCss();
      builder.Services.AddBlazorise(options => { options.ChangeTextOnKeyPress = true; });
      builder.Services.AddBootstrapProviders();
      builder.Services.AddFontAwesomeIcons();
      builder.Services.AddScoped<GameClient>();
      builder.Services.AddScoped<GamePlayerViewModel>();
      builder.Services.AddScoped<UserDataEditorViewModel>();
      builder.Services.AddScoped<GameManagerViewModel>();

      builder.Services.AddMatToaster(config =>
      {
        config.Position = MatToastPosition.TopCenter;
        config.PreventDuplicates = true;
        config.NewestOnTop = true;
        config.ShowCloseButton = true;
        config.MaximumOpacity = 100;
        config.VisibleStateDuration = 10000;
      });

      var webAssemblyHost = builder.Build();
      webAssemblyHost.Services
      .UseBootstrapProviders()
      .UseFontAwesomeIcons();

      await webAssemblyHost.RunAsync();
    }
  }
}
