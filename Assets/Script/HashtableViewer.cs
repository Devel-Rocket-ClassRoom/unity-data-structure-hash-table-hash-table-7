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


    #region DUMMY
    static public int bucketSize = 15;
    List<KeyValuePair<int, string>> data = new(bucketSize);
    List<LinkedList<KeyValuePair<int, string>>> linkedData = new(bucketSize);
    #endregion


    private void Start()
    {
        OnClear();
    }
    public void OnOptionChanged()
    {
        hashTableMode = (HashtableMode)typeDropdown.value;

        if(hashTableMode == HashtableMode.openAddressing)
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
        int key = Random.Range(0, int.MaxValue);
        string value = "RANDOM VALUE";

        Add(key, value);
    }

    private void Add(int key, string value)
    {
        #region DUMMY
        data[key % bucketSize] = new KeyValuePair<int, string>(key, value);
        if (linkedData[key % bucketSize] == null)
        {
            linkedData[key % bucketSize] = new();
        }
        linkedData[key % bucketSize].AddLast(new KeyValuePair<int, string>(key, value));
        #endregion

        UpdateUi();
    }

    public void OnClear()
    {
        #region DUMMY
        data.Clear();
        linkedData.Clear();
        
        for (int i = 0; i < bucketSize; i++)
        {
            data.Add(new KeyValuePair<int, string>(0, null));
            linkedData.Add(null);
        }
        #endregion

        UpdateUi();
    }

    public void UpdateUi()
    {
        for (int i = bucketViewers.Count; i < bucketSize; i++)
        {
            GameObject newViewer = GameObject.Instantiate(bucketUiPrefab,bucketScrollRect.content);
            Image colorViewer = newViewer.GetComponent<Image>();
            TextMeshProUGUI text = newViewer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            bucketColorViewer.Add(colorViewer);
            bucketViewers.Add(text);
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
                        if (linkedData[i] == null)
                        {
                            bucketColorViewer[i].color = emptyColor;
                            bucketViewers[i].text = "EMPTY";
                        }
                        else
                        {
                            var node = linkedData[i].First;

                            StringBuilder resultBuilder = new StringBuilder();
                            while (node != null)
                            {
                                resultBuilder.Append(string.Format(viewerFormat, node.Value.Key, node.Value.Value));
                                resultBuilder.Append(chainSeperator);
                            }
                            resultBuilder.Remove(resultBuilder.Length - 1, 1);

                            bucketColorViewer[i].color = fullColor;
                            bucketViewers[i].text = resultBuilder.ToString();
                        }
                        break;
                }

            }
        }
    }
}
