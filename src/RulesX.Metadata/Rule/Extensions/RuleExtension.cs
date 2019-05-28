using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RulesX.Metadata.Rule.Extensions
{
    public static class RuleExtension
    {
        private static (MethodCallExpression, IQueryable<T>) ToMethod<T>(this Rule<T> rule, IQueryable<T> s)
        {
            var x = s;
            if (rule.IsNestedProperty || rule.IsNullable)
                x = rule.NullRule.ToMethod(s).Evaluate().AsQueryable();
            return (Expression.Call(typeof(Queryable),
                rule.Clause.ToString(),
                new[] { x.ElementType },
                x.Expression,
                Expression.Lambda<Func<T, bool>>(rule.CombinedExp, new ParameterExpression[] { rule.ParamExp })), x);
        }

        public static bool Evaluate<T>(this Rule<T> rule, T s)
        {
            var queryableS= new List<T>{s};
            var result=ToMethod(rule, queryableS.AsQueryable()).Evaluate();
            return result.Any();
        }

        public static IEnumerable<T> Evaluate<T>(this Rule<T> rule, IEnumerable<T> s)
        {
            var queryable = s.AsQueryable();
            return rule.ToMethod(queryable).Evaluate();
        }

        private static IEnumerable<T> Evaluate<T>(this (MethodCallExpression ex, IQueryable<T> s) methodCallWithCollection)
        {
            var list = new List<T>();
            var exp = methodCallWithCollection.ToTuple().Item1;
            var s = methodCallWithCollection.ToTuple().Item2.AsQueryable();
            if (exp.Method.Name.Contains(ClauseCode.FirstOrDefault.ToString())
                || exp.Method.Name.Contains(ClauseCode.Any.ToString()))
                list.Add(s.Provider.Execute<T>(exp));
            else
                list = s.Provider.CreateQuery<T>(exp).ToList();
            return list;
        }

        public static Rule<T> And<T>(this Rule<T> rule1, Rule<T> rule2)
        {
            var rule = new Rule<T>(rule1.Operator, rule1.Value, rule1.Property);//new Rule<T>(rule1.Operator, rule1.TypeValuePair.ToTuple(), rule1.Property, rule1.Clause, rule1.Order, rule1.AggregateClauseCode);
            var combinedRule = rule1.CombinedExp.And<T>(rule2.CombinedExp, out ParameterExpression ex);
            rule.CombinedExp = combinedRule;
            rule.ParamExp = ex;
            return rule;
        }

        public static Rule<T> Or<T>(this Rule<T> rule1, Rule<T> rule2)
        {
            var rule = new Rule<T>(rule1.Operator, rule1.Value, rule1.Property);
            var combinedRule = rule1.CombinedExp.Or<T>(rule2.CombinedExp, out ParameterExpression ex);
            rule.CombinedExp = combinedRule;
            rule.ParamExp = ex;
            return rule;
        }

        public static IEnumerable Select(this IQueryable source, string member)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (member == null) throw new ArgumentNullException(nameof(member));
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");
            Type generic = typeof(IEnumerable<>);
            PropertyInfo property = source.ElementType.GetProperty(member);
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);
            var propertyType = property.PropertyType;
            var typeArgs = new[] { propertyType };
            generic.MakeGenericType(typeArgs);
            var selector = Expression.Lambda(getter, parameter);
            MethodInfo sumMethod = typeof(Enumerable).GetMethods().First(
                m => m.Name == "Select"
                     //&& m.ReturnType == property.PropertyType
                     && m.IsGenericMethod);

            var genericSumMethod = sumMethod.MakeGenericMethod(new[] { source.ElementType, propertyType });

            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] { source.Expression, selector });
            return source.Provider.Execute(callExpression) as IEnumerable;
        }
    }
}
