using System.Web.Mvc;
using Presentation.Web.Controllers.Web.Old;
using Xunit;

namespace Tests.Unit.Presentation.Web
{
    public class WebHomeController
    {
        private readonly OldHomeController _homeController;

        public WebHomeController()
        {
            _homeController = new OldHomeController(null,null);
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
