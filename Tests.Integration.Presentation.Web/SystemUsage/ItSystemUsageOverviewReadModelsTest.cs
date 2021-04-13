using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage.Read;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage
{
    public class ItSystemUsageOverviewReadModelsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Query_And_Page_ReadModels()
        {
            //Arrange
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name1, organizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name2, organizationId, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name3, organizationId, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system2.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system3.Id, organizationId);


            //Act
            var page1 = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

            //Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(name1, page1.First().Name);
            Assert.Equal(name2, page1.Last().Name);

            Assert.Single(page2);
            Assert.Equal(name3, page2.Single().Name);
        }

        [Fact]
        public async Task ReadModels_Contain_Correct_Content()
        {
            //Arrange
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var systemDisabled = A<bool>();
            var systemUsageActive = A<bool>();
            var systemUsageExpirationDate = DateTime.Now.AddDays(-1);

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            // System changes
            await ItSystemHelper.SendSetDisabledRequestAsync(system.Id, systemDisabled);

            // System Usage changes
            await ItSystemUsageHelper.SendSetActiveRequestAsync(systemUsage.Id, organizationId, systemUsageActive);
            await ItSystemUsageHelper.SendSetExpirationDateRequestAsync(systemUsage.Id, organizationId, systemUsageExpirationDate); //Only rely on the value of "Active" of system usage

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            // From System Usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(organizationId, readModel.OrganizationId);
            Assert.Equal(systemUsageActive, readModel.IsActive);

            // From System
            Assert.Equal(systemName, readModel.Name);
            Assert.Equal(systemDisabled, readModel.ItSystemDisabled);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_True_When_ExpirationDate_Is_Today()
        {
            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(DateTime.Now);

            //Assert
            Assert.True(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_True_When_ExpirationDate_Is_After_Today()
        {
            //Arrange
            var expirationDate = DateTime.Now.AddDays(A<int>());

            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(expirationDate);

            //Assert
            Assert.True(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_False_When_ExpirationDate_Is_Earlier_Than_Today()
        {
            //Arrange
            var expirationDate = DateTime.Now.AddDays(-A<int>());

            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(expirationDate);

            //Assert
            Assert.False(readModel.IsActive);
        }


        private async Task<ItSystemUsageOverviewReadModel> Test_For_IsActive_Based_On_ExpirationDate(DateTime expirationDate)
        {
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);
            await ItSystemUsageHelper.SendSetExpirationDateRequestAsync(systemUsage.Id, organizationId, expirationDate);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");


            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            return readModel;
        }

        private static async Task WaitForReadModelQueueDepletion()
        {
            await WaitForAsync(
                () =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<PendingReadModelUpdate, bool>(x => !x.AsQueryable().Any()));
                }, TimeSpan.FromSeconds(30));
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
