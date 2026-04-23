using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HashtableViewer : MonoBehaviour
{
    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown openHashtableDropdown;
    public TMP_InputField keyInput;
    public TMP_InputField valueInput;

    List<(int, string)> data;
    List<LinkedList<(int, string)>> linkedData;

    public void OnOptionChanged()
    {

    }

    public void OnAdd()
    {

    }

    public void OnRandomAdd()
    {

    }

    public void OnClear()
    {

    }
}
