using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Core.DomainModel;
using Core.DomainServices;
using Moq;
using Presentation.Web.Controllers.API;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Unit.Presentation.Web.Helpers
{
    public abstract class KitosRestControllerApiTestWithAutofixture : WithAutoFixture
    {
        protected User KitosUser { get; private set; }

        protected virtual void SetupControllerFrorTest(BaseApiController sut)
        {
            var userRepository = new Mock<IGenericRepository<User>>();
            sut.UserRepository = userRepository.Object;

            //Set request context
            var httpRequestMessage = new HttpRequestMessage();
            var httpRequestContext = new HttpRequestContext
            {
                Configuration = new HttpConfiguration()
            };
            httpRequestMessage.Properties.Add(HttpPropertyKeys.RequestContextKey, httpRequestContext);
            sut.RequestContext = httpRequestContext;
            sut.Request = httpRequestMessage;

            //Setup authenticated user
            var identity = new Mock<IIdentity>();
            var userId = A<int>();
            identity.Setup(x => x.Name).Returns(userId.ToString());
            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.Identity).Returns(identity.Object);
            sut.User = principal.Object;
            KitosUser = new User
            {
                DefaultOrganizationId = A<int>()
            };
            userRepository.Setup(x => x.GetByKey(userId)).Returns(KitosUser);
        }

        protected T ExpectResponseOf<T>(HttpResponseMessage message)
        {
            var content = Assert.IsType<ObjectContent<ApiReturnDTO<T>>>(message.Content);
            var dto = Assert.IsType<ApiReturnDTO<T>>(content.Value);
            Assert.NotNull(dto.Response);
            return dto.Response;
        }
    }
}
