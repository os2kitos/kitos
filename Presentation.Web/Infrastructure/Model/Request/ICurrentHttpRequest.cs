using System.Collections.Generic;
using Core.Abstractions.Types;
using Newtonsoft.Json.Linq;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public interface ICurrentHttpRequest
    {
        /// <summary>
        /// Based on the current request, returns a set of all defined json properties.
        /// If the input body is not json the response will contain an empty set.
        /// </summary>
        /// <param name="pathTokens">
        /// Path to use as reference for the property output.
        /// Example:
        ///     JSON:
        ///     {
        ///         "general" :
        ///             {
        ///                 "a" :
        ///                 {
        ///                     "b" : 1,
        ///                     "c" : 2
        ///                 }
        ///             }
        ///     }
        ///     The following parameters: ["general", "a"]  will result in ["b","c"] 
        /// </param>
        /// <returns></returns>
        ISet<string> GetDefinedJsonProperties(params string[] pathTokens);
        Maybe<JToken> GetObject(params string[] pathTokens);
    }
}
