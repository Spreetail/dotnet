using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using StackExchange.Profiling.MongoDB.Driver;

namespace StackExchange.Profiling.MongoDB.Driver
{
    public static class customExtensions {
        private static CustomTiming StartTiming()
        {
            return MiniProfiler.Current.CustomTiming("mongo", "");
        }
        public static Task<T> AsTimed<T>(this Task<T> source)
        {
            var t = StartTiming();
            return source.ContinueWith<T>(x => { t.Stop(); return x.Result; });
        }
        public static Task AsTimed(this Task source)
        {
            var t = StartTiming();
            return source.ContinueWith((x) => t.Stop());
        }
    }
    /// <summary>
    /// TODO: Create a ContinueWithMiniProfiler extension method that starts a profiler and stops when the task completes.Can be done later.Need to do more research if possible.
    /// </summary>
    /// <typeparam name = "TDocument" ></ typeparam >
    public class ProfiledMongoCollection<TDocument> : IMongoCollection<TDocument>
    {
        private readonly IMongoCollection<TDocument> _collection;
        public ProfiledMongoCollection(IMongoCollection<TDocument> CollectionToProfile)
        {
            this._collection = CollectionToProfile;
        }

        public CollectionNamespace CollectionNamespace
        {
            get
            {
                return _collection.CollectionNamespace;
            }
        }

        public IMongoDatabase Database
        {
            get
            {
                return _collection.Database;
            }
        }

        public IBsonSerializer<TDocument> DocumentSerializer
        {
            get
            {
                return _collection.DocumentSerializer;
            }
        }

        public IMongoIndexManager<TDocument> Indexes
        {
            get
            {
                return _collection.Indexes;
            }
        }

        public MongoCollectionSettings Settings
        {
            get
            {
                return _collection.Settings;
            }
        }

        /// <summary>
        /// For use later when there's a sync API. BOOYAH.
        /// </summary>
        private class DisposableTiming : IDisposable
        {
            private readonly CustomTiming _timing;

            public DisposableTiming()
            {
                this._timing = MiniProfiler.Current.CustomTiming("mongo", "");
            }
            public void Dispose()
            {
                _timing.Stop();
            }
        }

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.AggregateAsync<TResult>(pipeline, options, cancellationToken).ContinueWith(x=>x.Result).AsTimed();
        }

        public Task<BulkWriteResult<TDocument>> BulkWriteAsync(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.BulkWriteAsync(requests, options, cancellationToken).AsTimed();
        }

        public Task<long> CountAsync(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.CountAsync(filter, options, cancellationToken).AsTimed();
        }

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteManyAsync(filter, cancellationToken).AsTimed();
        }

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteOneAsync(filter, cancellationToken).AsTimed();
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DistinctAsync<TField>(field, filter, options, cancellationToken).AsTimed();
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindAsync<TProjection>(filter, options, cancellationToken).AsTimed();
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndDeleteAsync<TProjection>(filter, options, cancellationToken).AsTimed();
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndReplaceAsync<TProjection>(filter, replacement, options, cancellationToken).AsTimed();
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndUpdateAsync<TProjection>(filter, update, options, cancellationToken).AsTimed();
        }

        public Task InsertManyAsync(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertManyAsync(documents, options, cancellationToken).AsTimed();
        }

        public Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertOneAsync(document, cancellationToken).AsTimed();
        }

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.MapReduceAsync<TResult>(map, reduce, options, cancellationToken).AsTimed();
        }

        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : TDocument
        {
            return _collection.OfType<TDerivedDocument>();
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken).AsTimed();
        }

        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateManyAsync(filter, update, options, cancellationToken).AsTimed();
        }

        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateOneAsync(filter, update, options, cancellationToken).AsTimed();
        }

        public IMongoCollection<TDocument> WithReadPreference(ReadPreference readPreference)
        {
            return new ProfiledMongoCollection<TDocument>(_collection.WithReadPreference(readPreference));
        }

        public IMongoCollection<TDocument> WithWriteConcern(WriteConcern writeConcern)
        {
            return new ProfiledMongoCollection<TDocument>(_collection.WithWriteConcern(writeConcern));
        }
    }
}
