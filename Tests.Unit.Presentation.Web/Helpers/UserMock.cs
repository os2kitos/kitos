using System.Security.Principal;
using Microsoft.AspNet.OData;

namespace Tests.Unit.Presentation.Web.Helpers
{
    /// <summary>
    /// Handle user identity of ODataController
    /// </summary>
    public class UserMock
    {
        private readonly ODataController _controller;
        private GenericPrincipal _user;

        public UserMock(ODataController controller, string name)
        {
            _controller = controller;
            Name = name;
        }

        public string Name { get; set; }

        /// <summary>
        /// Set valid user identity
        /// </summary>
        public void LogOn()
        {
            _user = new GenericPrincipal(
                new GenericIdentity(Name),
                new string[0]
            );

            _controller.User = _user;
        }

        /// <summary>
        /// Set invalid user identity
        /// </summary>
        public void LogOff()
        {
            _user = new GenericPrincipal(
                new GenericIdentity(string.Empty),
                new string[0]
            );

            _controller.User = _user;
        }
    }
}
