using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZView.Core.Models;

namespace ZView.Core.Services
{
    public class GitHubUpdateCheckerService : IUpdateCheckerService
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ZeroUniverse-ZView", "1.0"));
            client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }

        public async Task<UpdateInfo?> CheckForUpdateAsync(string repoOwner, string repoName, string currentVersionStr, CancellationToken ct = default)
        {
            try
            {
                string url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
                using var response = await HttpClient.GetAsync(url, ct).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
                string releaseTitle = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? tagName : tagName;
                string body = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";
                string htmlUrl = root.TryGetProperty("html_url", out var htmlProp) ? htmlProp.GetString() ?? "" : "";
                DateTime publishedAt = root.TryGetProperty("published_at", out var pubProp) && pubProp.TryGetDateTime(out var dt) ? dt : DateTime.UtcNow;

                string cleanLatest = tagName.TrimStart('v', 'V');
                string cleanCurrent = currentVersionStr.TrimStart('v', 'V');

                bool hasUpdate = IsNewerVersion(cleanLatest, cleanCurrent);

                string? assetUrl = null;
                string? assetName = null;
                long assetSize = 0;

                if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var asset in assetsProp.EnumerateArray())
                    {
                        string name = asset.TryGetProperty("name", out var aName) ? aName.GetString() ?? "" : "";
                        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            assetName = name;
                            assetUrl = asset.TryGetProperty("browser_download_url", out var dlProp) ? dlProp.GetString() : null;
                            assetSize = asset.TryGetProperty("size", out var sProp) && sProp.TryGetInt64(out var sz) ? sz : 0;
                            break;
                        }
                    }
                }

                return new UpdateInfo
                {
                    HasUpdate = hasUpdate,
                    LatestVersion = tagName,
                    CurrentVersion = currentVersionStr,
                    ReleaseTitle = releaseTitle,
                    ReleaseNotes = body,
                    HtmlUrl = htmlUrl,
                    AssetDownloadUrl = assetUrl,
                    AssetFileName = assetName,
                    AssetSizeBytes = assetSize,
                    PublishedAt = publishedAt
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> DownloadReleaseAssetAsync(string downloadUrl, string destinationPath, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl)) return null;

            try
            {
                string? dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                using var response = await HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                using var contentStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read, ct).ConfigureAwait(false);
                    totalRead += read;
                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        progress?.Report((double)totalRead / totalBytes.Value);
                    }
                }

                return destinationPath;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsNewerVersion(string latestStr, string currentStr)
        {
            if (Version.TryParse(latestStr, out var latest) && Version.TryParse(currentStr, out var current))
            {
                return latest > current;
            }

            // Fallback to string compare
            return string.Compare(latestStr, currentStr, StringComparison.OrdinalIgnoreCase) > 0;
        }
    }
}
