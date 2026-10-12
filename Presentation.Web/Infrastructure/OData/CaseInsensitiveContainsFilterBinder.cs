using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.UriParser;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Presentation.Web.Infrastructure.OData
{
    public class CaseInsensitiveContainsFilterBinder: FilterBinder
    {
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;
        private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        private const string ContainsFunctionName = "contains";
        private const string StartsWithFunctionName = "startswith";
        private const string EndsWithFunctionName = "endswith";

        public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
        {
            return node.Name switch
            {
                ContainsFunctionName => CallCaseInsensitiveStringComparisonExpression(node, context, ContainsMethod),
                StartsWithFunctionName => CallCaseInsensitiveStringComparisonExpression(node, context, StartsWithMethod),
                EndsWithFunctionName => CallCaseInsensitiveStringComparisonExpression(node, context, EndsWithMethod),
                _ => base.BindSingleValueFunctionCallNode(node, context)
            };
        }

        private Expression CallCaseInsensitiveStringComparisonExpression(SingleValueFunctionCallNode node, QueryBinderContext context, MethodInfo stringMethod)
        {
            var parameters = node.Parameters.ToList();
            var left = Bind(parameters[0], context);
            var right = Bind(parameters[1], context);
            var loweredLeft = Expression.Call(left, ToLowerMethod);
            var loweredRight = Expression.Call(right, ToLowerMethod);

            Expression comparisonCall = Expression.Call(loweredLeft, stringMethod, loweredRight);

            // Guard against NullReferenceException when the property is null during in-memory filter
            // evaluation (e.g. after ToList()). OData semantics: contains(null, 'x') = false.
            if (!left.Type.IsValueType)
            {
                comparisonCall = Expression.Condition(
                    Expression.Equal(left, Expression.Default(left.Type)),
                    Expression.Constant(false),
                    comparisonCall);
            }

            return comparisonCall;
        }

    }
}
