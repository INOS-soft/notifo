﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;

namespace Notifo.SDK
{
    /// <summary>
    /// Default implementation of the <see cref="INotifoClient"/> interface.
    /// </summary>
    public sealed class NotifoClient : INotifoClient
    {
        private readonly Lazy<IAppsClient> apps;
        private readonly Lazy<IConfigsClient> configs;
        private readonly Lazy<IEventsClient> events;
        private readonly Lazy<ILogsClient> logs;
        private readonly Lazy<IMediaClient> media;
        private readonly Lazy<IMobilePushClient> mobilePush;
        private readonly Lazy<INotificationsClient> notifications;
        private readonly Lazy<ITemplatesClient> templates;
        private readonly Lazy<ITopicsClient> topics;
        private readonly Lazy<IUsersClient> users;

        /// <inheritdoc />
        public IAppsClient Apps => apps.Value;

        /// <inheritdoc />
        public IConfigsClient Configs => configs.Value;

        /// <inheritdoc />
        public IEventsClient Events => events.Value;

        /// <inheritdoc />
        public ILogsClient Logs => logs.Value;

        /// <inheritdoc />
        public IMediaClient Media => media.Value;

        /// <inheritdoc />
        public IMobilePushClient MobilePush => mobilePush.Value;

        /// <inheritdoc />
        public INotificationsClient Notifications => notifications.Value;

        /// <inheritdoc />
        public ITemplatesClient Templates => templates.Value;

        /// <inheritdoc />
        public ITopicsClient Topics => topics.Value;

        /// <inheritdoc />
        public IUsersClient Users => users.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifoClient"/> class with the HTTP client and the base URL.
        /// </summary>
        /// <param name="httpClient">The HTTP client. Cannot be null.</param>
        /// <param name="baseUrl">The base URL. Cannot be null.</param>
        public NotifoClient(HttpClient httpClient, string baseUrl)
        {
            apps = new Lazy<IAppsClient>(() =>
            {
                return new AppsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            configs = new Lazy<IConfigsClient>(() =>
            {
                return new ConfigsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            events = new Lazy<IEventsClient>(() =>
            {
                return new EventsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            logs = new Lazy<ILogsClient>(() =>
            {
                return new LogsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            media = new Lazy<IMediaClient>(() =>
            {
                return new MediaClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            mobilePush = new Lazy<IMobilePushClient>(() =>
            {
                return new MobilePushClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            notifications = new Lazy<INotificationsClient>(() =>
            {
                return new NotificationsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            templates = new Lazy<ITemplatesClient>(() =>
            {
                return new TemplatesClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            topics = new Lazy<ITopicsClient>(() =>
            {
                return new TopicsClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });

            users = new Lazy<IUsersClient>(() =>
            {
                return new UsersClient(httpClient)
                {
                    BaseUrl = baseUrl
                };
            });
        }
    }
}
