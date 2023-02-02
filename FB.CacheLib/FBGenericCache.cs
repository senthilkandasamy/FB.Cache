using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace FB.CacheLib
{
    public class FBGenericCache<TKey,TValue> : IGenericCache<TKey, TValue>
    {
        private int capacity;
        private readonly ConcurrentDictionary<TKey, LinkedListNode<FBCacheItem>> fbDictionary;
        private readonly LinkedList<FBCacheItem> linkedList = new LinkedList<FBCacheItem>();
        public FBGenericCache(int capacity, int concurrencyLevel)
        {
            this.capacity = capacity;
            int actualDictionaryCapacity = capacity;
            fbDictionary = new ConcurrentDictionary<TKey, LinkedListNode<FBCacheItem>>(actualDictionaryCapacity, concurrencyLevel);
        }

        public int CacheCount { get { return fbDictionary.Count; } }

        public event EventHandler<string> CacheEvictedEvent;


        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        
        public bool TryGet(TKey key, out TValue value)
        {
            LinkedListNode<FBCacheItem> cachedItem = null;
            bool hasItem = false;
            hasItem = this.fbDictionary.TryGetValue(key, out cachedItem);
            if(!hasItem)
            {
                value =  default(TValue);
                return false;
            }

            //Item is accessed move to End
            MoveItemToEnd(cachedItem);
            value = cachedItem.Value.Value;
            return hasItem;
        }


        private void MoveItemToEnd(LinkedListNode<FBCacheItem> node)
        {
            // If the node has already been removed from the list, ignore.
            // E.g. thread A reads x from the dictionary. Thread B adds a new item, removes x from 
            // the List & Dictionary. Now thread A will try to move x to the end of the list.
            if (node.List == null)
            {
                return;
            }

            lock (this.linkedList)
            {
                if (node.List == null)
                {
                    return;
                }

                linkedList.Remove(node);
                linkedList.AddLast(node);
            }
        }

        public bool AddOrUpdate(TKey key, TValue newValue)
        {
            if (key == null)
            {
               throw new ArgumentNullException("missing key");
            }

            if(this.fbDictionary.TryGetValue(key, out var cachedItem))
            {
                cachedItem.Value.Value = newValue;
                return true;
            }
            //Now try to add since item doesn't exist in the dictionary
            LinkedListNode<FBCacheItem> newItem = new LinkedListNode<FBCacheItem>(new FBCacheItem(key, newValue));
            LinkedListNode<FBCacheItem> firstItem = null;

            // Get the node ready to add to dictionary
            lock (this.linkedList)
            {
                bool itemAdded =  this.fbDictionary.TryAdd(key,newItem);

                bool itemRemovedFromLinkedList = false;

                if (this.fbDictionary.Count > 0 &&  this.fbDictionary.Count > this.capacity && this.linkedList.Count != 0)
                {
                    firstItem = this.linkedList.First;
                    this.linkedList.RemoveFirst();
                    itemRemovedFromLinkedList = true;
                }
                
                linkedList.AddLast(newItem);

                if (firstItem != null)
                {
                    bool itemRemovedFromDictionary = this.fbDictionary.TryRemove(firstItem.Value.Key, out var removed);

                    if (itemRemovedFromDictionary && itemRemovedFromLinkedList)
                        this.CacheEvictedEvent.Invoke(this, removed.Value.Value.ToString());

                    return itemRemovedFromDictionary && itemRemovedFromLinkedList;
                }

                return itemAdded;
            }

        }

        public bool Clear()
        {
            lock(this.fbDictionary)
            {
                this.fbDictionary.Clear();
                this.linkedList.Clear();
            }
            return true;
        }

        public bool GetOrAdd(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        private class FBCacheItem
        {
            public FBCacheItem(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
            }
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }
    }
}
