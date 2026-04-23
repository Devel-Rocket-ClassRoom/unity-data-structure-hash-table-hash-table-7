using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI;

public class ChainingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    public List<LinkedList<KeyValuePair<TKey, TValue>>> buckets;
    private int count;
    private int capacity;
    private float loadFactor = 0.75f;

    public ChainingHashTable(int capacity = 16)
    {
        this.capacity = capacity;
        buckets = new List<LinkedList<KeyValuePair<TKey, TValue>>>();
        for (int i = 0; i < capacity; i++)
        {
            buckets.Add(null);
        }
        count = 0;
    }
    private void Resize()
    {
        int oldCapacity = capacity;
        capacity *= 2;

        var tempBuckets = new List<LinkedList<KeyValuePair<TKey, TValue>>>();
        for (int i = 0; i < capacity; i++)
        {
            tempBuckets.Add(null);
        }

        for (int i = 0; i < oldCapacity; i++)
        {
            if (buckets[i] != null)
            {
                foreach (var kvp in buckets[i])
                {
                    int newIndex = Mathf.Abs(kvp.Key.GetHashCode()) % capacity;
                    if (tempBuckets[newIndex] == null)
                    {
                        tempBuckets[newIndex] = new LinkedList<KeyValuePair<TKey, TValue>>();
                    }
                    tempBuckets[newIndex].AddLast(kvp);
                }
            }
        }
        buckets = tempBuckets;
    }

    private int GetIndex(TKey key)
    {
        return Mathf.Abs(key.GetHashCode()) % capacity;
    }

    public TValue this[TKey key]
    {
        get
        {
            if(TryGetValue(key, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException();
        }
        set=> Add(key, value);
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var list = new List<TKey>();
            for(int i = 0; i < capacity; i++)
            {
                if (buckets[i] != null)
                {
                    foreach(var kvp in buckets[i])
                    {
                        list.Add(kvp.Key);
                    }
                }
            }
            return list;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var list = new List<TValue>();
            for (int i = 0; i < capacity; i++)
            {
                if (buckets[i] != null)
                {
                    foreach (var kvp in buckets[i])
                    {
                        list.Add(kvp.Value);
                    }
                }
            }
            return list;
        }
    }

    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if ((float)count / capacity >= loadFactor)
        {
            Resize();
        }
        int index = GetIndex(key);
        if (buckets[index] == null)
        {
            buckets[index] = new LinkedList<KeyValuePair<TKey, TValue>>();
        }
        foreach (var kvp in buckets[index])
        {
            if (kvp.Key.Equals(key))
            {
                throw new ArgumentException($"키가 {key} 이미 존재합니다");
            }
        }
        buckets[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
        count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        for(int i = 0; i < capacity; i++)
        {
            buckets[i] = null;
        }
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach(var kvp in this)
        {
            array[arrayIndex++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < capacity; i++)
        {
            if (buckets[i] != null)
            {
                foreach(var kvp in buckets[i])
                {
                    yield return kvp;
                }
            }
        }
    }

    public bool Remove(TKey key)
    {
        int index = GetIndex(key);
        if (buckets[index] != null)
        {
            var node = buckets[index].First;
            while (node != null)
            {
                if (node.Value.Key.Equals(key))
                {
                    buckets[index].Remove(node);
                    count--;
                    return true;
                }
                node = node.Next;
            }
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = GetIndex(key);
        if (buckets[index] != null)
        {
            foreach (var kvp in buckets[index])
            {
                if (kvp.Key.Equals(key))
                {
                    value = kvp.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
