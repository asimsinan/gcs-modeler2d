using FuzzyMsc.Pattern.Infrastructure;
using FuzzyMsc.Pattern.Repositories;
using FuzzyMsc.ServicePattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FuzzyMsc.ServicePattern
{
    public abstract class Service<TEntity> : IService<TEntity> where TEntity : class, IObjectState
    {

        #region Private Fields
        private IRepositoryAsync<TEntity> _repository;
        #endregion Private Fields
        public IRepositoryAsync<TEntity> Repository
        {
            get { return _repository; }
            set { _repository = value; }
        }


        #region Constructor
        protected Service(IRepositoryAsync<TEntity> repository) { _repository = repository; }

        public Service()
        {
        }
        #endregion Constructor

        public virtual TEntity Find(params object[] keyValues) { return _repository.Find(keyValues); }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters) { return _repository.SelectQuery(query, parameters).AsQueryable(); }

        public virtual void Insert(TEntity entity) { _repository.Insert(entity); }

        public virtual void InsertRange(IEnumerable<TEntity> entities) { _repository.InsertRange(entities); }

        public virtual void InsertOrUpdateGraph(TEntity entity) { _repository.InsertOrUpdateGraph(entity); }

        public virtual void InsertGraphRange(IEnumerable<TEntity> entities) { _repository.InsertGraphRange(entities); }

        public virtual void Update(TEntity entity) { _repository.Update(entity); }

        public virtual void Delete(object id) { _repository.Delete(id); }

        public virtual void Delete(TEntity entity) { _repository.Delete(entity); }

        public virtual void BulkInsertRange(IEnumerable<TEntity> entities) { _repository.BulkInsertRange(entities); }
        public virtual void BulkDeleteRange(IEnumerable<TEntity> entities) { _repository.BulkDeleteRange(entities); }
        public virtual void BulkUpdateRange(IEnumerable<TEntity> entities) { _repository.BulkUpdateRange(entities); }
        public virtual void BulkInsert(TEntity entity) { _repository.BulkInsert(entity); }
        public virtual void BulkUpdate(TEntity entity) { _repository.BulkUpdate(entity); }
        public virtual void BulkDelete(TEntity entity) { _repository.BulkDelete(entity); }

        public IQueryFluent<TEntity> Query() { return _repository.Query(); }

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject) { return _repository.Query(queryObject); }

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query) { return _repository.Query(query); }

        public virtual async Task<TEntity> FindAsync(params object[] keyValues) { return await _repository.FindAsync(keyValues); }

        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues) { return await _repository.FindAsync(cancellationToken, keyValues); }

        public virtual async Task<bool> DeleteAsync(params object[] keyValues) { return await DeleteAsync(CancellationToken.None, keyValues); }

        //IF 04/08/2014 - Before: return await DeleteAsync(cancellationToken, keyValues);
        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues) { return await _repository.DeleteAsync(cancellationToken, keyValues); }

        public IQueryable<TEntity> Queryable() { return _repository.Queryable(); }
    }
}
