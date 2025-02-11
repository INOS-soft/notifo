﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;
using Squidex.Hosting;

namespace Notifo.Domain.UserNotifications.MongoDb
{
    public sealed class MongoDbUserNotificationRepository : MongoDbRepository<UserNotification>, IUserNotificationRepository, IInitializable
    {
        static MongoDbUserNotificationRepository()
        {
            BsonClassMap.RegisterClassMap<UserNotification>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.MapProperty(x => x.IsSeen)
                    .SetIgnoreIfNull(true);

                cm.MapProperty(x => x.IsConfirmed)
                    .SetIgnoreIfNull(true);
            });

            BsonClassMap.RegisterClassMap<BaseUserNotification>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.MapIdProperty(x => x.Id)
                    .SetSerializer(new GuidSerializer(BsonType.String));
            });

            BsonClassMap.RegisterClassMap<UserNotificationChannel>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);

                cm.MapProperty(x => x.Status)
                    .SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, ChannelSendInfo>, string, ChannelSendInfo>()
                        .WithKeySerializer(new Base64Serializer()));
            });
        }

        public MongoDbUserNotificationRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Notifications";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<UserNotification> collection, CancellationToken ct)
        {
            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserNotification>(
                    IndexKeys
                        .Ascending(x => x.UserId)
                        .Ascending(x => x.EventId)),
                null, ct);

            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<UserNotification>(
                    IndexKeys
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.UserId)
                        .Ascending(x => x.Updated)
                        .Descending(x => x.Created)),
                null, ct);
        }

        public async Task<bool> IsConfirmedOrHandled(Guid id, string channel, string configuration)
        {
            var filter =
               Filter.And(
                   Filter.Eq(x => x.Id, id),
                   Filter.Exists(x => x.IsConfirmed, false),
                   Filter.Ne($"Channels.{channel}.Status.{configuration}.Status", ProcessStatus.Handled));

            var count =
                await Collection.Find(filter).Limit(1)
                    .CountDocumentsAsync();

            return count == 1;
        }

        public async Task<IResultList<UserNotification>> QueryAsync(string appId, string userId, UserNotificationQuery query, CancellationToken ct)
        {
            var filters = new List<FilterDefinition<UserNotification>>
            {
                Filter.Eq(x => x.AppId, appId),
                Filter.Eq(x => x.UserId, userId)
            };

            if (query.After != default)
            {
                filters.Add(Filter.Gte(x => x.Updated, query.After));
            }

            switch (query.Scope)
            {
                case UserNotificationQueryScope.Deleted:
                    {
                        filters.Add(Filter.Eq(x => x.IsDeleted, true));
                        break;
                    }

                case UserNotificationQueryScope.NonDeleted:
                    {
                        filters.Add(
                            Filter.Or(
                                Filter.Exists(x => x.IsDeleted, false),
                                Filter.Eq(x => x.IsDeleted, false)));
                        break;
                    }
            }

            var filter = Filter.And(filters);

            var resultItems = await Collection.Find(filter).SortByDescending(x => x.Created).ToListAsync(query, ct);
            var resultTotal = (long)resultItems.Count;

            if (query.ShouldQueryTotal(resultItems))
            {
                resultTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(resultTotal, resultItems);
        }

        public async Task<UserNotification?> FindAsync(Guid id)
        {
            var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();

            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.IsDeleted, true), cancellationToken: ct);
        }

        public async Task TrackSeenAsync(IEnumerable<Guid> ids, HandledInfo handle)
        {
            var writes = new List<WriteModel<UserNotification>>();

            foreach (var id in ids)
            {
                writes.Add(new UpdateOneModel<UserNotification>(
                    Filter.And(
                        Filter.Eq(x => x.Id, id),
                        Filter.Exists(x => x.IsSeen, false)),
                    Update
                        .Set(x => x.IsSeen, handle)
                        .Set(x => x.Updated, handle.Timestamp)));

                writes.Add(new UpdateOneModel<UserNotification>(
                    Filter.And(
                        Filter.Eq(x => x.Id, id),
                        Filter.Eq(x => x.Formatting.ConfirmMode, ConfirmMode.Seen),
                        Filter.Exists(x => x.IsConfirmed, false)),
                    Update
                        .Set(x => x.IsConfirmed, handle)
                        .Set(x => x.Updated, handle.Timestamp)));
            }

            if (writes.Count == 0)
            {
                return;
            }

            await Collection.BulkWriteAsync(writes);
        }

        public async Task<UserNotification?> TrackConfirmedAsync(Guid id, HandledInfo handle)
        {
            var entity =
                await Collection.FindOneAndUpdateAsync(
                    Filter.And(
                        Filter.Eq(x => x.Id, id),
                        Filter.Eq(x => x.Formatting.ConfirmMode, ConfirmMode.Explicit),
                        Filter.Exists(x => x.IsConfirmed, false)),
                    Update
                        .Set(x => x.IsConfirmed, handle)
                        .Set(x => x.Updated, handle.Timestamp));

            if (entity != null)
            {
                entity.IsConfirmed = handle;

                entity.Updated = handle.Timestamp;
            }

            return entity;
        }

        public async Task BatchWriteAsync(IEnumerable<(Guid Id, string Channel, string Configuraton, ChannelSendInfo Info)> updates, CancellationToken ct)
        {
            var writes = new List<WriteModel<UserNotification>>();

            var documentUpdates = new List<UpdateDefinition<UserNotification>>();

            foreach (var group in updates.GroupBy(x => x.Id))
            {
                documentUpdates.Clear();

                foreach (var (_, channel, configuration, info) in group)
                {
                    var path = $"Channels.{channel}.Status.{configuration.ToBase64()}";

                    documentUpdates.Add(Update.Set($"{path}.Detail", info.Detail));
                    documentUpdates.Add(Update.Set($"{path}.Status", info.Status));
                    documentUpdates.Add(Update.Set($"{path}.LastUpdate", info.LastUpdate));
                }

                var update = Update.Combine(documentUpdates);

                writes.Add(new UpdateOneModel<UserNotification>(Filter.Eq(x => x.Id, group.Key), update));
            }

            if (writes.Count == 0)
            {
                return;
            }

            await Collection.BulkWriteAsync(writes, cancellationToken: ct);
        }

        public async Task InsertAsync(UserNotification notification, CancellationToken ct)
        {
            try
            {
                await Collection.InsertOneAsync(notification, null, ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new UniqueConstraintException();
            }
        }
    }
}
