using Benraz.Infrastructure.Common.Repositories;
using System;

namespace _MicroserviceTemplate_.Domain.MyTables
{
    /// <summary>
    /// My table repository.
    /// </summary>
    public interface IMyTablesRepository : IRepository<Guid, MyTable>
    {
    }
}
