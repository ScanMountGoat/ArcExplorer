using Octokit;
using System;
using System.Threading.Tasks;
using SerilogTimings;
using System.Net;

namespace ArcExplorer.Tools
{
    internal class HashLabelUpdater
    {
        public static HashLabelUpdater Instance { get; } = new HashLabelUpdater();

        // 1. check if hashes can be updated
        // 2. prompt user to update 
        // 3. download hashes file (block opening ARC until done)
        public Commit? LatestHashesCommit { get; private set; } = null;

        private HashLabelUpdater()
        {

        }

        // TODO: This can be async.
        public async Task<bool> CanUpdateHashes(string pathToCurrentLabels)
        {
            try
            {
                LatestHashesCommit = await GetLatestArchiveHashesCommit();

                var githubCommitTime = LatestHashesCommit?.Author.Date.UtcDateTime;
                var localUpdateTime = System.IO.File.GetLastWriteTimeUtc(pathToCurrentLabels);

                return githubCommitTime > localUpdateTime;
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, "Error checking for latest hashes");
                return false;
            }
        }

        public async Task DownloadHashes(string pathToCurrentLabels)
        {
            // TODO: Log information about what commit is being used.
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

        private static async Task<Commit?> GetLatestArchiveHashesCommit()
        {
            var client = new GitHubClient(new ProductHeaderValue("arc-explorer"));
            var commit = await client.Repository.Commit.Get("ultimate-research", "archive-hashes", "master");
            return commit?.Commit;
        }
    }
}
