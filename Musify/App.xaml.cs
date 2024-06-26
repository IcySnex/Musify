﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Musify.Helpers;
using Musify.Models;
using Musify.Services;
using Musify.ViewModels;
using Musify.Views;
using Serilog;

namespace Musify;

public partial class App : Application
{
    readonly IHost host;

    public static IServiceProvider Provider { get; private set; } = default!;
    public static InMemorySink Sink { get; private set; } = new();

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration.WriteTo.Debug();
                configuration.WriteTo.File(@"logs\log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10);
                configuration.WriteTo.Sink(Sink);
            })
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonFile("config.json", true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.Configure<Config>(context.Configuration);
                //Config config = context.Configuration.Get<Config>() ?? new();

                // Add services
                services.AddSingleton<AppStartupHandler>();
                services.AddSingleton<Navigation>();
                services.AddSingleton<JsonConverter>();
                services.AddSingleton<Spotify>();
                services.AddSingleton<YouTube>();
                services.AddSingleton<YouTubeMusic>();
                services.AddSingleton<Lyrics>();

                // Add ViewModels and MainView
                services.AddSingleton<HomeViewModel>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<SpotifyViewModel>();
                services.AddSingleton<YouTubeViewModel>();
                services.AddSingleton<YouTubeMusicViewModel>();
                services.AddSingleton<LyricsViewModel>();

                services.AddSingleton<DownloadsViewModel>();

                services.AddTransient<LyricsInfoViewModel>();

                services.AddSingleton<MainView>();

            })
            .Build();
        Provider = host.Services;

        InitializeComponent();
    }


    protected override void OnLaunched(LaunchActivatedEventArgs args) =>
        Provider.GetRequiredService<AppStartupHandler>();

}