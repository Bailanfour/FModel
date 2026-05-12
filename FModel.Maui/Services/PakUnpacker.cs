using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.Wwise;
using CUE4Parse_Conversion;
using Serilog;

namespace FModel.Maui.Services;

public class PakUnpacker : IDisposable
{
    public event EventHandler<string> OnProgress;
    public event EventHandler<int> OnProgressPercentage;
    
    private DefaultFileProvider _provider;
    private string _outputDirectory;
    private MeshConverter _meshConverter;

    public async Task InitializeAsync(string pakPath, string outputDir)
    {
        _outputDirectory = outputDir;
        Directory.CreateDirectory(outputDir);

        OnProgress?.Invoke(this, "Initializing file provider...");
        
        _provider = new DefaultFileProvider(pakPath, SearchOption.AllDirectories, true);
        await _provider.InitializeAsync();
        
        _meshConverter = new MeshConverter(_provider);
        
        OnProgress?.Invoke(this, "Loading packages...");
        await _provider.LoadAllAsync();
        
        OnProgress?.Invoke(this, "Ready!");
    }

    public async Task AddAesKeyAsync(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            try
            {
                var aesKey = new FAesKey(key);
                _provider.AddAesKey(aesKey);
                await _provider.LoadAllAsync();
                OnProgress?.Invoke(this, $"Added AES key: {key.Substring(0, 8)}...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add AES key");
                OnProgress?.Invoke(this, $"Invalid AES key format");
            }
        }
    }

    public IEnumerable<string> GetAllAssets()
    {
        return _provider.Files.Keys.Where(k => 
            k.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase) ||
            k.EndsWith(".umap", StringComparison.OrdinalIgnoreCase))
            .Select(k => k.Replace("../../../", ""));
    }

    public int GetAssetCount()
    {
        return GetAllAssets().Count();
    }

    public async Task ExportAssetAsync(string assetPath, CancellationToken token)
    {
        OnProgress?.Invoke(this, $"Exporting: {Path.GetFileName(assetPath)}");
        
        try
        {
            var fullPath = $"../../../{assetPath}";
            
            if (!_provider.Files.TryGetValue(fullPath, out var file))
            {
                return;
            }

            var export = await _provider.LoadObjectAsync(fullPath, token);
            if (export == null)
            {
                return;
            }

            var ext = GetAssetExtension(export);
            var outputPath = Path.Combine(_outputDirectory, assetPath);
            var outputFolder = Path.GetDirectoryName(outputPath);
            
            if (!string.IsNullOrEmpty(outputFolder))
                Directory.CreateDirectory(outputFolder);

            await ExportAssetToFile(export, outputPath, ext, token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to export {assetPath}");
        }
    }

    public async Task ExportAllAssetsAsync(CancellationToken token)
    {
        var assets = GetAllAssets().ToList();
        var total = assets.Count;
        var current = 0;

        OnProgress?.Invoke(this, $"Found {total} assets to export");

        foreach (var asset in assets)
        {
            if (token.IsCancellationRequested)
                break;

            await ExportAssetAsync(asset, token);
            current++;
            OnProgressPercentage?.Invoke(this, (int)((current * 100) / total));
        }
    }

    private string GetAssetExtension(UObject export)
    {
        return export switch
        {
            UTexture2D => ".png",
            USkeletalMesh => ".obj",
            UStaticMesh => ".obj",
            USoundWave => ".wav",
            _ => ".json"
        };
    }

    private async Task ExportAssetToFile(UObject export, string outputPath, string ext, CancellationToken token)
    {
        var finalPath = Path.ChangeExtension(outputPath, ext);

        switch (export)
        {
            case UTexture2D texture:
                await ExportTexture(texture, finalPath);
                break;
            
            case USkeletalMesh mesh:
                ExportSkeletalMesh(mesh, finalPath);
                break;
            
            case UStaticMesh mesh:
                ExportStaticMesh(mesh, finalPath);
                break;
            
            case USoundWave sound:
                await ExportSound(sound, finalPath);
                break;
            
            default:
                await ExportAsJson(export, finalPath);
                break;
        }
    }

    private async Task ExportTexture(UTexture2D texture, string outputPath)
    {
        try
        {
            var bitmap = texture.Decode();
            if (bitmap != null)
            {
                using var ms = new MemoryStream();
                await Task.Run(() => 
                {
                    using var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(
                        bitmap.Data, bitmap.Width, bitmap.Height);
                    image.SaveAsPng(outputPath);
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export texture");
        }
    }

    private void ExportSkeletalMesh(USkeletalMesh mesh, string outputPath)
    {
        try
        {
            var objData = _meshConverter.ExportSkeletalMeshAsObj(mesh);
            File.WriteAllText(outputPath, objData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export skeletal mesh");
        }
    }

    private void ExportStaticMesh(UStaticMesh mesh, string outputPath)
    {
        try
        {
            var objData = _meshConverter.ExportStaticMeshAsObj(mesh);
            File.WriteAllText(outputPath, objData);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export static mesh");
        }
    }

    private async Task ExportSound(USoundWave sound, string outputPath)
    {
        try
        {
            var data = sound.GetDecodedAudioData();
            if (data != null && data.Length > 0)
            {
                await File.WriteAllBytesAsync(outputPath, data);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export sound");
        }
    }

    private async Task ExportAsJson(UObject export, string outputPath)
    {
        try
        {
            var json = export.GetJson();
            await File.WriteAllTextAsync(outputPath, json);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export as JSON");
        }
    }

    public void Dispose()
    {
        _provider?.Dispose();
    }
}