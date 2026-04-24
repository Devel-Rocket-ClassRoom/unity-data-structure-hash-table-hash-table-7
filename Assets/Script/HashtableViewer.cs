using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HashtableViewer : MonoBehaviour
{
    public enum HashtableMode
    {
        simple,
        chaining,
        openAddressing,
    }

    public enum OpenAddressingMode
    {
        linear,
        quadratic,
        doubleHash,
    }




    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown openHashtableDropdown;
    public TMP_InputField keyInput;
    public TMP_InputField valueInput;
    public TextMeshProUGUI logText;

    public ScrollRect bucketScrollRect;
    public GameObject bucketUiPrefab;

    [Header("Config")]
    public Color emptyColor = Color.white;
    public Color fullColor = Color.green;
    public Color tombStoneColor = Color.red;

    public string viewerFormat = "{0} : {1}";
    public string chainSeperator = "->";

    public string resizeLogFormat = "\n리사이즈 감지 {0}->{1}";
    public string probeLogFormat = "\n프로빙 감지 {0}->{1}";




    HashtableMode hashTableMode = HashtableMode.simple;
    OpenAddressingMode openAddressingMode = OpenAddressingMode.linear;

    List<Image> bucketColorViewer = new();
    List<TextMeshProUGUI> bucketViewers = new();

    SimpleHashTable<int, string> simpleHashtable = new();
    ChainingHashTable<int, string> chainingHashtable = new();

    OpenAddressingHashtable<int, string> openHashTableLinear = new(OpenAddressingMode.linear);
    OpenAddressingHashtable<int, string> openHashTableQuadratic = new(OpenAddressingMode.quadratic);
    OpenAddressingHashtable<int, string> openHashTableDoubleHash = new(OpenAddressingMode.doubleHash);

    int selectedIndex = -1;


    private void Start()
    {
        OnClear();
    }

    private void OnEnable()
    {
        simpleHashtable.OnResize += OnResize;
        chainingHashtable.OnResize += OnResize;
        openHashTableLinear.OnResize += OnResize;
        openHashTableQuadratic.OnResize += OnResize;
        openHashTableDoubleHash.OnResize += OnResize;

        openHashTableLinear.OnProbe += OnProbe;
        openHashTableQuadratic.OnProbe += OnProbe;
        openHashTableDoubleHash.OnProbe += OnProbe;
    }

    private void OnDisable()
    {
        simpleHashtable.OnResize -= OnResize;
        chainingHashtable.OnResize -= OnResize;
        openHashTableLinear.OnResize -= OnResize;
        openHashTableQuadratic.OnResize -= OnResize;
        openHashTableDoubleHash.OnResize -= OnResize;

        openHashTableLinear.OnProbe -= OnProbe;
        openHashTableQuadratic.OnProbe -= OnProbe;
        openHashTableDoubleHash.OnProbe -= OnProbe;
    }

    private void OnResize(int oldSize, int newSize)
    {
        logText.text += string.Format(resizeLogFormat, oldSize.ToString(), newSize.ToString());
    }

    private void OnProbe(int oldIndex, int newIndex)
    {
        logText.text += string.Format(probeLogFormat, oldIndex.ToString(), newIndex.ToString());
    }


    public void OnOptionChanged()
    {
        hashTableMode = (HashtableMode)typeDropdown.value;

        if (hashTableMode == HashtableMode.openAddressing)
        {
            openHashtableDropdown.interactable = true;
            openAddressingMode = (OpenAddressingMode)openHashtableDropdown.value;
        }
        else
        {
            openHashtableDropdown.interactable = false;
        }

        OnClear();
    }

    public void OnAdd()
    {
        int key = int.Parse(keyInput.text);
        string value = valueInput.text;

        Add(key, value);
    }

    public void OnRandomAdd()
    {
        int key = UnityEngine.Random.Range(0, int.MaxValue);
        string value = "RANDOM VALUE";

        Add(key, value);
    }

    private void Add(int key, string value)
    {

        switch (hashTableMode)
        {
            case HashtableMode.simple:
                try
                {
                    simpleHashtable[key] = value;
                    logText.text += $"\n{key}:{value} Added Successfully";
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    logText.text += $"\n{e.Message}";
                }
                break;
            case HashtableMode.chaining:
                try
                {
                    chainingHashtable.Add(key, value);
                    logText.text += $"\n{key}:{value} Added Successfully";
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    logText.text += $"\n{e.Message}";
                }
                break;
            case HashtableMode.openAddressing:
                try
                {
                    switch (openAddressingMode)
                    {
                        case OpenAddressingMode.linear:
                            openHashTableLinear.Add(key, value);
                            break;
                        case OpenAddressingMode.quadratic:
                            openHashTableQuadratic.Add(key, value);
                            break;
                        case OpenAddressingMode.doubleHash:
                            openHashTableDoubleHash.Add(key, value);
                            break;
                    }
                    logText.text += $"\n{key}:{value} Added Successfully";
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    logText.text += $"\n{e.Message}";
                }
                break;
        }


        UpdateUi();
    }

    public void OnClear()
    {
        simpleHashtable.Clear();
        chainingHashtable.Clear();
        openHashTableLinear.Clear();
        openHashTableQuadratic.Clear();
        openHashTableDoubleHash.Clear();

        logText.text = $"Cleared";
        UpdateUi();
    }

    public void UpdateUi()
    {
        int bucketSize = 0;
        switch (hashTableMode)
        {
            case HashtableMode.simple:
                bucketSize = simpleHashtable.capacity;
                break;
            case HashtableMode.chaining:
                bucketSize = chainingHashtable.capacity;
                break;
            case HashtableMode.openAddressing:
                switch (openAddressingMode)
                {
                    case OpenAddressingMode.linear:
                        bucketSize = openHashTableLinear.capacity;
                        break;
                    case OpenAddressingMode.quadratic:
                        bucketSize = openHashTableQuadratic.capacity;
                        break;
                    case OpenAddressingMode.doubleHash:
                        bucketSize = openHashTableDoubleHash.capacity;
                        break;
                }
                break;
        }

        for (int i = bucketViewers.Count; i < bucketSize; i++)
        {
            GameObject newViewer = GameObject.Instantiate(bucketUiPrefab, bucketScrollRect.content);
            Image colorViewer = newViewer.GetComponent<Image>();
            DragPassingScrollRect rect = newViewer.transform.GetChild(0).GetComponent<DragPassingScrollRect>();
            TextMeshProUGUI text = rect.content.GetChild(0).GetComponent<TextMeshProUGUI>();

            bucketColorViewer.Add(colorViewer);
            bucketViewers.Add(text);
            rect.parentScroll = bucketScrollRect;

            Button btn = newViewer.GetComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() =>
            {
                Select(index);
            });
        }

        for (int i = 0; i < bucketViewers.Count; i++)
        {
            if (i >= bucketSize)
            {
                bucketColorViewer[i].gameObject.SetActive(false);
            }
            else
            {
                bucketColorViewer[i].gameObject.SetActive(true);

                switch (hashTableMode)
                {
                    case HashtableMode.simple:
                        if (i < simpleHashtable.buckets.Count)
                        {
                            if (simpleHashtable.buckets[i].Value == null)
                            {
                                bucketColorViewer[i].color = emptyColor;
                                bucketViewers[i].text = $"I:{i} EMPTY";
                            }
                            else
                            {
                                bucketColorViewer[i].color = fullColor;
                                bucketViewers[i].text = $"I:{i} {string.Format(viewerFormat, simpleHashtable.buckets[i].Key, simpleHashtable.buckets[i].Value)}";
                            }
                        }
                        else
                        {
                            bucketColorViewer[i].color = emptyColor;
                            bucketViewers[i].text = $"I:{i} EMPTY";
                        }
                        break;

                    case HashtableMode.openAddressing:
                        switch (openAddressingMode)
                        {
                            case OpenAddressingMode.linear:
                                if (i < openHashTableLinear.buckets.Count)
                                {
                                    if (openHashTableLinear.deleted[i])
                                    {
                                        bucketColorViewer[i].color = tombStoneColor;
                                        bucketViewers[i].text = $"I:{i} EMPTY with Tombstone";
                                    }
                                    else if (openHashTableLinear.buckets[i].Value == null)
                                    {
                                        bucketColorViewer[i].color = emptyColor;
                                        bucketViewers[i].text = $"I:{i} EMPTY";
                                    }
                                    else
                                    {
                                        bucketColorViewer[i].color = fullColor;
                                        bucketViewers[i].text = $"I:{i} {string.Format(viewerFormat, openHashTableLinear.buckets[i].Key, openHashTableLinear.buckets[i].Value)}";
                                    }
                                }
                                else
                                {
                                    bucketColorViewer[i].color = emptyColor;
                                    bucketViewers[i].text = $"I:{i} EMPTY";
                                }
                                break;
                            case OpenAddressingMode.quadratic:
                                if (i < openHashTableQuadratic.buckets.Count)
                                {
                                    if (openHashTableQuadratic.deleted[i])
                                    {
                                        bucketColorViewer[i].color = tombStoneColor;
                                        bucketViewers[i].text = $"I:{i} EMPTY with Tombstone";
                                    }
                                    else if (openHashTableQuadratic.buckets[i].Value == null)
                                    {
                                        bucketColorViewer[i].color = emptyColor;
                                        bucketViewers[i].text = $"I:{i} EMPTY";
                                    }
                                    else
                                    {
                                        bucketColorViewer[i].color = fullColor;
                                        bucketViewers[i].text = $"I:{i} {string.Format(viewerFormat, openHashTableQuadratic.buckets[i].Key, openHashTableQuadratic.buckets[i].Value)}";
                                    }
                                }
                                else
                                {
                                    bucketColorViewer[i].color = emptyColor;
                                    bucketViewers[i].text = $"I:{i} EMPTY";
                                }
                                break;
                            case OpenAddressingMode.doubleHash:
                                if (i < openHashTableDoubleHash.buckets.Count)
                                {
                                    if (openHashTableDoubleHash.deleted[i])
                                    {
                                        bucketColorViewer[i].color = tombStoneColor;
                                        bucketViewers[i].text = $"I:{i} EMPTY with Tombstone";
                                    }
                                    else if (openHashTableDoubleHash.buckets[i].Value == null)
                                    {
                                        bucketColorViewer[i].color = emptyColor;
                                        bucketViewers[i].text = $"I:{i} EMPTY";
                                    }
                                    else
                                    {
                                        bucketColorViewer[i].color = fullColor;
                                        bucketViewers[i].text = $"I:{i} {string.Format(viewerFormat, openHashTableDoubleHash.buckets[i].Key, openHashTableDoubleHash.buckets[i].Value)}";
                                    }
                                }
                                else
                                {
                                    bucketColorViewer[i].color = emptyColor;
                                    bucketViewers[i].text = $"I:{i} EMPTY";
                                }
                                break;
                        }
                        break;
                    case HashtableMode.chaining:
                        if (i < chainingHashtable.buckets.Count)
                        {
                            if (chainingHashtable.buckets[i] == null)
                            {
                                bucketColorViewer[i].color = emptyColor;
                                bucketViewers[i].text = $"I:{i} EMPTY";
                            }
                            else
                            {
                                var node = chainingHashtable.buckets[i].First;

                                StringBuilder resultBuilder = new StringBuilder();
                                resultBuilder.Append($"I:{i} ");
                                while (node != null)
                                {
                                    resultBuilder.Append(string.Format(viewerFormat, node.Value.Key, node.Value.Value));
                                    resultBuilder.Append(chainSeperator);
                                    node = node.Next;
                                }
                                resultBuilder.Remove(resultBuilder.Length - chainSeperator.Length, chainSeperator.Length);

                                bucketColorViewer[i].color = fullColor;
                                bucketViewers[i].text = resultBuilder.ToString();
                            }
                        }
                        else
                        {
                            bucketColorViewer[i].color = emptyColor;
                            bucketViewers[i].text = $"I:{i} EMPTY";
                        }
                        break;
                }

            }
        }
    }


    public void Select(int i)
    {
        ColorBlock newColor;
        if (selectedIndex != -1)
        {
            Button oldBtn = bucketColorViewer[selectedIndex].GetComponent<Button>();
            newColor = oldBtn.colors;
            newColor.normalColor = Color.white;
            newColor.selectedColor = Color.white;
            oldBtn.colors = newColor;
        }

        selectedIndex = i;
        Button btn = bucketColorViewer[selectedIndex].GetComponent<Button>();
        newColor = btn.colors;
        newColor.normalColor = Color.yellow;
        newColor.selectedColor = Color.yellow;
        btn.colors = newColor;
    }

    public void OnRemove()
    {

        if (int.TryParse(keyInput.text, out int key))
        {
            try
            {
                switch (hashTableMode)
                {
                    case HashtableMode.simple:
                        simpleHashtable.Remove(key);
                        break;
                    case HashtableMode.chaining:
                        chainingHashtable.Remove(key);
                        break;
                    case HashtableMode.openAddressing:
                        switch (openAddressingMode)
                        {
                            case OpenAddressingMode.linear:
                                openHashTableLinear.Remove(key);
                                break;
                            case OpenAddressingMode.quadratic:
                                openHashTableQuadratic.Remove(key);
                                break;
                            case OpenAddressingMode.doubleHash:
                                openHashTableDoubleHash.Remove(key);
                                break;
                        }
                        break;
                }
                logText.text += $"\n{key}: Removed Successfully";
            }
            catch (Exception e)
            {
                logText.text += "\n"+e.Message;
                Debug.LogException(e);
            }
        }
        else
        {
            logText.text += "\n키 형식이 올바르지 않습니다(INT)";
        }

        UpdateUi();
    }
}
