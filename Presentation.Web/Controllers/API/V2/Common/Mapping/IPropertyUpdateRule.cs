using System;
using System.Linq.Expressions;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IPropertyUpdateRule<TRoot>
    {
        /// <summary>
        /// Determines if the property selected by <param name="pickProperty"> must be mapped as a property update</param>
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="pickProperty"></param>
        /// <returns></returns>
        public bool MustUpdate<TProperty>(Expression<Func<TRoot, TProperty>> pickProperty);

    }
}
