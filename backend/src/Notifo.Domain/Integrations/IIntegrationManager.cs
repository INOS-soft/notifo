﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Integrations
{
    public interface IIntegrationManager
    {
        IEnumerable<IntegrationDefinition> Definitions { get; }

        bool IsConfigured<T>(App app, bool test);

        Task HandleConfigured(ConfiguredIntegration configured, ConfiguredIntegration? previous);

        IEnumerable<T> Resolve<T>(App app, bool test) where T : class;
    }
}
