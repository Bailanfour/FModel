using Microsoft.Maui.Controls;
using System;
using System.IO;
using Serilog;

namespace FModel.Maui;

public partial class App : Application
{
    public static string OutputDirectory { get; private set; }

    public App()
    {
        InitializeComponent();
        InitializeApp();
        MainPage = new MainPage();
    }

    private void InitializeApp()
    {
        OutputDirectory = Path.Combine(FileSystem.AppDataDirectory, "FModelMobile");
        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(Path.Combine(OutputDirectory, "Exports"));
        Directory.CreateDirectory(Path.Combine(OutputDirectory, "Logs"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                Path.Combine(OutputDirectory, "Logs", $"FModel-Mobile-{DateTime.Now:yyyy-MM-dd}.log"),
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
    }

    protected override void OnResume()
    {
        base.OnResume();
    }
}