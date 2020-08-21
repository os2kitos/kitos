using System;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject.Web.Common;

namespace Presentation.Web.Ninject
{
    public class NinjectControllerActivator : IControllerActivator
    {
        private readonly Bootstrapper _bootstrapper;

        public NinjectControllerActivator()
        {
            _bootstrapper = new Bootstrapper();
        }

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return _bootstrapper?.Kernel?.GetService(controllerType) as IController;
        }
    }
}