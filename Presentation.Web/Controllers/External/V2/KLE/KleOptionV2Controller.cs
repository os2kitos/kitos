using System.Web.Http;

namespace Presentation.Web.Controllers.External.V2.KLE
{
    /// <summary>
    /// Returns the available KLE options in KITOS
    /// </summary>
    [RoutePrefix("api/v2/kle-options")]
    public class KleOptionV2Controller
    {
        //TODO: Get many -> wraps in {version:DD, contents[]}
        //TODO: Single -> wrap in {version: DD, content:kle}
        //TODO: Filter: kleNumberPrefix
    }
}