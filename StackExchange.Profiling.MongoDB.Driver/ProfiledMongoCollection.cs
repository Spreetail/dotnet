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
    public static class customExtensions
    {
        private static CustomTiming StartTiming()
        {
            return MiniProfiler.Current.CustomTiming("mongodb", "");
        }
        private static IDisposable StartStep<T>(IMongoCollection<T> col)
        {
            //MiniProfiler.StepStatic("asdf");
            return MiniProfiler.Current.Step("Mongo - " + col.CollectionNamespace);
        }
        public static Task<T> AsTimed<T,TDoc>(this Task<T> source, IMongoCollection<TDoc> col)
        {
            var t = StartStep(col);
            if (t == null)
                return source;
            return source.ContinueWith<T>(x => { t.Dispose(); return x.Result; });
        }
        public static Task AsTimed<TDoc>(this Task source, IMongoCollection<TDoc> col)
        {
            var t = StartStep(col);
            if (t == null)
                return source;
            return source.ContinueWith((x) => t.Dispose());
        }
    }

    /// <summary>
    /// Provides generic timings for the length of requests using the. Does NOT provide info about the query executed. These timings show up as Steps, not as 
    /// CustomTimings.
    /// TODO: figure out how inject $explain into queries so this works. OR, get a pull request in to mongodb.driver 
    /// to mark the OperationExecutor interface / class as public and provider a way to override / subclass the operationExecutor in the 
    /// MongoClient class.
    /// <typeparam name = "TDocument" ></ typeparam >
    public class ProfiledMongoCollection<TDocument> : IMongoCollection<TDocument>
    {
        private readonly IMongoCollection<TDocument> _collection;
        public ProfiledMongoCollection(IMongoCollection<TDocument> CollectionToProfile)
        {
            this._collection = CollectionToProfile;
        }

        public CollectionNamespace CollectionNamespace { get { return _collection.CollectionNamespace; } }

        public IMongoDatabase Database { get { return _collection.Database; } }

        public IBsonSerializer<TDocument> DocumentSerializer { get { return _collection.DocumentSerializer; } }

        public IMongoIndexManager<TDocument> Indexes { get { return _collection.Indexes; } }

        public MongoCollectionSettings Settings { get { return _collection.Settings; } }

        /// <summary>
        /// For use later when there's a sync API.
        /// </summary>
        //private class DisposableTiming : IDisposable
        //{
        //    private readonly CustomTiming _timing;

        //    public DisposableTiming()
        //    {
        //        this._timing = MiniProfiler.Current.CustomTiming("mongodb", "");
        //    }
        //    public void Dispose()
        //    {
        //        _timing.Stop();
        //    }
        //}

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.AggregateAsync<TResult>(pipeline, options, cancellationToken).ContinueWith(x => x.Result).AsTimed(this);
        }

        public Task<BulkWriteResult<TDocument>> BulkWriteAsync(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.BulkWriteAsync(requests, options, cancellationToken).AsTimed(this);
        }

        public Task<long> CountAsync(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.CountAsync(filter, options, cancellationToken).AsTimed(this);
        }

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteManyAsync(filter, cancellationToken).AsTimed(this);
        }

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteOneAsync(filter, cancellationToken).AsTimed(this);
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DistinctAsync<TField>(field, filter, options, cancellationToken).AsTimed(this);
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindAsync<TProjection>(filter, options, cancellationToken).AsTimed(this);
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndDeleteAsync<TProjection>(filter, options, cancellationToken).AsTimed(this);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndReplaceAsync<TProjection>(filter, replacement, options, cancellationToken).AsTimed(this);
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndUpdateAsync<TProjection>(filter, update, options, cancellationToken).AsTimed(this);
        }

        public Task InsertManyAsync(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertManyAsync(documents, options, cancellationToken).AsTimed(this);
        }

        public Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertOneAsync(document, cancellationToken).AsTimed(this);
        }

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.MapReduceAsync<TResult>(map, reduce, options, cancellationToken).AsTimed(this);
        }

        /// <summary>
        /// not timed.
        /// </summary>
        /// <typeparam name="TDerivedDocument"></typeparam>
        /// <returns></returns>
        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : TDocument
        {
            return _collection.OfType<TDerivedDocument>();
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken).AsTimed(this);
        }

        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateManyAsync(filter, update, options, cancellationToken).AsTimed(this);
        }

        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateOneAsync(filter, update, options, cancellationToken).AsTimed(this);
        }

        /// <summary>
        /// not timed.
        /// </summary>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public IMongoCollection<TDocument> WithReadPreference(ReadPreference readPreference)
        {
            return new ProfiledMongoCollection<TDocument>(_collection.WithReadPreference(readPreference));
        }

        /// <summary>
        /// not timed.
        /// </summary>
        /// <param name="writeConcern"></param>
        /// <returns></returns>
        public IMongoCollection<TDocument> WithWriteConcern(WriteConcern writeConcern)
        {
            return new ProfiledMongoCollection<TDocument>(_collection.WithWriteConcern(writeConcern));
        }
    }
}
