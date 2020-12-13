using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using MarketMakingGame.Server.Hubs;
using MarketMakingGame.Server.Lib;
using MarketMakingGame.Server.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;

namespace MarketMakingGame.Server
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSignalR();
      services.AddControllersWithViews();
      services.AddRazorPages();
      services.AddResponseCompression(opts =>
            {
              opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                          new[] { "application/octet-stream" });
            });
      services.AddDbContext<GameDbContext>();
      services.AddScoped<GameService>();
      services.AddSingleton<GameHubEventManager>();
      services.AddSingleton<CardRepository>();
      services.AddHostedService<DeleteGameService>();
      services.AddLogging(opt =>
      {
        opt.AddConsole(c =>
        {
          c.TimestampFormat = "[HH:mm:ss.fff] ";
        });
      });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseForwardedHeaders(new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
      });
      
      app.UseResponseCompression();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseWebAssemblyDebugging();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseBlazorFrameworkFiles();
      app.UseStaticFiles();

      app.UseRouting();
      
      app.ApplicationServices.GetService<CardRepository>().Initialize();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
        endpoints.MapControllers();
        endpoints.MapHub<GameHub>("/gamehub");
        endpoints.MapFallbackToFile("index.html");
      });
    }
  }
}
