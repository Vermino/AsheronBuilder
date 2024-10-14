using System;
using System.Collections.Generic;

namespace AsheronBuilder.Core.Assets
{
    public class AssetCache<T> where T : class
    {
        private readonly Dictionary<uint, WeakReference<T>> _cache = new Dictionary<uint, WeakReference<T>>();
        private readonly object _lockObject = new object();

        public T GetOrLoad(uint fileId, Func<uint, T> loadFunc)
        {
            lock (_lockObject)
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
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _cache.Clear();
            }
        }

        public void Remove(uint fileId)
        {
            lock (_lockObject)
            {
                _cache.Remove(fileId);
            }
        }

        public bool TryGetValue(uint fileId, out T asset)
        {
            lock (_lockObject)
            {
                if (_cache.TryGetValue(fileId, out var weakRef) && weakRef.TryGetTarget(out asset))
                {
                    return true;
                }
            }

            asset = null;
            return false;
        }

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _cache.Count;
                }
            }
        }

        public void Trim()
        {
            lock (_lockObject)
            {
                var keysToRemove = new List<uint>();
                foreach (var kvp in _cache)
                {
                    if (!kvp.Value.TryGetTarget(out _))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }
    }
}