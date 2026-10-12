using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Archive;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Presentation.Web.Controllers.API.V1.OData;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Controllers.API.V1.OData
{
    public class ItSystemUsageArchivesControllerTest : WithAutoFixture
    {
        [Fact]
        public void ActionDiscovery_OnlyExposesOrganizationScopedGet()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddControllers().AddApplicationPart(typeof(ItSystemUsageArchivesController).Assembly);
            using var provider = services.BuildServiceProvider();

            var actions = provider.GetRequiredService<IActionDescriptorCollectionProvider>()
                .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                .Where(action => action.ControllerTypeInfo.AsType() == typeof(ItSystemUsageArchivesController));

            var action = Assert.Single(actions);
            Assert.Equal(nameof(ItSystemUsageArchivesController.Get), action.ActionName);
            var parameter = Assert.Single(action.Parameters);
            Assert.Equal("organizationUuid", parameter.Name);
            Assert.Equal(typeof(Guid), parameter.ParameterType);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Get_OnlyReturnsArchivesFromRequestedOrganization(bool isGlobalAdmin)
        {
            var organizationUuid = Guid.NewGuid();
            const int organizationId = 42;
            var expected = new ItSystemUsageArchive { OrganizationId = organizationId, Note = "", ArchivingDate = DateTime.Today };
            var repository = new Mock<IGenericRepository<ItSystemUsageArchive>>();
            repository.Setup(x => x.AsQueryable()).Returns(new[]
            {
                expected,
                new ItSystemUsageArchive { OrganizationId = 43, Note = "", ArchivingDate = DateTime.Today }
            }.AsQueryable());
            var resolver = new Mock<IEntityIdentityResolver>();
            resolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(Maybe<int>.Some(organizationId));
            var userContext = new Mock<IOrganizationalUserContext>();
            userContext.Setup(x => x.IsGlobalAdmin()).Returns(isGlobalAdmin);
            userContext.Setup(x => x.OrganizationIds).Returns(new[] { organizationId, 43 });
            var authorization = new Mock<IAuthorizationContext>();
            authorization.Setup(x => x.GetOrganizationReadAccessLevel(organizationId))
                .Returns(OrganizationDataReadAccessLevel.All);
            using var provider = new ServiceCollection()
                .AddSingleton(userContext.Object)
                .AddSingleton(authorization.Object)
                .BuildServiceProvider();
            var controller = new ItSystemUsageArchivesController(repository.Object, resolver.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = provider }
                }
            };

            var result = Assert.IsType<OkObjectResult>(controller.Get(organizationUuid));

            var archives = Assert.IsAssignableFrom<IEnumerable<ItSystemUsageArchive>>(result.Value);
            Assert.Same(expected, Assert.Single(archives));
        }
    }
}
