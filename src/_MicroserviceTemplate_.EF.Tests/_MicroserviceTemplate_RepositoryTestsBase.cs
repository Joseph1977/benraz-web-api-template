using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Benraz.Infrastructure.Common.EntityBase;
using Benraz.Infrastructure.Common.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace _MicroserviceTemplate_.EF.Tests
{
    [TestFixture]
    public abstract class _MicroserviceTemplate_RepositoryTestsBase<TKey, TEntity, TRepository>
        where TEntity : class, IAggregateRoot<TKey>
        where TRepository : IRepository<TKey, TEntity>
    {
        [SetUp]
        public virtual async Task SetUpAsync()
        {
            await CreateContext().Database.MigrateAsync();
            await ClearDataAsync();
        }

        [TearDown]
        public virtual async Task TearDownAsync()
        {
            await ClearDataAsync();
        }

        [Test]
        public async Task AddAsync_NewEntity_AddsEntity()
        {
            var entity = CreateDefaultEntity();

            await CreateRepository().AddAsync(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity.Should().BeEquivalentTo(entity);
        }

        [Test]
        public async Task ChangeAsync_OldEntity_ChangesEntity()
        {
            var entity = CreateDefaultEntity();
            await CreateRepository().AddAsync(entity);

            entity = ChangeEntity(entity);
            await CreateRepository().ChangeAsync(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity.Should().BeEquivalentTo(entity);
        }

        [Test]
        public async Task ChangeAsync_OldEntityAndSameContext_ChangesEntity()
        {
            var repository = CreateRepository();
            var entity = CreateDefaultEntity();
            await repository.AddAsync(entity);

            entity = ChangeEntity(entity);
            await repository.ChangeAsync(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity.Should().BeEquivalentTo(entity);
        }

        [Test]
        public async Task RemoveAsync_OldEntity_RemovesEntity()
        {
            var entity = CreateDefaultEntity();
            await CreateRepository().AddAsync(entity);

            await CreateRepository().RemoveAsync(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity.Should().BeNull();
        }

        [Test]
        public async Task GetAsync_OneEntity_ReturnsOneEntity()
        {
            await CreateRepository().AddAsync(CreateDefaultEntity());

            var dbEntities = await CreateRepository().GetAllAsync();

            dbEntities.Should().HaveCount(1);
        }

        protected abstract TEntity CreateDefaultEntity();
        protected abstract TEntity ChangeEntity(TEntity entity);
        protected abstract TRepository CreateRepository();

        protected virtual async Task ClearDataAsync()
        {
            await CommitAsync(x => x.Set<TEntity>().RemoveRange(x.Set<TEntity>()));
        }

        protected async Task CommitAsync(Action<_MicroserviceTemplate_DbContext> action)
        {
            using (var context = CreateContext())
            {
                action(context);
                await context.SaveChangesAsync();
            }
        }

        protected _MicroserviceTemplate_DbContext CreateContext()
        {
            return new _MicroserviceTemplate_DbContext(CreateContextBuilder().Options);
        }

        protected DbContextOptionsBuilder<_MicroserviceTemplate_DbContext> CreateContextBuilder()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>();
            builder.UseSqlServer(configuration.GetConnectionString("_MicroserviceTemplate_"));

            return builder;
        }
    }
}