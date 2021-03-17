using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public Text text;

    public Color normalColor;
    public Color hoverColor;
    public Color selectedcolor;

    void Start()
    {
        text.color = normalColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
