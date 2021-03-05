using Octokit;
using System;
using System.Threading.Tasks;
using SerilogTimings;
using System.Linq;
using System.Net;

namespace ArcExplorer.Tools
{
    internal class HashLabelUpdater
    {
        public static HashLabelUpdater Instance { get; } = new HashLabelUpdater();


        private HashLabelUpdater()
        {

        }

        /// <summary>
        /// Attempts to find a newer commit than the current commit for the hash labels.
        /// </summary>
        /// <returns></returns>
        public async Task<GitHubCommit?> TryFindNewerHashesCommit()
        {
            try
            {
                // Github doesn't store creation/modified times for files and the newly downloaded file will have the current time as its created date.
                // In order to accurately determine whether an update is available, compare the dates for the current and latest commit.
                // This may result in a redundant update if the user decides to update the file locally.
                var latestHashesCommit = await GetLatestArchiveHashesCommit();
                var currentHashesCommit = await GetCurrentCommit();

                if (latestHashesCommit?.Commit == null || currentHashesCommit?.Commit == null)
                {
                    return null;
                }

                if (latestHashesCommit.Commit.Author.Date.UtcDateTime > currentHashesCommit.Commit.Author.Date.UtcDateTime)
                {
                    return latestHashesCommit;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, "Error checking for latest hashes");
                return null;
            }
        }

        public async Task DownloadHashes(string pathToCurrentLabels)
        {
            using (var operation = Operation.Begin("Updating hashes"))
            {
                try
                {
                    // Replace the existing file using the latest file from Github.
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync("https://github.com/ultimate-research/archive-hashes/raw/master/Hashes", pathToCurrentLabels);
                    }
                    operation.Complete();
                }
                catch (Exception e)
                {
                    operation.Abandon(e);
                }
            }
        }

        private static async Task<GitHubCommit?> GetCurrentCommit()
        {
            // TODO: This assumes the commit modified the archive hashes file.
            var client = new GitHubClient(new ProductHeaderValue("arc-explorer"));
            // TODO: Get the current SHA from user preferences.
            var commit = await client.Repository.Commit.GetAll("ultimate-research", "archive-hashes", new CommitRequest { Sha = "1b2da43a6e4cbeb0809acc2d5f325314a3ea2f72" });
            return commit?.FirstOrDefault();
        }

        private static async Task<GitHubCommit?> GetLatestArchiveHashesCommit()
        {
            // TODO: This assumes the commit modified the archive hashes file.
            var client = new GitHubClient(new ProductHeaderValue("arc-explorer"));
            var commit = await client.Repository.Commit.Get("ultimate-research", "archive-hashes", "master");
            return commit;
        }
    }
}
