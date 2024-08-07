﻿using CommunityToolkit.Mvvm.ComponentModel;
using Musify.Enums;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;

namespace Musify.Models;

public class Config
{
    public static readonly string ConfigFilepath = Path.Combine(Environment.CurrentDirectory, "Config.json");


    public Config()
    {
        Reset();
    }


    public ConfigPlugins Plugins { get; set; } = new();

    public ConfigDownloads Downloads { get; set; } = new();

    public ConfigPaths Paths { get; set; } = new();

    public ConfigLyrics Lyrics { get; set; } = new();


    public void Reset()
    {
        Plugins.ShowInstalled = true;
        Plugins.ShowAvailable = true;
        Plugins.Sorting = Sorting.Default;
        Plugins.SortDescending = false;

        Downloads.SelectedMetadatePlugin = null;
        Downloads.AlreadyExistsBehavior = AlreadyExistsBehavior.Ask;
        Downloads.Sorting = Sorting.Default;
        Downloads.SortDescending = false;

        Lyrics.GeniusAccessToken = "u_s2DsG-ewN4YDxgLZxzpo01mZaWSePOilc5rkBcylAYZ29cl93UzA7OEuPxWOCr";
        Lyrics.SearchResultsSorting = Sorting.Default;
        Lyrics.SearchResultsSortDescending = false;

        Paths.DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        Paths.Filename = "{artists} - {title}";
        Paths.FFMPEGLocation = "FFMPEG.exe";
    }
}


public partial class ConfigLyrics : ObservableObject
{
    [ObservableProperty]
    string geniusAccessToken = default!;

    [ObservableProperty]
    Sorting searchResultsSorting = default!;

    [ObservableProperty]
    bool searchResultsSortDescending = default!;
}

public partial class ConfigDownloads : ObservableObject
{
    [ObservableProperty]
    string? selectedMetadatePlugin = null;

    [ObservableProperty]
    AlreadyExistsBehavior alreadyExistsBehavior = default!;

    [ObservableProperty]
    Sorting sorting = default!;

    [ObservableProperty]
    bool sortDescending = default!;
}

public partial class ConfigPaths : ObservableObject
{
    [ObservableProperty]
    string downloadLocation = default!;

    [ObservableProperty]
    string filename = default!;

    [ObservableProperty]
    string fFMPEGLocation = default!;
}

public partial class ConfigPlugins : ObservableObject
{
    public Dictionary<string, IPluginConfig> Configs { get; set; } = [];

    [ObservableProperty]
    bool showOfKindPlatformSupport = default!;
    
    [ObservableProperty]
    bool showOfKindMetadata = default!;
    
    [ObservableProperty]
    bool showInstalled = default!;
    
    [ObservableProperty]
    bool showAvailable = default!;

    [ObservableProperty]
    Sorting sorting = default!;

    [ObservableProperty]
    bool sortDescending = default!;
}