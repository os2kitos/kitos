/* ProviderInitializationModule.cs
 * 
 * Since our custom membership and role providers are created by MVC when starting the application,
 * Ninject have no chance of handling injection into them.
 * Specifically, the user and role repositories used for the providers are never instantiated.
 * 
 * To combat this, we use this http module whose constructor are in need of the providers,
 * which are then injected ahead of time!
 * 
 * A binding to this module occurs in NinjectWebCommon.cs
 * 
 * See http://www.planetgeek.ch/2012/02/08/asp-net-provider-injection-with-ninject-3-0-0/ for more info
 */

using System.Web;
using System.Web.Security;

namespace UI.MVC4.Infrastructure
{
    public class ProviderInitializationHttpModule : IHttpModule
    {
        public ProviderInitializationHttpModule(MembershipProvider membershipProvider, RoleProvider roleProvider)
        {
        }

        public void Init(HttpApplication context)
        {
        }

        public void Dispose()
        {
        }
    }
}