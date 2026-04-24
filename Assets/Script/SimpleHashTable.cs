using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    public List<KeyValuePair<TKey, TValue>> buckets;
    private List<bool> occupied;
    private int count;
    public int capacity;
    private float loadFactor = 0.75f;

    public event Action<int, int> OnResize;

    public SimpleHashTable(int capacity = 16)
    {
        this.capacity = capacity;
        buckets = new List<KeyValuePair<TKey, TValue>>();
        occupied = new List<bool>();

        for (int i = 0; i < capacity; i++)
        {
            buckets.Add(default); 
            occupied.Add(false);
        }
        count = 0;
    }
    private int GetIndex(TKey key)
    {
        return Mathf.Abs(key.GetHashCode()) % capacity;
    }

    private void Resize()
    {
        int oldCapacity = capacity;
        capacity *= 2;
        var tempBuckets = new List<KeyValuePair<TKey, TValue>>();
        var tempOccupied = new List<bool>();
        for (int i = 0; i < capacity; i++)
        {
            tempBuckets.Add(default);
            tempOccupied.Add(false);
        }
        for (int i = 0; i < oldCapacity; i++)
        {
            if (occupied[i])
            {
                int newIndex = Mathf.Abs(buckets[i].Key.GetHashCode()) % capacity;
                tempBuckets[newIndex] = buckets[i];
                tempOccupied[newIndex] = occupied[i];
            }
        }
        buckets = tempBuckets;
        occupied = tempOccupied;

        OnResize?.Invoke(oldCapacity, capacity);
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
        set => Add(key, value); 
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var list = new List<TKey>();
            for(int i = 0; i < capacity; i++)
            {
                if (occupied[i])
                {
                    list.Add(buckets[i].Key);
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
                if (occupied[i])
                {
                    list.Add(buckets[i].Value);
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
        if (occupied[index])
        {
            throw new InvalidOperationException($"충돌 발생: 인덱스 {index}");
        }
        buckets[index] = new KeyValuePair<TKey, TValue>(key, value);
        occupied[index] = true;
        count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        buckets = new List<KeyValuePair<TKey, TValue>>();
        occupied = new List<bool>();
        for (int i = 0; i < capacity; i++)
        {
            buckets.Add(default); // 빈 슬롯으로 초기화
            occupied.Add(false);
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
            if (occupied[i])
            {
                yield return buckets[i];
            }
        }
    }

    public bool Remove(TKey key)
    {
        int index = GetIndex(key);
        if (occupied[index] && buckets[index].Key.Equals(key))
        {
            buckets[index] = default;
            occupied[index] = false;
            count--;
            return true;
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
        if (occupied[index] == true && buckets[index].Key.Equals(key))
        {
            value = buckets[index].Value;
            return true;
        }
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
