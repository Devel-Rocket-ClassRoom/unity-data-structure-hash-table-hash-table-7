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

    public OpenAddressingHashtable(int capacity = 16, HashtableViewer.OpenAddressingMode mode)
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
        return Mathf.Abs(key.GetHashCode() % (capacity - 1));
    }
    private int Probe(TKey key, int i)
    {
        int hash = GetIndex(key);
        switch (mode)
        {
            case HashtableViewer.OpenAddressingMode.linear:
                return (hash + i) % capacity;
                break;
            case HashtableViewer.OpenAddressingMode.quadratic:
                return (hash + i * i) % capacity;
                break;
            case HashtableViewer.OpenAddressingMode.doubleHash:
                return (hash + i * GetIndex2(key) % capacity);
                break;
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
    public TValue this[TKey key] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public ICollection<TKey> Keys => throw new System.NotImplementedException();

    public ICollection<TValue> Values => throw new System.NotImplementedException();

    public int Count => throw new System.NotImplementedException();

    public bool IsReadOnly => throw new System.NotImplementedException();

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
        throw new System.NotImplementedException();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        throw new System.NotImplementedException();
    }

    public bool ContainsKey(TKey key)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(TKey key)
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
