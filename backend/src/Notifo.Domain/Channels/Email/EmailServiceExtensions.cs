﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.AspNetCore;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EmailServiceExtensions
    {
        public static void AddMyEmailChannel(this IServiceCollection services)
        {
            services.AddHttpClient("notifo-mjml");
            services.AddMjmlServices();

            services.AddSingletonAs<EmailChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<EmailJob>>();

            services.AddSingletonAs<EmailFormatter>()
                .As<IEmailFormatter>();

            services.AddScheduler<EmailJob>(new SchedulerOptions { QueueName = Providers.Email });
        }
    }
}
