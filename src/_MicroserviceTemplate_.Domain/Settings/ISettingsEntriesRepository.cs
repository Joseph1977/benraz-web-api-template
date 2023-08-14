using Benraz.Infrastructure.Common.Repositories;

namespace _MicroserviceTemplate_.Domain.Settings
{
    /// <summary>
    /// Settings entries repository.
    /// </summary>
    public interface ISettingsEntriesRepository : IRepository<string, SettingsEntry>
    {
    }
}