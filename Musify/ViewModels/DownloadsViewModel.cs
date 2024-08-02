﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Enums;
using Musify.Services;
using Musify.Views;
using System.ComponentModel;
using Windows.System;

namespace Musify.ViewModels;

public partial class DownloadsViewModel : ObservableObject
{
    readonly ILogger<DownloadsViewModel> logger;
    readonly MainView mainView;
    readonly MediaEncoder encoder;

    public Config Config { get; }

    public DownloadsViewModel(
        ILogger<DownloadsViewModel> logger,
        Config config,
        MainView mainView,
        MediaEncoder encoder)
    {
        this.logger = logger;
        this.Config = config;
        this.mainView = mainView;
        this.encoder = encoder;

        Downloads = new()
        {
            KeySelector = Config.Downloads.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => download => download.Track.Title,
                Sorting.Artist => download => download.Track.Artists,
                Sorting.Duration => download => download.Track.Duration,
                _ => null
            },
            Descending = Config.Downloads.SortDescending,
            Filter = download =>
                (
                    download.Track.Title.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ||
                    download.Track.Artists.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ||
                    (download.Track.Album?.Contains(Query, StringComparison.InvariantCultureIgnoreCase) ?? false)
                ) &&
                ShowTracksFrom.Any(pair => download.Track.Plugin.Name == pair.Key && pair.Value)
        };

        Config.Downloads.PropertyChanged += OnConfigPropertyChanged;

        logger.LogInformation("[DownloadsViewModel-.ctor] DownloadsViewModel has been initialized");
    }


    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                Downloads.KeySelector = Config.Downloads.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => download => download.Track.Title,
                    Sorting.Artist => download => download.Track.Artists,
                    Sorting.Duration => download => download.Track.Duration,
                    _ => null
                };
                break;
            case "SortDescending":
                Downloads.Descending = Config.Downloads.SortDescending;
                break;
        }
        logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered search results");
    }


    public ObservableRangeCollection<DownloadContainer> Downloads { get; }


    public Dictionary<string, bool> ShowTracksFrom { get; } = [];


    [RelayCommand]
    async Task ShowTrackInfoAsync(
        DownloadContainer download)
    {
        DownloadableTrackInfoViewModel viewModel = App.Provider.GetRequiredService<DownloadableTrackInfoViewModel>();
        viewModel.Track = download.Track;

        await mainView.AlertAsync(new DownloadableTrackInfoView(viewModel));
        logger.LogInformation("[DownloadsViewModel-ShowTrackInfoAsync] Showed download track info");
    }

    [RelayCommand]
    async Task OpenTrackSourceAsync(
        DownloadContainer download)
    {
        await Launcher.LaunchUriAsync(new(download.Track.Url));
        logger.LogInformation("[DownloadsViewModel-OpenTrackSourceAsync] Browser was opened with track source url");
    }


    [ObservableProperty]
    string query = string.Empty;

    partial void OnQueryChanged(
        string value)
    {
        Downloads.Refresh();
        logger.LogInformation("[DownloadsViewModel-OnPropertyChanged] Refreshed downloads");
    }


    [RelayCommand]
    async Task DownloadAsync(
        DownloadContainer download)
    {
        IProgress<TimeSpan> progress = new Progress<TimeSpan>(currentTime => download.Progress = (int)(currentTime / download.Track.Duration * 100));
        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Starting download of track");
        try
        {
            download.Progress = 0;
            download.IsProcessing = true;
            Stream stream = await download.Track.Plugin.GetStreamAsync(download.Track, download.CancellationSource.Token);

            download.IsProcessing = false;
            string fileName = Config.Paths.Filename
                .Replace("{title}", download.Track.Title)
                .Replace("{artists}", download.Track.Artists)
                .Replace("{album}", download.Track.Album)
                .Replace("{release}", download.Track.ReleasedAt.ToString("MM-dd-yyyyy"))
                .ToLegitFileName();

            await encoder.WriteAsync(
                fileName,
                stream,
                download.Track.Plugin.Config.Quality,
                download.Track.Plugin.Config.Format,
                progress,
                download.CancellationSource.Token);

            download.IsProcessing = true;
            await Task.Delay(1000, download.CancellationSource.Token); // Simulate after download progress, e.g. writing metadata...

            Downloads.Remove(download);
            download.Dispose();

            mainView.ShowNotification("Success!", $"Finished downloading track: {download.Track.Title}.", NotificationLevel.Success);
            logger.LogInformation("[DownloadsViewModel-DownloadAsync] Finished download of track");
        }
        catch (OperationCanceledException)
        {
            download.Reset();

            logger.LogInformation("[DownloadsViewModel-DownloadAsync] Cancelled download of track {trackTitle}", download.Track.Title);
        }
        catch (Exception ex)
        {
            download.Reset();

            mainView.ShowNotification("Something went wrong!", $"Failed to download track: {download.Track.Title}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError("[DownloadsViewModel-DownloadAsync] Failed to download track {trackTitle}: {exception}", download.Track.Title, ex.Message);
        }
    }

    [RelayCommand]
    void Remove(
        DownloadContainer download)
    {
        download.CancellationSource.Cancel();
        download.Dispose();
        Downloads.Remove(download);

        logger.LogInformation("[DownloadsViewModel-RemoveDownload] Removed track from downloads");
    }


    [RelayCommand(IncludeCancelCommand = true)]
    async Task DownloadAllAsync(
        CancellationToken cancellationToken)
    {
        if (Downloads.Count == 0)
        {
            await mainView.AlertAsync("There are currently no downloads.\nSearch for tracks on a plugin to start downloading or change the filter options.", "Something went wrong!");
            return;
        }

        logger.LogInformation("[DownloadsViewModel-DownloadAllAsync] Starting to download all tracks...");
        foreach (DownloadContainer download in Downloads.ToArray())
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            if (!download.IsIdle)
                continue;

            CancellationTokenRegistration registration = cancellationToken.Register(download.CancellationSource.Cancel);

            await DownloadCommand.ExecuteAsync(download);
            await registration.DisposeAsync();
        }
        logger.LogInformation("[DownloadsViewModel-DownloadAsync] Finished downloading all tracks");

        if (await mainView.AlertAsync("All tracks have finished to download.\nDo you want to open the download location in the file explorer?", "Finished successfully!", "No", "Yes") == ContentDialogResult.Primary)
            await Launcher.LaunchFolderPathAsync(Config.Paths.DownloadLocation);
    }

    [RelayCommand]
    async Task RemoveAllAsync()
    {
        if (await mainView.AlertAsync("By clearing all your downloads your queue will be emptied and all running downloads will be stopped.", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
            return;

        DownloadAllCommand.Cancel();
        Downloads.Clear();

        logger.LogInformation("[DownloadsViewModel-ClearAsync] Cleared all tracks from downloads");
    }
}