using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionContainer : MonoBehaviour
{
    public Text title;
    public Image[] item;
    public Text [] quantity;
    public Button clearMission;
    public Transform clearedObject;
    public Color clearedColor;

    public void SetVisual(bool cleared, bool posible)
    {
        clearedObject.gameObject.SetActive(cleared);
        if (!cleared)
        {
            SetTextColor(Color.black);
            clearMission.gameObject.SetActive(posible);
        }
        else
        {
            SetTextColor(clearedColor);
            clearMission.gameObject.SetActive(false);
        }

    }

    void SetTextColor(Color c)
    {
        title.color = c;
        foreach (Text t in quantity)
            t.color = c;
    }

}
