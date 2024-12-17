using Benraz.Infrastructure.Common.EntityBase;
using Benraz.Infrastructure.Common.EnvironmentConfiguration;
using Benraz.Infrastructure.Common.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
#if SQLSERVER
using _MicroserviceTemplate_.EF.SqlServer;
#else
using _MicroserviceTemplate_.EF.PostgreSql;
#endif

namespace _MicroserviceTemplate_.EF.Tests
{
    [TestFixture]
    public abstract class _MicroserviceTemplate_RepositoryTestsBase<TKey, TEntity, TRepository>
        where TEntity : class, IAggregateRoot<TKey>
        where TRepository : IRepository<TKey, TEntity>
    {
        protected IConfiguration configuration;

        [SetUp]
        public virtual async Task SetUpAsync()
        {
            // Load EnvironmentVariables.json
            var launchSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "EnvironmentVariables.json");

            if (File.Exists(launchSettingsPath))
            {
                var jsonContent = File.ReadAllText(launchSettingsPath);
                var jsonReader = new JsonTextReader(new StringReader(jsonContent));
                var launchSettings = JObject.Load(jsonReader);

                var environmentVariables = launchSettings.Root;
                if (environmentVariables != null)
                {
                    foreach (JProperty variable in environmentVariables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }

            configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Properties/EnvironmentVariables.json", optional: true, reloadOnChange: true)
            .Add(new CustomEnvironmentVariableConfigurationSource())
            .Build();

            if (!IsCheckConnectionStringExists())
            {
                Assert.Ignore("Connection string is missing. Skipping test.");
            }
            else
            {
                await CreateContext().Database.MigrateAsync();
                await ClearDataAsync();
            }
        }

        [TearDown]
        public virtual async Task TearDownAsync()
        {
            if (IsCheckConnectionStringExists())
            {
                await ClearDataAsync();
            }
        }

        [Test]
        public async Task AddAsync_NewEntity_AddsEntity()
        {
            var entity = CreateDefaultEntity();

            await CreateRepository().AddAsync(entity);
            entity = RoundToMilliseconds(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity = RoundToMilliseconds(dbEntity);

            dbEntity.Should().BeEquivalentTo(entity);
        }

        [Test]
        public async Task ChangeAsync_OldEntity_ChangesEntity()
        {
            var entity = CreateDefaultEntity();
            await CreateRepository().AddAsync(entity);

            entity = ChangeEntity(entity);
            await CreateRepository().ChangeAsync(entity);
            entity = RoundToMilliseconds(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity = RoundToMilliseconds(dbEntity);

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
            entity = RoundToMilliseconds(entity);

            var dbEntity = await CreateRepository().GetByIdAsync(entity.Id);
            dbEntity = RoundToMilliseconds(dbEntity);
            
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
            var builder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>();
            if (IsCheckConnectionStringExists())
            {
                string connectionString = configuration.GetValue<string>("ConnectionStrings");
                #if SQLSERVER
                if (configuration.GetValue<bool>("InjectDBCredentialFromEnvironment"))
                {
                    connectionString +=
                        $";User Id={configuration.GetValue<string>("AspNetCoreDbUserName")};Password={configuration.GetValue<string>("AspNetCoreDbPassword")}";
                }
                builder.UseSqlServer(connectionString);
                #else
                if (configuration.GetValue<bool>("InjectDBCredentialFromEnvironment"))
                {
                    connectionString +=
                        $";Username={configuration.GetValue<string>("AspNetCoreDbUserName")};Password={configuration.GetValue<string>("AspNetCoreDbPassword")}";
                }
                builder.UseNpgsql(connectionString);
                #endif
            }
            return builder;
        }

        public bool IsCheckConnectionStringExists()
        {
            if (configuration.GetValue<bool>("SkipDbTestIfNoConnectionString"))
            {
                return false;
            }
            else
            {
                string connectionString = configuration.GetValue<string>("ConnectionStrings");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return false;
                }
                return true;
            }
        }

        private static TEntity RoundToMilliseconds<TEntity>(TEntity entity)
        {
            // Get the type of the entity
            var entityType = typeof(TEntity);

            // Check if the 'CreateTimeUtc' property exists and is of type DateTime
            var createTimeUtcProperty = entityType.GetProperty("CreateTimeUtc");
            if (createTimeUtcProperty != null && createTimeUtcProperty.PropertyType == typeof(DateTime))
            {
                var createTimeUtcValue = (DateTime)createTimeUtcProperty.GetValue(entity);
                // Round to milliseconds
                createTimeUtcProperty.SetValue(entity, new DateTime((createTimeUtcValue.Ticks / TimeSpan.TicksPerMillisecond) * TimeSpan.TicksPerMillisecond));
            }

            // Check if the 'UpdateTimeUtc' property exists and is of type DateTime
            var updateTimeUtcProperty = entityType.GetProperty("UpdateTimeUtc");
            if (updateTimeUtcProperty != null && updateTimeUtcProperty.PropertyType == typeof(DateTime))
            {
                var updateTimeUtcValue = (DateTime)updateTimeUtcProperty.GetValue(entity);
                // Round to milliseconds
                updateTimeUtcProperty.SetValue(entity, new DateTime((updateTimeUtcValue.Ticks / TimeSpan.TicksPerMillisecond) * TimeSpan.TicksPerMillisecond));
            }

            return entity;
        }
    }
}