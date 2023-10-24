using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public GameObject button1;
    public GameObject button2;

    // Start is called before the first frame update
    public void HideUI1()
    {
        button1.SetActive(false);
    }

    public void HideUI2()
    {
        button2.SetActive(false);
    }
}
