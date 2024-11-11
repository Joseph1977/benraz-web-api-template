using System.Threading.Tasks;

namespace _MicroserviceTemplate_.Domain.Jobs
{
    /// <summary>
    /// Empty repeatable jobs service.
    /// </summary>
    public class EmptyRepeatableJobsService : IEmptyRepeatableJobsService
    {
        /// <summary>
        /// Creates service.
        /// </summary>
        public EmptyRepeatableJobsService()
        {
        }

        /// <summary>
        /// Returns if execution is in processing.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>If execution is in processing.</returns>
        public Task<bool> IsInProcessingAsync(object args = null)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Returns if execution is on cooldown.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>If execution is on cooldown.</returns>
        public Task<bool> IsOnCooldownAsync(object args = null)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Cleans up stale jobs.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>Task.</returns>
        public Task CleanUpStaleAsync(object args = null)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processes new job.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>Task.</returns>
        public Task ProcessAsync(object args = null)
        {
            return Task.CompletedTask;
        }
    }
}