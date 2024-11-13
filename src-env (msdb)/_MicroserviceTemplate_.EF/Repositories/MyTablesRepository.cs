using _MicroserviceTemplate_.Domain.MyTables;
using Benraz.Infrastructure.EF;

namespace _MicroserviceTemplate_.EF.Repositories
{
    /// <summary>
    /// My tables repository.
    /// </summary>
    public class MyTablesRepository :
        RepositoryBase<string, MyTable, _MicroserviceTemplate_DbContext>,
        IMyTablesRepository
    {
        /// <summary>
        /// Creates repository.
        /// </summary>
        /// <param name="context">Context.</param>
        public MyTablesRepository(_MicroserviceTemplate_DbContext context)
            : base(context)
        {
        }
    }
}
