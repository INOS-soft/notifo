﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.UserEvents;

namespace Notifo.Domain.UserNotifications
{
    public interface IUserNotificationService
    {
        Task DistributeAsync(UserEventMessage userEvent);

        Task TrackSeenAsync(IEnumerable<Guid> ids, TrackingDetails options);

        Task<(UserNotification?, App?)> TrackConfirmedAsync(Guid id, TrackingDetails options);
    }
}
