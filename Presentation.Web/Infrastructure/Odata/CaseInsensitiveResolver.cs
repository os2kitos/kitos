using Microsoft.OData.UriParser;

namespace Presentation.Web.Infrastructure.Odata
{
    //For making urls case insensitive
    internal class CaseInsensitiveResolver : ODataUriResolver
    {
        public override bool EnableCaseInsensitive
        {
            get => true;
            set { /*ignore - always return true*/ }
        }
    }
}