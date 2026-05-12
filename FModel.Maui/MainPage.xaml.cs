using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.IO;
using FModel.ViewModels;
using FModel.Settings;
using System.Threading.Tasks;

namespace FModel.Maui;

public partial class MainPage : ContentPage
{
    private ObservableCollection<ArchiveItem> archives = new();
    private string selectedArchivePath;

    public MainPage()
    {
        InitializeComponent();
        ArchiveListView.ItemsSource = archives;
        LoadArchives();
    }

    private void LoadArchives()
    {
        archives.Clear();
        var externalDir = Android.App.Application.Context.GetExternalFilesDir(null)?.AbsolutePath;
        if (!string.IsNullOrEmpty(externalDir))
        {
            foreach (var dir in Directory.GetDirectories(externalDir))
            {
                var dirInfo = new DirectoryInfo(dir);
                var fileCount = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
                archives.Add(new ArchiveItem
                {
                    Name = dirInfo.Name,
                    FileCount = fileCount,
                    Size = GetDirectorySize(dir),
                    Path = dir
                });
            }
        }
    }

    private string GetDirectorySize(string path)
    {
        try
        {
            var size = new DirectoryInfo(path).EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            if (size < 1024) return $"{size} B";
            if (size < 1024 * 1024) return $"{size / 1024:F1} KB";
            if (size < 1024 * 1024 * 1024) return $"{size / (1024 * 1024):F1} MB";
            return $"{size / (1024 * 1024 * 1024):F1} GB";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async void OnSelectArchiveClicked(object sender, System.EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select Game Archive",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/octet-stream", "application/x-archive" } }
            })
        });

        if (result != null)
        {
            selectedArchivePath = result.FullPath;
            ArchiveInfoLabel.Text = $"Selected: {result.FileName}";
        }
    }

    private void OnArchiveTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ArchiveItem item)
        {
            selectedArchivePath = item.Path;
            ArchiveInfoLabel.Text = $"Selected: {item.Name}";
        }
    }

    private async void OnLoadClicked(object sender, System.EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedArchivePath))
        {
            await DisplayAlert("Error", "Please select an archive first", "OK");
            return;
        }

        await Navigation.PushAsync(new AssetExplorerPage(selectedArchivePath));
    }

    private void OnClearClicked(object sender, System.EventArgs e)
    {
        selectedArchivePath = null;
        ArchiveInfoLabel.Text = "No archive selected";
        ArchiveListView.SelectedItem = null;
    }
}

public class ArchiveItem
{
    public string Name { get; set; }
    public int FileCount { get; set; }
    public string Size { get; set; }
    public string Path { get; set; }
}