using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FuzzyMsc.ServicePattern
{
    public class TextAra<T>
    {
        // private IQueryable<T> sorgu;

        public IQueryable<T> TextrAra(IQueryable<T> queryable, string searchKey, bool exactMatch)
        {

            ParameterExpression parameter = Expression.Parameter(typeof(T), "c");

            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            MethodInfo toStringMethod = typeof(object).GetMethod("ToString", new Type[] { });

            var publicProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(p => p.PropertyType == typeof(string));
            Expression orExpressions = null;

            string[] searchKeyParts;
            if (exactMatch)
            {
                searchKeyParts = new[] { searchKey };
            }
            else
            {
                searchKeyParts = searchKey.Split(' ');
            }

            foreach (var property in publicProperties)
            {
                Expression nameProperty = Expression.Property(parameter, property);
                foreach (var searchKeyPart in searchKeyParts)
                {
                    Expression searchKeyExpression = Expression.Constant(searchKeyPart);
                    Expression callContainsMethod = Expression.Call(nameProperty, containsMethod, searchKeyExpression);
                    if (orExpressions == null)
                    {
                        orExpressions = callContainsMethod;
                    }
                    else
                    {
                        orExpressions = Expression.Or(orExpressions, callContainsMethod);
                    }
                }
            }

            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryable.ElementType },
                queryable.Expression,
                Expression.Lambda<Func<T, bool>>(orExpressions, new ParameterExpression[] { parameter }));
            // this.sorgu = queryable.Provider.CreateQuery<T>(whereCallExpression);
            return queryable.Provider.CreateQuery<T>(whereCallExpression);
        }

        //public  IQueryable<T> TexttAra(IQueryable<T> sorgu)
        //{
        //    return null;
        //}
    }
}
