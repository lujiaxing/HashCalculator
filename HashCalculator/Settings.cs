using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using static System.Environment;

namespace HashCalculator;

internal static class Settings
{
    private static Configure? config = null;
    private static readonly DirectoryInfo configDir =
        new(Path.Combine(GetFolderPath(
            SpecialFolder.LocalApplicationData), "HashCalculator"));
    private static readonly string configFile = Path.Combine(
        configDir.FullName, "config.json");

    public static Configure Current
    {
        get
        {
            config ??= LoadConfigure();
            return config;
        }
        set { config = value; }
    }

    public static bool SaveConfigure()
    {
        try
        {
            if (!configDir.Exists)
                configDir.Create();
            config ??= new Configure();
            using (FileStream fs = File.Create(configFile))
                JsonSerializer.Serialize(fs, config, typeof(Configure));
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"设置保存失败：\n{ex.Message}", "错误");
            return false;
        }
    }

    private static Configure LoadConfigure()
    {
        if (!File.Exists(configFile))
            return new Configure();
        try
        {
            object? obj;
            using (FileStream fs = File.OpenRead(configFile))
                obj = JsonSerializer.Deserialize(fs, typeof(Configure));
            return obj is Configure cfg ? cfg : new Configure();
        }
        catch
        {
            return new Configure();
        }
    }
}

internal sealed class Configure
{
    private double mainWindowWidth = 800.0;
    private double mainWindowHeight = 600.0;
    private double mainWindowTop = 0.0;
    private double mainWindowLeft = 0.0;
    private string theLastUsedPath = string.Empty;
    private Concurrency concurrency = Concurrency.One;
    private double settingsWindowWidth = 400.0;
    private double settingsWindowHeight = 280.0;

    public AlgoType SelectedAlgo { get; set; }

    public bool MainWindowTopmost { get; set; }

    public bool SaveMainWindowSize { get; set; }

    public bool SaveMainWindowPosition { get; set; }

    public double MainWindowTop
    {
        get { return this.mainWindowTop; }
        set { this.mainWindowTop = value; }
    }

    public double MainWindowLeft
    {
        get { return this.mainWindowLeft; }
        set { this.mainWindowLeft = value; }
    }

    public double MainWindowWidth
    {
        get { return this.mainWindowWidth; }
        set { this.mainWindowWidth = value; }
    }

    public double MainWindowHeight
    {
        get { return this.mainWindowHeight; }
        set { this.mainWindowHeight = value; }
    }

    public SearchPolicy FileSearchPolicy { get; set; }

    public SearchPolicy QuickVerifyFileSearchPolicy { get; set; }

    public bool UseLowercaseHash { get; set; }

    public Concurrency TaskNumber
    {
        get { return this.concurrency; }
        set { this.concurrency = value; }
    }

    public double SettingsWindowWidth
    {
        get { return this.settingsWindowWidth; }
        set { this.settingsWindowWidth = value; }
    }

    public double SettingsWindowHeight
    {
        get { return this.settingsWindowHeight; }
        set { this.settingsWindowHeight = value; }
    }

    public bool ShowResultText { get; set; }

    public bool NoExportColumn { get; set; }

    public bool NoDurationColumn { get; set; }

    public string TheLastUsedPath
    {
        get
        {
            if (this.theLastUsedPath != string.Empty)
                return this.theLastUsedPath;
            return GetFolderPath(SpecialFolder.Desktop);
        }
        set
        {
            if (value != null)
                this.theLastUsedPath = value;
            else
                this.theLastUsedPath = string.Empty;
        }
    }

    public bool RecalculateIncomplete { get; set; }
}
