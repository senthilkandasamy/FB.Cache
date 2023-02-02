using System;
using System.Collections.Generic;
using System.Text;

namespace FB.CacheLib
{
    public interface IGenericCache<TKey, TValue> 
    {
        public event EventHandler<string> CacheEvictedEvent;

        bool TryGet(TKey key, out TValue value);

        bool GetOrAdd(TKey key, TValue value);

        bool AddOrUpdate(TKey key, TValue value);


        bool Remove(TKey key);

        bool Clear();

    }
}
