using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
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

    public string viewerFormat = "{0} : {1}";
    public string chainSeperator = "->";





    HashtableMode hashTableMode = HashtableMode.simple;
    OpenAddressingMode openAddressingMode = OpenAddressingMode.linear;

    List<Image> bucketColorViewer = new();
    List<TextMeshProUGUI> bucketViewers = new();

    SimpleHashTable<int, string> simpleHashtable = new();
    ChainingHashTable<int, string> chainingHashtable = new();
    int selectedIndex = -1;

    #region DUMMY
    static public int bucketSize = 15;
    List<KeyValuePair<int, string>> data = new(bucketSize);
    #endregion


    private void Start()
    {
        OnClear();
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
                catch(Exception e)
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
                #region DUMMY
                data[key % bucketSize] = new KeyValuePair<int, string>(key, value);
                #endregion
                break;
        }


        UpdateUi();
    }

    public void OnClear()
    {
        #region DUMMY
        data.Clear();

        for (int i = 0; i < bucketSize; i++)
        {
            data.Add(new KeyValuePair<int, string>(0, null));
        }
        #endregion
        simpleHashtable.Clear();
        chainingHashtable.Clear();

        logText.text += $"\nCleared";
        UpdateUi();
    }

    public void UpdateUi()
    {
        switch (hashTableMode)
        {
            case HashtableMode.simple:
                bucketSize = simpleHashtable.capacity;
                break;
            case HashtableMode.chaining:
                bucketSize = 15;
                break;
            case HashtableMode.openAddressing:
                bucketSize = 15;
                break;
        }

        for (int i = bucketViewers.Count; i < bucketSize; i++)
        {
            GameObject newViewer = GameObject.Instantiate(bucketUiPrefab, bucketScrollRect.content);
            Image colorViewer = newViewer.GetComponent<Image>();
            TextMeshProUGUI text = newViewer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            bucketColorViewer.Add(colorViewer);
            bucketViewers.Add(text);

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
                                bucketViewers[i].text = "EMPTY";
                            }
                            else
                            {
                                bucketColorViewer[i].color = fullColor;
                                bucketViewers[i].text = simpleHashtable.buckets[i].Value;
                            }
                        }
                        else
                        {
                            bucketColorViewer[i].color = emptyColor;
                            bucketViewers[i].text = "EMPTY";
                        }
                        break;

                    case HashtableMode.openAddressing:
                        if (data[i].Value == null)
                        {
                            bucketColorViewer[i].color = emptyColor;
                            bucketViewers[i].text = "EMPTY";
                        }
                        else
                        {
                            bucketColorViewer[i].color = fullColor;
                            bucketViewers[i].text = data[i].Value;
                        }
                        break;
                    case HashtableMode.chaining:
                        if (i < chainingHashtable.buckets.Count)
                        {
                            if (chainingHashtable.buckets[i] == null)
                            {
                                bucketColorViewer[i].color = emptyColor;
                                bucketViewers[i].text = "EMPTY";
                            }
                            else
                            {
                                var node = chainingHashtable.buckets[i].First;

                                StringBuilder resultBuilder = new StringBuilder();
                                while (node != null)
                                {
                                    resultBuilder.Append(string.Format(viewerFormat, node.Value.Key, node.Value.Value));
                                    resultBuilder.Append(chainSeperator);
                                    node = node.Next;
                                }
                                resultBuilder.Remove(resultBuilder.Length - 1, 1);

                                bucketColorViewer[i].color = fullColor;
                                bucketViewers[i].text = resultBuilder.ToString();
                            }
                        }
                        else
                        {
                            bucketColorViewer[i].color = emptyColor;
                            bucketViewers[i].text = "EMPTY";
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
        if (selectedIndex == -1)
            return;

        switch (hashTableMode)
        {
            case HashtableMode.simple:
                var key = simpleHashtable.buckets[selectedIndex].Key;
                simpleHashtable.Remove(key);
                break;
            case HashtableMode.chaining:
                break;
            case HashtableMode.openAddressing:
                break;
        }

        UpdateUi();
    }
}
