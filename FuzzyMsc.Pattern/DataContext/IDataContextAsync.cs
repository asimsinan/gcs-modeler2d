using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuzzyMsc.Pattern.DataContext
{
    public interface IDataContextAsync : IDataContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
        void BulkInsertRange<TEntity>(IEnumerable<TEntity> entity) where TEntity : class;
        void BulkDeleteRange<TEntity>(IEnumerable<TEntity> entity) where TEntity : class;
        void BulkUpdateRange<TEntity>(IEnumerable<TEntity> entity) where TEntity : class;
    }
}
