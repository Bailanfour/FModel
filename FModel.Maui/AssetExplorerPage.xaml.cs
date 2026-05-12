using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.IO;

namespace FModel.Maui;

public partial class AssetExplorerPage : ContentPage
{
    private readonly string archivePath;
    private ObservableCollection<AssetItem> assets = new();

    public AssetExplorerPage(string path)
    {
        InitializeComponent();
        archivePath = path;
        AssetCollectionView.ItemsSource = assets;
        SearchBar.TextChanged += OnSearchTextChanged;
        LoadAssets();
    }

    private async void LoadAssets()
    {
        LoadingIndicator.IsRunning = true;
        assets.Clear();

        await Task.Delay(100);

        if (Directory.Exists(archivePath))
        {
            foreach (var file in Directory.GetFiles(archivePath, "*", SearchOption.TopDirectoryOnly).Take(50))
            {
                var fileInfo = new FileInfo(file);
                assets.Add(new AssetItem
                {
                    Name = fileInfo.Name,
                    Type = GetFileType(fileInfo.Extension),
                    Size = GetFileSize(fileInfo.Length),
                    Icon = GetFileIcon(fileInfo.Extension),
                    Path = file
                });
            }
        }

        LoadingIndicator.IsRunning = false;
    }

    private string GetFileType(string extension)
    {
        return extension switch
        {
            ".uasset" => "Unreal Asset",
            ".umap" => "Unreal Map",
            ".pak" => "Packed Archive",
            ".json" => "JSON File",
            ".png" => "Image",
            ".jpg" => "Image",
            ".wav" => "Audio",
            ".ogg" => "Audio",
            _ => "Unknown"
        };
    }

    private string GetFileSize(long size)
    {
        if (size < 1024) return $"{size} B";
        if (size < 1024 * 1024) return $"{size / 1024:F1} KB";
        if (size < 1024 * 1024 * 1024) return $"{size / (1024 * 1024):F1} MB";
        return $"{size / (1024 * 1024 * 1024):F1} GB";
    }

    private string GetFileIcon(string extension)
    {
        return extension switch
        {
            ".uasset" => "package.png",
            ".umap" => "map.png",
            ".pak" => "archive.png",
            ".json" => "json.png",
            ".png" => "image.png",
            ".jpg" => "image.png",
            ".wav" => "sound.png",
            ".ogg" => "sound.png",
            _ => "file.png"
        };
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Implement search filter
    }

    private void OnAssetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AssetItem item)
        {
            DisplayAlert("Selected", $"Asset: {item.Name}", "OK");
        }
    }

    private void OnBackClicked(object sender, System.EventArgs e)
    {
        Navigation.PopAsync();
    }
}

public class AssetItem
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Size { get; set; }
    public string Icon { get; set; }
    public string Path { get; set; }
}