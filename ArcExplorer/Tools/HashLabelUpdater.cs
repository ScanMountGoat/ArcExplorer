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

        public static readonly string HashesPath = "Hashes.txt";


        private HashLabelUpdater()
        {

        }

        /// <summary>
        /// Attempts to find a newer commit than the current commit for the hash labels.
        /// </summary>
        /// <returns></returns>
        public async Task<GitHubCommit?> TryFindNewerHashesCommit()
        {
            // Github doesn't store creation/modified times for files and the newly downloaded file will have the current time as its created date.
            // In order to accurately determine whether an update is available, compare the dates for the current and latest commit.
            // This may result in a redundant update if the user decides to update the file locally.
            var latestHashesCommit = await GetLatestArchiveHashesCommit();
            var currentHashesCommit = await GetCurrentCommit();

            // If the current hashes can't be found for some reason, try and update to fix the potentially invalid commit SHA.
            if ((latestHashesCommit?.Commit.Author.Date.UtcDateTime > currentHashesCommit?.Commit.Author.Date.UtcDateTime) || currentHashesCommit == null)
            {
                return latestHashesCommit;
            }

            return null;
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
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("arc-explorer"));
                var commit = await client.Repository.Commit.GetAll("ultimate-research", "archive-hashes",
                    new CommitRequest { Sha = Models.ApplicationSettings.Instance.CurrentHashesCommitSha });
                return commit?.FirstOrDefault();
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, "Error checking for current hashes commit");
                return null;
            }

        }

        private static async Task<GitHubCommit?> GetLatestArchiveHashesCommit()
        {
            try
            {
                // TODO: This assumes the commit modified the archive hashes file.
                var client = new GitHubClient(new ProductHeaderValue("arc-explorer"));
                var commit = await client.Repository.Commit.Get("ultimate-research", "archive-hashes", "master");
                return commit;
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, "Error checking for latest hashes");
                return null;
            }
        }
    }
}
