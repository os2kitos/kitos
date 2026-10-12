using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using Presentation.Web.Controllers.Web.Old;
using Xunit;

namespace Tests.Unit.Presentation.Web
{
    public class WebHomeController
    {
        private readonly OldHomeController _homeController;

        public WebHomeController()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["AppSettings:Environment"]).Returns("dev");
            _homeController = new OldHomeController(null!, null!, configMock.Object);
            _homeController.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public void Index_Call_ReturnsView()
        {
            // Arrange

            // Act
            var ret = _homeController.Index();

            // Assert
            Assert.IsType<ViewResult>(ret);
        }
    }
}
