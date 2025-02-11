﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Notifo.Domain.Channels.Sms
{
    public delegate Task SmsHandler(SmsResponse response);

    public interface ISmsSender
    {
        Task<SmsResult> SendAsync(string to, string body, string? token = null, CancellationToken ct = default);

        Task HandleStatusAsync(HttpContext httpContext);

        Task RegisterAsync(SmsHandler handler);
    }
}
