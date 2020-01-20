using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Infrastructure.DataAccess;
using Presentation.Web.Models;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.KLE
{
    public class KleUpdateIntegrationTests : IDisposable
    {
        private readonly KitosContext _dbContext;
        private readonly GenericRepository<TaskRef> _taskRefRepository;
        private readonly GenericRepository<KLEUpdateHistoryItem> _kleHistoryRepository;

        public KleUpdateIntegrationTests()
        {
            _dbContext = TestEnvironment.GetDatabase();
            _taskRefRepository = new GenericRepository<TaskRef>(_dbContext);
            _kleHistoryRepository = new GenericRepository<KLEUpdateHistoryItem>(_dbContext);
        }

        public void Dispose()
        {
            _taskRefRepository.Dispose();
            _dbContext.Dispose();
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Get_Kle_Status_Returns_Forbidden(OrganizationRole role)
        {
            //Arrange - make sure update will be "not up to date" by clearing status
            var url = TestEnvironment.CreateUrl("api/v1/kle/status");
            var login = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }

        [Fact]
        public async Task GlobalAdmin_Can_Get_Kle_Status()
        {
            //Arrange - make sure update will be "not up to date" by clearing status
            var url = TestEnvironment.CreateUrl("api/v1/kle/status");
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            ResetKleHistory();

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var statusDto = await response.ReadResponseBodyAsKitosApiResponseAsync<KLEStatusDTO>();
                Assert.True(DateTime.TryParse(statusDto.Version, out var dt), "Failed to parse version as a string");
                Assert.False(statusDto.UpToDate);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Get_Kle_Changes_Returns_Forbidden(OrganizationRole role)
        {
            //Arrange - make sure update will be "not up to date" by clearing status
            var url = TestEnvironment.CreateUrl("api/v1/kle/changes");
            var login = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }

        [Fact]
        public async Task GlobalAdmin_Can_Get_Kle_Changes()
        {
            //Arrange - make sure update will be "not up to date" by clearing status
            var url = TestEnvironment.CreateUrl("api/v1/kle/changes");
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            ResetKleHistory();

            //Act
            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task Put_Kle_Returns_Forbidden(OrganizationRole role)
        {
            //Arrange - make sure update will be "not up to date" by clearing status
            var url = TestEnvironment.CreateUrl("api/v1/kle/update");
            var login = await HttpApi.GetCookieAsync(role);

            //Act
            using (var response = await HttpApi.PutWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }

        [Fact]
        public async Task GlobalAdmin_Can_Put_Kle_Update()
        {
            //Arrange - make sure update will be "not up to date" by clearing status
            var url = TestEnvironment.CreateUrl("api/v1/kle/update");
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            ResetKleHistory();

            //Act
            using (var response = await HttpApi.PutWithCookieAsync(url, login))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        private void ResetKleHistory()
        {
            var all = _dbContext.KLEUpdateHistoryItems.AsQueryable().ToList();
            _kleHistoryRepository.RemoveRange(all);
            _kleHistoryRepository.Save();
        }
    }
}
