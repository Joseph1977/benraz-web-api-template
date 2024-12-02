using _MicroserviceTemplate_.Domain.MyTables;
using _MicroserviceTemplate_.WebApi.Models.MyTables;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace _MicroserviceTemplate_.WebApi.IntegrationTests
{
    [TestFixture]
    public class MyTableControllerTests : ControllerTestsBase
    {
        [Test]
        public async Task CanGetAllEntitiesAsync()
        {
            await AddDefaultEntityAsync();
            await AddDefaultEntityAsync();

            var responseContentString = await HttpClient.GetStringAsync("/v1/MyTable");
            var viewModels =
                JsonConvert.DeserializeObject<IEnumerable<MyTableViewModel>>(responseContentString);

            viewModels.Should().NotBeNull();
            viewModels.Should().HaveCount(2);
        }

        [Test]
        public async Task CanGetEntityByIdAsync()
        {
            var entity = await AddDefaultEntityAsync();

            var responseContentString = await HttpClient.GetStringAsync($"/v1/MyTable/{entity.Id}");
            var viewModel = JsonConvert.DeserializeObject<MyTableViewModel>(responseContentString);

            viewModel.Should().NotBeNull();
        }

        [Test]
        public async Task CanAddEntityAsync()
        {
            var viewModel = new AddMyTableViewModel
            {
                Value = "Value001",
                Description = "Description001"
            };
            var response = await HttpClient.PostAsJsonAsync("/v1/MyTable", viewModel);
            var responseContentString = await response.Content.ReadAsStringAsync();

            responseContentString.Should().NotBeNullOrEmpty();
            GetDbSet().Should().HaveCount(1);
        }

        [Test]
        public async Task CanChangeEntityAsync()
        {
            var entity = await AddDefaultEntityAsync();

            var viewModel = new ChangeMyTableViewModel
            {
                Value = "Value001",
                Description = "Description001"
            };
            var response = await HttpClient.PutAsJsonAsync($"/v1/MyTable/{entity.Id}", viewModel);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            GetDbSet().Should().HaveCount(1);
        }

        [Test]
        public async Task CanDeleteEntityAsync()
        {
            var entity = await AddDefaultEntityAsync();

            var response = await HttpClient.DeleteAsync($"/v1/MyTable/{entity.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            GetDbSet().Should().HaveCount(0);
        }

        protected override async Task ClearDataAsync()
        {
            await base.ClearDataAsync();

            GetDbSet().RemoveRange(GetDbSet());
            await DBContext.SaveChangesAsync();
        }

        protected DbSet<MyTable> GetDbSet()
        {
            return DBContext.MyTables;
        }

        private async Task<MyTable> AddDefaultEntityAsync()
        {
            var entity = CreateDefaultEntity();
            GetDbSet().Add(entity);
            await DBContext.SaveChangesAsync();

            return entity;
        }

        private MyTable CreateDefaultEntity()
        {
            return new MyTable
            {
                Id = Guid.NewGuid(),
                Value = "Value001",
                Description = "Description001"
            };
        }
    }
}
