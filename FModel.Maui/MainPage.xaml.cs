using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FModel.Maui.Services;
using Serilog;

namespace FModel.Maui;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private string _selectedPakPath;
    private string _outputDirectory;
    private PakUnpacker _pakUnpacker;
    private CancellationTokenSource _cts;
    private bool _isUnpacking;

    public bool IsUnpacking
    {
        get => _isUnpacking;
        set
        {
            _isUnpacking = value;
            OnPropertyChanged(nameof(IsUnpacking));
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        _outputDirectory = Path.Combine(FileSystem.AppDataDirectory, "FModelExports");
        OutputPathLabel.Text = $"Output: {_outputDirectory}";
    }

    private async void OnSelectPakClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select PAK File",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/octet-stream", "application/x-archive", "*.pak" } }
                })
            });

            if (result != null)
            {
                _selectedPakPath = result.FullPath;
                PakInfoLabel.Text = $"Selected: {result.FileName}";
                ExportButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to select PAK file");
            await DisplayAlert("Error", "Failed to select file", "OK");
        }
    }

    private async void OnLoadClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPakPath))
        {
            await DisplayAlert("Error", "Please select a PAK file first", "OK");
            return;
        }

        IsUnpacking = true;
        LoadButton.IsEnabled = false;
        CancelButton.IsVisible = true;
        ProgressBar.Progress = 0;
        ProgressPercentage.Text = "0%";

        try
        {
            _pakUnpacker = new PakUnpacker();
            _pakUnpacker.OnProgress += OnUnpackerProgress;
            _pakUnpacker.OnProgressPercentage += OnUnpackerProgressPercentage;

            ProgressLabel.Text = "Initializing...";
            await _pakUnpacker.InitializeAsync(_selectedPakPath, _outputDirectory);

            var aesKey = AesKeyEntry.Text?.Trim();
            if (!string.IsNullOrEmpty(aesKey))
            {
                await _pakUnpacker.AddAesKeyAsync(aesKey);
            }

            var assets = _pakUnpacker.GetAllAssets();
            var assetCount = 0;
            foreach (var _ in assets) assetCount++;

            ProgressLabel.Text = $"Loaded {assetCount} assets";
            ExportButton.IsEnabled = true;
            LoadButton.IsEnabled = true;
            CancelButton.IsVisible = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load PAK file");
            ProgressLabel.Text = $"Error: {ex.Message}";
            await DisplayAlert("Error", $"Failed to load PAK: {ex.Message}", "OK");
            LoadButton.IsEnabled = true;
            CancelButton.IsVisible = false;
        }
        finally
        {
            IsUnpacking = false;
        }
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (_pakUnpacker == null)
        {
            await DisplayAlert("Error", "Please load a PAK file first", "OK");
            return;
        }

        IsUnpacking = true;
        ExportButton.IsEnabled = false;
        LoadButton.IsEnabled = false;
        CancelButton.IsVisible = true;
        ProgressBar.Progress = 0;
        ProgressPercentage.Text = "0%";

        _cts = new CancellationTokenSource();

        try
        {
            await _pakUnpacker.ExportAllAssetsAsync(_cts.Token);
            await DisplayAlert("Success", "All assets exported successfully!", "OK");
        }
        catch (OperationCanceledException)
        {
            ProgressLabel.Text = "Export canceled";
            await DisplayAlert("Canceled", "Export was canceled", "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export assets");
            ProgressLabel.Text = $"Error: {ex.Message}";
            await DisplayAlert("Error", $"Failed to export: {ex.Message}", "OK");
        }
        finally
        {
            IsUnpacking = false;
            ExportButton.IsEnabled = true;
            LoadButton.IsEnabled = true;
            CancelButton.IsVisible = false;
            _cts?.Dispose();
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
        ProgressLabel.Text = "Canceling...";
    }

    private void OnUnpackerProgress(object sender, string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProgressLabel.Text = message;
        });
    }

    private void OnUnpackerProgressPercentage(object sender, int percentage)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProgressBar.Progress = percentage / 100.0;
            ProgressPercentage.Text = $"{percentage}%";
        });
    }
}