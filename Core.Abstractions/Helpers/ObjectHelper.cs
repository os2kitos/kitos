using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Abstractions.Helpers
{
    public class ObjectHelper
    {
        public static string GetPropertyPath<T>(Expression<Func<T, object?>> expression)
        {
            var path = new List<string>();
            var expr = expression.Body;

            while (expr is MemberExpression memberExpr)
            {
                path.Insert(0, memberExpr.Member.Name);
                expr = memberExpr.Expression;
            }

            if (expr is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression memberExpr2)
            {
                path.Insert(0, memberExpr2.Member.Name);
            }

            var className = typeof(T).Name;
            path.Insert(0, className);

            return string.Join(".", path);
        }
    }
}
