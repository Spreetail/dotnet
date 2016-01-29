using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Profiling.MongoDB.Driver
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// http://stackoverflow.com/a/30441479
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="childSelector"></param>
        /// <returns></returns>
        public static IQueryable<TSource> AsProfiledQueryable<TSource>(this IMongoCollection<TSource> source)
        {
            return new ProfiledMongoQueryable<TSource>(source.AsQueryable());
        }
    }

    // Define other methods and classes here
    public class ProfiledMongoQueryable<T> : IOrderedQueryable<T>
    {
        private readonly Expression _expression;
        private readonly QueryTranslatorProvider<T> _provider;

        public ProfiledMongoQueryable(IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            _expression = source.Expression;
            _provider = new QueryTranslatorProvider<T>(source);
        }
        public ProfiledMongoQueryable(IQueryable source, Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            _expression = expression;
            _provider = new QueryTranslatorProvider<T>(source);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_provider.ExecuteEnumerable(_expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _provider.ExecuteEnumerable(_expression).GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public IQueryProvider Provider
        {
            get { return _provider; }
        }
    }


    abstract class QueryTranslatorProvider : ExpressionVisitor
    {
        private readonly IQueryable _source;

        protected QueryTranslatorProvider(IQueryable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            _source = source;
        }

        internal IQueryable Source
        {
            get { return _source; }
        }
    }

    class QueryTranslatorProvider<T> : QueryTranslatorProvider, IQueryProvider
    {

        public QueryTranslatorProvider(IQueryable source)
            : base(source)
        {
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            return new ProfiledMongoQueryable<TElement>(Source, expression) as IQueryable<TElement>;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Type elementType = expression.Type.GetGenericArguments().First();
            IQueryable result = (IQueryable)Activator.CreateInstance(typeof(ProfiledMongoQueryable<>).MakeGenericType(elementType),
                    new object[] { Source, expression });
            return result;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            object result = (this as IQueryProvider).Execute(expression);
            return (TResult)result;
        }

        public object Execute(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Expression translated = VisitAll(expression);
            string cmdString = Source.Provider.CreateQuery(expression).ToString();
            var timing = StackExchange.Profiling.MiniProfiler.Current.CustomTiming("mongodb", cmdString);
            var results = Source.Provider.Execute(translated);
            timing.Stop();
            return results;
        }

        internal IEnumerable ExecuteEnumerable(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Expression translated = VisitAll(expression);
            string cmdString = Source.Provider.CreateQuery(expression).ToString();
            var timing = StackExchange.Profiling.MiniProfiler.Current.CustomTiming("mongodb", cmdString);
            var results = Source.Provider.CreateQuery(translated);
            timing.Stop();
            return results;
        }

        private Expression VisitAll(Expression expression)
        {
            // Run all visitors in order
            var visitors = new ExpressionVisitor[] { this };

            return visitors.Aggregate<ExpressionVisitor, Expression>(expression, (expr, visitor) => visitor.Visit(expr));
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Fix up the Expression tree to work with the underlying LINQ provider
            if (node.Type.IsGenericType &&
                node.Type.GetGenericTypeDefinition() == typeof(ProfiledMongoQueryable<>))
            {

                var provider = ((IQueryable)node.Value).Provider as QueryTranslatorProvider;

                if (provider != null)
                {
                    return provider.Source.Expression;
                }

                return Source.Expression;
            }

            return base.VisitConstant(node);
        }
    }

}
