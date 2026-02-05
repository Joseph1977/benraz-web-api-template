using _MicroserviceTemplate_.Domain.MyTables;
using NUnit.Framework;
using System;
using _MicroserviceTemplate_.EF.Data.Repositories;

namespace _MicroserviceTemplate_.EF.Tests
{
    [TestFixture]
    public class MyTablesRepositoryTests :
        _MicroserviceTemplate_RepositoryTestsBase<Guid, MyTable, MyTablesRepository>
    {
        protected override MyTable CreateDefaultEntity()
        {
            return new MyTable
            {
                Id = Guid.NewGuid(),
                Value = "MyTableValue001",
                Description = "MyTableDescription001"
            };
        }

        protected override MyTable ChangeEntity(MyTable entity)
        {
            entity.Value = "MyTableValue002";
            entity.Description = "MyTableDescription002";
            return entity;
        }

        protected override MyTablesRepository CreateRepository()
        {
            return new MyTablesRepository(CreateContext());
        }
    }
}
