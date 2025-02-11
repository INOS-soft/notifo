﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Notifo.Identity.InMemory
{
    public static class Extensions
    {
        public static ValueTask<T> AsValueTask<T>(this T value)
        {
            return new ValueTask<T>(value);
        }
    }
}
