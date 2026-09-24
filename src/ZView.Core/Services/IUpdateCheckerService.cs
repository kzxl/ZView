using System;
using System.Threading;
using System.Threading.Tasks;
using ZView.Core.Models;

namespace ZView.Core.Services
{
    public interface IUpdateCheckerService
    {
        Task<UpdateInfo?> CheckForUpdateAsync(string repoOwner, string repoName, string currentVersionStr, CancellationToken ct = default);
        Task<string?> DownloadReleaseAssetAsync(string downloadUrl, string destinationPath, IProgress<double>? progress = null, CancellationToken ct = default);
    }
}
