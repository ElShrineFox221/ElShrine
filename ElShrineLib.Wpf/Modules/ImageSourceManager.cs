using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElShrine.Modules;

public sealed class ImageSourceManager
{
    private readonly ConcurrentDictionary<string, ImageSource?> imageCache = new();
    private readonly ConcurrentDictionary<string, Task<ImageSource?>> downloadTasks = new();
    private static readonly HttpClient httpClient = new();
    public ImageSourceManager() { }
    public Task<ImageSource?> GetImageSourceAsync(string url, int decodePixelsW = -1, int decodePixelsH = -1)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Task.FromResult<ImageSource?>(null);
        }
        if (imageCache.TryGetValue(url, out var cachedImage))
        {
            return Task.FromResult(cachedImage);
        }
        return downloadTasks.GetOrAdd(url, async (key) =>
        {
            try
            {
                // 核心改动：调用区分了本地和网络的加载方法
                var image = await LoadImageSourceDataAsync(key, decodePixelsW, decodePixelsH);

                // 3. 缓存结果
                if (image is not null) imageCache.TryAdd(key, image);
                return image;
            }
            finally
            {
                // 4. 移除任务，无论成功或失败
                downloadTasks.TryRemove(key, out _);
            }
        });
    }
    /// <summary>
    /// 根据 URL 类型（网络/本地）加载字节数据并转换为 ImageSource。
    /// </summary>
    private static async Task<ImageSource?> LoadImageSourceDataAsync(string url, int decodePixelsW, int decodePixelsH)
    {
        bool isWebUrl = url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        try
        {
            byte[]? imageBytes;
            if (isWebUrl) imageBytes = await httpClient.GetByteArrayAsync(url);
            else if (File.Exists(url)) imageBytes = await File.ReadAllBytesAsync(url);
            else return null;

            if (imageBytes == null || imageBytes.Length == 0) return null;
            return await ConvertBytesToImageSourceAsync(imageBytes, decodePixelsW, decodePixelsH);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// 将字节数组转换为 ImageSource（从 DownloadAndConvertToImageSourceAsync 提取）
    /// </summary>
    private static Task<ImageSource?> ConvertBytesToImageSourceAsync(byte[] imageBytes, int decodePixelsW, int decodePixelsH)
    {
        return Task.Run<ImageSource?>(() =>
        {
            try
            {
                using var stream = new MemoryStream(imageBytes);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; 

                if (decodePixelsW > 0) bitmapImage.DecodePixelWidth = decodePixelsW;
                if (decodePixelsH > 0) bitmapImage.DecodePixelHeight = decodePixelsH;

                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                if (bitmapImage.CanFreeze) bitmapImage.Freeze();

                return bitmapImage; // Wpf 隐含的从 BitmapImage 到 ImageSource 的转换
            }
            catch
            {
                return null;
            }
        });
    }
}
