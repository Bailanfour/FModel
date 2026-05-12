using Microsoft.Maui.Controls;
using System;
using System.IO;
using Serilog;
using FModel.Settings;
using Newtonsoft.Json;

namespace FModel.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        InitializeApp();
        MainPage = new MainPage();
    }

    private void InitializeApp()
    {
        try
        {
            var settingsPath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
            if (File.Exists(settingsPath))
            {
                UserSettings.Default = JsonConvert.DeserializeObject<UserSettings>(
                    File.ReadAllText(settingsPath), 
                    FModel.Framework.JsonNetSerializer.SerializerSettings);
            }
            else
            {
                UserSettings.Default = new UserSettings();
            }
        }
        catch
        {
            UserSettings.Default = new UserSettings();
        }

        var outputDir = Path.Combine(FileSystem.AppDataDirectory, "Output");
        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(Path.Combine(outputDir, "Exports"));
        Directory.CreateDirectory(Path.Combine(outputDir, "Backups"));
        Directory.CreateDirectory(Path.Combine(outputDir, "Logs"));

        UserSettings.Default.OutputDirectory = outputDir;
        UserSettings.Default.RawDataDirectory = Path.Combine(outputDir, "Exports");
        UserSettings.Default.PropertiesDirectory = Path.Combine(outputDir, "Exports");
        UserSettings.Default.TextureDirectory = Path.Combine(outputDir, "Exports");
        UserSettings.Default.AudioDirectory = Path.Combine(outputDir, "Exports");
        UserSettings.Default.CodeDirectory = Path.Combine(outputDir, "Exports");
        UserSettings.Default.ModelDirectory = Path.Combine(outputDir, "Exports");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                Path.Combine(outputDir, "Logs", $"FModel-Mobile-{DateTime.Now:yyyy-MM-dd}.log"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("FModel Mobile initialized");
    }

    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        UserSettings.Save();
    }

    protected override void OnResume()
    {
        base.OnResume();
    }
}