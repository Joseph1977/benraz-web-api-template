using Benraz.Infrastructure.Common.Repositories;

namespace _MicroserviceTemplate_.Domain.MyTables
{
    /// <summary>
    /// My table repository.
    /// </summary>
    public interface IMyTablesRepository : IRepository<string, MyTable>
    {
    }
}
