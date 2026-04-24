using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OpenAddressingHashtable<TKey, TValue> : IDictionary<TKey, TValue>
{
    public List<KeyValuePair<TKey, TValue>> buckets;
    public List<bool> occupied;
    public List<bool> deleted;
    public int capacity;
    public int count;
    private float loadFactor = 0.6f;
    private HashtableViewer.OpenAddressingMode mode;

    public OpenAddressingHashtable(HashtableViewer.OpenAddressingMode mode, int capacity = 16)
    {
        this.capacity = capacity;
        this.mode = mode;
        count = 0;
        buckets = new List<KeyValuePair<TKey, TValue>>();
        occupied = new List<bool>();
        deleted = new List<bool>();
        for (int i = 0; i < capacity; i++)
        {
            buckets.Add(default);
            occupied.Add(false);
            deleted.Add(false);
        }
    }
    private int GetIndex(TKey key)
    {
        return Mathf.Abs(key.GetHashCode() % capacity);
    }

    private int GetIndex2(TKey key)
    {
        return 1 + Mathf.Abs(key.GetHashCode() % (capacity - 1));
    }
    private int Probe(TKey key, int i)
    {
        int hash = GetIndex(key);
        switch (mode)
        {
            case HashtableViewer.OpenAddressingMode.linear:
                return (hash + i) % capacity;
            case HashtableViewer.OpenAddressingMode.quadratic:
                return (hash + i * i) % capacity;
            case HashtableViewer.OpenAddressingMode.doubleHash:
                return (hash + i * GetIndex2(key)) % capacity;
        }
        return hash;
    }
    private void Resize()
    {
        int oldCapacity = capacity;
        capacity *= 2;

        var tempBuckets = new List<KeyValuePair<TKey, TValue>>();
        var tempOccupied = new List<bool>();
        var tempDeleted = new List<bool>();

        for (int i = 0; i < capacity; i++)
        {
            tempBuckets.Add(default);
            tempOccupied.Add(false);
            tempDeleted.Add(false);
        }
        for (int i = 0; i < oldCapacity; i++)
        {
            if (occupied[i] && !deleted[i])
            {
                for (int j = 0; j < capacity; j++)
                {
                    int newIndex = Probe(buckets[i].Key, j);
                    if (!tempOccupied[newIndex])
                    {
                        tempBuckets[newIndex] = buckets[i];
                        tempOccupied[newIndex] = true;
                        break;
                    }
                }
            }
        }
        buckets = tempBuckets;
        occupied = tempOccupied;
        deleted = tempDeleted;
    }
    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out var value))
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
            for (int i = 0; i < capacity; i++)
            {
                if (occupied[i] && !deleted[i])
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
                if (occupied[i] && !deleted[i])
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
        if ((float)count / capacity > loadFactor)
        {
            Resize();
        }
        for (int i = 0; i < capacity; i++)
        {
            int index = Probe(key, i);
            if (!occupied[index] || deleted[index])
            {
                buckets[index] = new KeyValuePair<TKey, TValue>(key, value);
                occupied[index] = true;
                deleted[index] = false;
                count++;
                return;
            }
            if (buckets[index].Key.Equals(key))
            {
                throw new ArgumentException($"키가 {key} 이미 존재합니다");
            }
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        for(int i = 0; i < capacity; i++)
        {
            buckets[i] = default;
            occupied[i] = false;
            deleted[i] = false;
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
            if (occupied[i] && !deleted[i])
            {
                yield return buckets[i];
            }
        }
    }

    public bool Remove(TKey key)
    {
        for (int i = 0; i < capacity; i++)
        {
            int index = Probe(key, i);
            if (!occupied[index])
            {
                break;
            }
            if (!deleted[index] && buckets[index].Key.Equals(key))
            {
                deleted[index] = true;
                count--;
                return true;
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
        for (int i = 0; i < capacity; i++)
        {
            int index = Probe(key, i);
            if (!occupied[index])
            {
                break;
            }
            if (!deleted[index] && buckets[index].Key.Equals(key))
            {
                value = buckets[index].Value;
                return true;
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
