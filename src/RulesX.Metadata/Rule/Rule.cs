using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RulesX.Metadata.Rule.Extensions;

namespace RulesX.Metadata.Rule
{
    public class Rule<T>
    {
        internal Rule(OperationCode op, Tuple<string, Type> keyValue, string property, ClauseCode clause)
        {
            Operator = op;
            TypeValuePair = (keyValue.Item1, keyValue.Item2);
            Property = property;
            Clause = clause;
            ParamExp = Expression.Parameter(typeof(T), "x");
            var typeValue = Convert.ChangeType(TypeValuePair.Item1, TypeValuePair.Item2);
            RightExp = Expression.Constant(typeValue);
            LeftExp = Left(ParamExp, property);
            CombinedExp = Combined(LeftExp, RightExp);
        }

        public Rule(OperationCode op, string value, string property)
        {
            Operator = op;
            Value = value;
            Property = property;
            var propInfo = typeof(T).GetProperty(property);
            Clause = ClauseCode.Where;
            ParamExp = Expression.Parameter(typeof(T), "x");
            var typeValue = ChangeType(value, propInfo);
            LeftExp = Left(ParamExp, property);
            RightExp = Expression.Constant(typeValue, propInfo.PropertyType);
            CombinedExp = Combined(LeftExp, RightExp);
        }

        private Rule(OperationCode op, Expression left, Expression right, ParameterExpression pe, ClauseCode cl)
        {
            LeftExp = left;
            RightExp = right;
            CombinedExp = left.Operate(right, op);
            ParamExp = pe;
            Clause = cl;
        }

        public bool IsNullable
        {
            get
            {
                if (Property == null)
                    return false;
                var t = typeof(T).GetProperty(Property).PropertyType;
                if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return true;
                }
                return false;
            }
        }

        public string Value { get; set; }
        public ParameterExpression ParamExp { get; set; }
        public OperationCode Operator { get; }
        public (string, Type) TypeValuePair { get; }
        public string Property { get; }
        public ClauseCode Clause { get; }
        public Expression CombinedExp { get; set; }
        public Expression LeftExp { get; }
        public Expression RightExp { get; }

        public Rule<T> NullRule =>
            IsNestedProperty || IsNullable ?
                new Rule<T>(OperationCode.NotEquals, Expression.Property(ParamExp, Property.Split('.')[0]), Expression.Constant(null), ParamExp, Clause)
                : null;

        public MemberExpression Left(Expression ex, string propertyName)
        {
            string[] parts = propertyName.Split('.');
            int partsL = parts.Length;

            return partsL > 1
                ?
                Expression.Property(
                    Left(
                        ex,
                        parts.Take(partsL - 1)
                            .Aggregate((a, i) => a + "." + i)
                    ),
                    parts[partsL - 1])
                :
                Expression.Property(ex, propertyName);

        }
        public Expression Combined(Expression exp1, Expression exp2)
        {
            return LeftExp.Operate(RightExp, Operator);
        }

        public bool IsNestedProperty
        {
            get
            {
                if (Property != null)
                {
                    var array = Property?.Split('.');
                    return array.Length > 1;
                }
                return false;
            }
        }

        public static object ChangeType(string value, PropertyInfo conversion)
        {
            var t = conversion.PropertyType;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {

                Type tx = Nullable.GetUnderlyingType(conversion.PropertyType) ?? conversion.PropertyType;
                object safeValue = value == null ? null : Convert.ChangeType(value, tx);
                return safeValue;
            }
            return Convert.ChangeType(value, t);
        }
    }
}
