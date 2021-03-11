using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PauseMenuManager : MonoBehaviour
{
    public Sprite[] itemSprites;
    public Image[] itemImages;
    public MissionContainer[] missionList;
    public Text[] itemQuantity;

    public List<Mission> mission;
    public Transform missionContainer;
    public Scrollbar scrollbar;
    public float moveDistance;

    public PlayerManager player;

    int missionClearedCount;
    Vector3 initialPos;

    void Start()
    {
        missionClearedCount = 0;
        initialPos = missionContainer.transform.localPosition;
        for (int i = 0; i < itemImages.Length && i < itemSprites.Length; i++)
            itemImages[i].sprite = itemSprites[i];
    }

    private void Update()
    {
        initialPos.y = scrollbar.value * moveDistance;
        missionContainer.transform.localPosition = initialPos;
    }

    public void StartMenu()
    {
        int i;
        for (i = 0; i < player.obtenibles.Length; i++)
            itemQuantity[i].text = player.obtenibles[i].ToString();

        for (i = 0; i < mission.Count; i++)
            SetContainer(i);

        SetContainer(missionList[i], player.recoverHealth);

        i++;
        if (player.recoverCrash != null)
        {
            missionList[i].gameObject.SetActive(true);
            SetContainer(missionList[i], player.recoverCrash);
        }
        else
            missionList[i].gameObject.SetActive(false);

    }

    void SetContainer(int index)
    {
        missionList[index].title.text = mission[index].title;
        for (int i = 0; i < 3; i++)
        {
            missionList[index].item[i].sprite = itemSprites[mission[index].item[i]];
            missionList[index].quantity[i].text = mission[index].quantity[i].ToString();
        }
        missionList[index].SetVisual(mission[index].cleared, isPosible(mission[index]));
    }

    void SetContainer(MissionContainer container, Mission m)
    {
        container.title.text = m.title;
        for (int i = 0; i < 3; i++)
        {
            container.item[i].sprite = itemSprites[m.item[i]];
            container.quantity[i].text = m.quantity[i].ToString();
        }
        container.SetVisual(m.cleared, isPosible(m));
    }

    bool isPosible(Mission m)
    {
        if (!m.cleared)
        {
            for(int i = 0; i < m.item.Length && i < m.quantity.Length; i++)
                if (m.quantity[i] > player.obtenibles[m.item[i]])
                    return false;
            return true;
        }
        else
            return false;
    }

    public void ClearMission(int index)
    {
        missionList[index].SetVisual(true, false);
        if(index < mission.Count)
        {
            missionClearedCount++;
            mission[index].cleared = true;
            for(int i = 0; i < mission[index].item.Length; i++)
                player.obtenibles[mission[index].item[i]] -= mission[index].quantity[i];
        }
        player.ClearMission(index, missionClearedCount == mission.Count);
    }

}

[Serializable]
public class Mission
{
    public string title;
    public int[] item;
    public int[] quantity;
    public bool cleared;

    public Mission(string t, int s)
    {
        title = t;
        item = new int[s];
        quantity = new int[s];
        cleared = false;
    }
}
