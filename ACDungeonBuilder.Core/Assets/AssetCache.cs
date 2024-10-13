using System;
using System.Collections.Generic;

namespace ACDungeonBuilder.Core.Assets
{
    public class AssetCache<T> where T : class
    {
        private readonly Dictionary<uint, WeakReference<T>> _cache = new Dictionary<uint, WeakReference<T>>();

        public T GetOrLoad(uint fileId, Func<uint, T> loadFunc)
        {
            if (_cache.TryGetValue(fileId, out var weakRef))
            {
                if (weakRef.TryGetTarget(out var asset))
                {
                    return asset;
                }
            }

            var newAsset = loadFunc(fileId);
            _cache[fileId] = new WeakReference<T>(newAsset);
            return newAsset;
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}