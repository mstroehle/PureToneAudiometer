namespace PureToneAudiometer
{
    using System.Linq.Expressions;
    using System;

    public static class Property
    {
        public static string Name<T>(Expression<Func<T>> action)
        {
            var expression = action.Body as MemberExpression;
            if (expression == null)
            {
                throw new ArgumentException("Wrong lambda format for the property name extraction");
            }
            return expression.Member.Name;
        }
    }
}
