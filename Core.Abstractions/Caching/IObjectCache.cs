﻿using System;
using Core.Abstractions.Types;

namespace Core.Abstractions.Caching
{
    public interface IObjectCache
    {
        void Write<T>(T entry, string key, TimeSpan duration) where T : class;
        void Clear(string key);
        Maybe<T> Read<T>(string key) where T : class;

    }
}
