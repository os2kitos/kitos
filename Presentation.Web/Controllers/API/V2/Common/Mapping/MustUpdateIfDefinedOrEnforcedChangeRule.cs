using System;
using System.Linq;
using System.Linq.Expressions;
using Core.Abstractions.Extensions;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class MustUpdateIfDefinedOrEnforced<TRoot> : IPropertyUpdateRule<TRoot>
    {
        private readonly Func<string[], bool> _checkPresenceInRequest;
        private readonly bool _enforceDefined;

        public MustUpdateIfDefinedOrEnforced(Func<string[], bool> checkPresenceInRequest, bool enforceDefined)
        {
            _checkPresenceInRequest = checkPresenceInRequest;
            _enforceDefined = enforceDefined;
        }

        private bool ClientRequestsChangeTo<TRoot, TProperty>(Expression<Func<TRoot, TProperty>> propertySelection)
        {
            var expression = propertySelection.Body;
            while (expression.NodeType == ExpressionType.Convert) //Called if implicit upcast is applied by the compiler
            {
                //Get the inner expression
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression.ToString() //the lambda body
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)  //We expect a property accessor devided by "."
                .Skip(1) // first segment is skipped (is the input parameter)
                .ToArray()
                .Transform(_checkPresenceInRequest);
        }

        public bool MustUpdate<TProperty>(Expression<Func<TRoot, TProperty>> pickProperty) => _enforceDefined || ClientRequestsChangeTo(pickProperty);
    }
}