using System.Web.Mvc;
using Presentation.Web.Controllers.Web;
using Xunit;

namespace Tests.Unit.Presentation.Web
{
    public class WebHomeController
    {
        private readonly HomeController _homeController;

        public WebHomeController()
        {
            _homeController = new HomeController(null,null);
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
