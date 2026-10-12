using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model;
using Core.DomainModel;
using Moq;
using System;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class CrudAuthorizationModelTest: WithAutoFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AuthorizeUpdate_ReturnsResultOf_AuthorizationContext_CanModify(bool expected)
        {
            var entity = new Mock<IEntityOwnedByOrganization>();
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();
            var authorizationContext = new Mock<IAuthorizationContext>();
            authorizationContext.Setup(_ => _.AllowModify(It.IsAny<IEntity>())).Returns(expected);
            var sut = new CrudAuthorizationModel(authorizationContext.Object);

            var result = sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.Equal(expected, result.Ok && result.Value);
            authorizationContext.Verify(_ => _.AllowModify(entity.Object));

        }
    }
}
