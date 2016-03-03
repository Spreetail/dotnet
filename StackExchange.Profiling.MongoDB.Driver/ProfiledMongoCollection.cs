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
    public static class CustomExtensions
    {
        private static IDisposable StartStep<T>(IMongoCollection<T> col)
        {
            return MiniProfiler.Current.CustomTiming("MongoDB", col.Database.DatabaseNamespace + "." + col.CollectionNamespace);
        }

        public static Task<T> AsTimed<T,TDoc>(this Task<T> source, IMongoCollection<TDoc> col)
        {
            var t = StartStep(col);
            if (t == null)
                return source;
            return source.ContinueWith<T>(x => { t.Dispose(); return x.Result; });
        }
        public static Task<IAsyncCursor<TDoc>> AsTimed<T, TDoc>(this Task<IAsyncCursor<TDoc>> source, IMongoCollection<TDoc> col)
        {
            var t = MiniProfiler.Current.CustomTiming("mongodb", "");
            if (t == null)
                return source;
            return source.ContinueWith<IAsyncCursor<TDoc>>(x => { t.CommandString = x.Result.ToString(); t.Stop(); return x.Result; });
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
    /// For use later when there's a sync API.
    /// </summary>
    class DisposableTiming<TDoc> : IDisposable
    {
        private CustomTiming _timing;
        public DisposableTiming(IMongoCollection<TDoc> col)
        {
            this._timing = MiniProfiler.Current.CustomTiming("mongodb", col.CollectionNamespace.FullName);
        }
        public void Dispose()
        {
            if(_timing != null)
                _timing.Stop();
            _timing = null;
        }
    }

    //TODO -- spreetail is DeleteOneModel mongo 2.1, mongo 2.2 changes the interface. Package up as AbandonedMutexException nuget package.

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

        public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {

            return _collection.Aggregate<TResult>(pipeline, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.AggregateAsync<TResult>(pipeline, options, cancellationToken).ContinueWith(x => x.Result).AsTimed(this);
        }

        public BulkWriteResult<TDocument> BulkWrite(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.BulkWrite(requests, options, cancellationToken);
        }

        public Task<BulkWriteResult<TDocument>> BulkWriteAsync(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.BulkWriteAsync(requests, options, cancellationToken).AsTimed(this);
        }

        public long Count(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.Count(filter, options, cancellationToken);
        }

        public Task<long> CountAsync(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.CountAsync(filter, options, cancellationToken).AsTimed(this);
        }

        public DeleteResult DeleteMany(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteMany(filter, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteManyAsync(filter, cancellationToken).AsTimed(this);
        }

        public DeleteResult DeleteOne(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteOne(filter, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DeleteOneAsync(filter, cancellationToken).AsTimed(this);
        }

        public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.Distinct<TField>(field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.DistinctAsync<TField>(field, filter, options, cancellationToken).AsTimed(this);
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindAsync<TProjection>(filter, options, cancellationToken).AsTimed(this);
        }

        public TProjection FindOneAndDelete<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndDelete<TProjection>(filter, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndDeleteAsync<TProjection>(filter, options, cancellationToken).AsTimed(this);
        }

        public TProjection FindOneAndReplace<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndReplace<TProjection>(filter, replacement, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndReplaceAsync<TProjection>(filter, replacement, options, cancellationToken).AsTimed(this);
        }

        public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndUpdate<TProjection>(filter, update, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindOneAndUpdateAsync<TProjection>(filter, update, options, cancellationToken).AsTimed(this);
        }

        public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.FindSync<TProjection>(filter, options, cancellationToken);
        }

        public void InsertMany(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            _collection.InsertMany(documents, options, cancellationToken);
        }

        public Task InsertManyAsync(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertManyAsync(documents, options, cancellationToken).AsTimed(this);
        }

        public void InsertOne(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            _collection.InsertOne(document, options, cancellationToken);
        }

        public Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertOneAsync(document, cancellationToken).AsTimed(this);
        }

        public Task InsertOneAsync(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertOneAsync(document, options, cancellationToken);
        }

        public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.MapReduce<TResult>(map, reduce, options, cancellationToken);
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

        public ReplaceOneResult ReplaceOne(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.ReplaceOne(filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken).AsTimed(this);
        }

        public UpdateResult UpdateMany(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateMany(filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateManyAsync(filter, update, options, cancellationToken).AsTimed(this);
        }

        public UpdateResult UpdateOne(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateOne(filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.UpdateOneAsync(filter, update, options, cancellationToken).AsTimed(this);
        }

        public IMongoCollection<TDocument> WithReadConcern(ReadConcern readConcern)
        {
            return _collection.WithReadConcern(readConcern);
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
