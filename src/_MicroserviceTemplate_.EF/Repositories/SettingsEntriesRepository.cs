using _MicroserviceTemplate_.Domain.Settings;
using Benraz.Infrastructure.EF;

namespace _MicroserviceTemplate_.EF.Repositories
{
    /// <summary>
    /// Settings entries repository.
    /// </summary>
    public class SettingsEntriesRepository :
        RepositoryBase<string, SettingsEntry, _MicroserviceTemplate_DbContext>,
        ISettingsEntriesRepository
    {
        /// <summary>
        /// Creates repository.
        /// </summary>
        /// <param name="context">Context.</param>
        public SettingsEntriesRepository(_MicroserviceTemplate_DbContext context)
            : base(context)
        {
        }
    }
}