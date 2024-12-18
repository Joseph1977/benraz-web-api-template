using _MicroserviceTemplate_.Domain.MyTables;
using Benraz.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace _MicroserviceTemplate_.EF.Data.Repositories
{
    /// <summary>
    /// My tables repository.
    /// </summary>
    public class MyTablesRepository :
        RepositoryBase<Guid, MyTable, DbContext>,
        IMyTablesRepository
    {
        /// <summary>
        /// Creates repository.
        /// </summary>
        /// <param name="context">Context.</param>
        public MyTablesRepository(DbContext context)
            : base(context)
        {
        }
    }
}
