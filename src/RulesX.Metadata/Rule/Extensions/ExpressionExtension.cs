using System;
using System.Linq.Expressions;

namespace RulesX.Metadata.Rule.Extensions
{
    public static class ExpressionExtension
    {
        public static Expression Operate(this Expression ex1, Expression ex2, OperationCode op)
        {
            switch (op)
            {
                case OperationCode.NotEquals:
                    return Expression.NotEqual(ex1, ex2);
                case OperationCode.Equals:
                    return Expression.Equal(ex1, ex2);
                case OperationCode.GreaterThan:
                    return Expression.GreaterThan(ex1, ex2);
                case OperationCode.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(ex1, ex2);
                case OperationCode.LessThan:
                    return Expression.LessThan(ex1, ex2);
                case OperationCode.LessThanOrEqual:
                    return Expression.LessThanOrEqual(ex1, ex2);
            }
            return Expression.Equal(ex1, ex2);
        }

        public static Expression And<T>(this Expression expr1, Expression expr2, out ParameterExpression paramExpr)
        {
            paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.And(expr1, expr2);
            exprBody = (BinaryExpression)new ParameterReplacer(paramExpr).Visit(exprBody);
            var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody ?? throw new InvalidOperationException(), paramExpr).Body;
            return finalExpr;
        }

        public static Expression Or<T>(this Expression expr1, Expression expr2, out ParameterExpression paramExpr)
        {
            paramExpr = Expression.Parameter(typeof(T));
            var exprBody = Expression.Or(expr1, expr2);
            exprBody = (BinaryExpression)new ParameterReplacer(paramExpr).Visit(exprBody);
            var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody ?? throw new InvalidOperationException(), paramExpr).Body;
            return finalExpr;
        }
    }
}
