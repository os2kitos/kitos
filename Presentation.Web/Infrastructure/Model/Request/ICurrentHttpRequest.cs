using System.Collections.Generic;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public interface ICurrentHttpRequest
    {
        /// <summary>
        /// Based on the current request, returns a set of all defined root json properties.
        /// If the input body is not json the response will contain an empty set.
        /// </summary>
        /// <returns></returns>
        ISet<string> GetDefinedJsonRootProperties();
    }
}
