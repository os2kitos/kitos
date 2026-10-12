using Core.DomainServices.Organizations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class StsTokenFetcherTest
    {
        [Fact]
        public void Can_Register_And_Resolve_StsOrganisationTypes()
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => Mock.Of<IStsOrganizationCompanyLookupService>());
            services.AddScoped(_ => Mock.Of<IStsOrganizationService>());
            services.AddScoped(_ => Mock.Of<IStsOrganizationSystemService>());

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IStsOrganizationCompanyLookupService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IStsOrganizationService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IStsOrganizationSystemService>());
        }
    }
}
