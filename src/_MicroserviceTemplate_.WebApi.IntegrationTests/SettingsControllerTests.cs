using _MicroserviceTemplate_.Domain.Settings;
using _MicroserviceTemplate_.WebApi.Models.Settings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using Benraz.Infrastructure.Common.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace _MicroserviceTemplate_.WebApi.IntegrationTests
{
    [TestFixture]
    public class SettingsControllerTests : ControllerTestsBase
    {
        [Test]
        public async Task CanGetAllEntitiesAsync()
        {
            await AddDefaultEntityAsync();
            await AddDefaultEntityAsync();

            var responseContentString = await HttpClient.GetStringAsync("/v1/Settings");
            var viewModels =
                JsonConvert.DeserializeObject<IEnumerable<SettingsEntryViewModel>>(responseContentString);

            viewModels.Should().NotBeNull();
            viewModels.Should().HaveCount(2);
        }

        [Test]
        public async Task CanGetEntityByIdAsync()
        {
            var entity = await AddDefaultEntityAsync();

            var responseContentString = await HttpClient.GetStringAsync($"/v1/Settings/{entity.Id}");
            var viewModel = JsonConvert.DeserializeObject<SettingsEntryViewModel>(responseContentString);

            viewModel.Should().NotBeNull();
        }

        [Test]
        public async Task CanAddEntityAsync()
        {
            var viewModel = new AddSettingsEntryViewModel
            {
                Id = "Id001",
                Value = "Value001",
                Description = "Description001"
            };
            var response = await HttpClient.PostAsJsonAsync("/v1/Settings", viewModel);
            var responseContentString = await response.Content.ReadAsStringAsync();

            responseContentString.Should().NotBeNullOrEmpty();
            GetDbSet().Should().HaveCount(1);
        }

        [Test]
        public async Task CanChangeEntityAsync()
        {
            var entity = await AddDefaultEntityAsync();

            var viewModel = new ChangeSettingsEntryViewModel
            {
                Value = "Value001"
            };
            var response = await HttpClient.PutAsJsonAsync($"/v1/Settings/{entity.Id}", viewModel);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            GetDbSet().Should().HaveCount(1);
        }

        [Test]
        public async Task CanDeleteEntityAsync()
        {
            var entity = await AddDefaultEntityAsync();

            var response = await HttpClient.DeleteAsync($"/v1/Settings/{entity.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            GetDbSet().Should().HaveCount(0);
        }

        protected override async Task ClearDataAsync()
        {
            await base.ClearDataAsync();

            GetDbSet().RemoveRange(GetDbSet());
            await DBContext.SaveChangesAsync();
        }

        protected DbSet<SettingsEntry> GetDbSet()
        {
            return DBContext.SettingsEntries;
        }

        private async Task<SettingsEntry> AddDefaultEntityAsync()
        {
            var entity = CreateDefaultEntity();
            GetDbSet().Add(entity);
            await DBContext.SaveChangesAsync();

            return entity;
        }

        private SettingsEntry CreateDefaultEntity()
        {
            return new SettingsEntry
            {
                Id = Guid.NewGuid().ToString(),
                Value = "Value001",
                Description = "Description001"
            };
        }
    }
}