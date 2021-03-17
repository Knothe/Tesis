using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Audio;

public class PauseMenuManager : MonoBehaviour
{
    public Transform mainPause;
    public Transform audioPause;

    public Sprite[] itemSprites;
    public MissionContainer[] missionList;

    public List<Mission> mission;
    public float moveDistance;

    public Slider general;
    public Slider music;
    public Slider sfx;

    public AudioMixer mixer;


    public PlayerManager player;

    int missionClearedCount;

    void OnEnable()
    {
        mainPause.gameObject.SetActive(true);
        audioPause.gameObject.SetActive(false);

        general.value = PlayerPrefs.GetFloat("GeneralAudio");
        music.value = PlayerPrefs.GetFloat("MusicAudio");
        sfx.value = PlayerPrefs.GetFloat("EffectsAudio");
    }

    void Start()
    {
        missionClearedCount = 0;
    }


    public void SetValuesFromMenu(int i)
    {
        if(i == 0)
            PlayerPrefs.SetFloat("GeneralAudio", general.value);
        else if(i == 1)
            PlayerPrefs.SetFloat("MusicAudio", music.value);
        else if(i == 2)
            PlayerPrefs.SetFloat("EffectsAudio", sfx.value);
        PlayerPrefs.Save();
        SetAudioValues();
    }

    public void SetAudioValues()
    {
        mixer.SetFloat("MainVolume", Mathf.Log10(PlayerPrefs.GetFloat("GeneralAudio")) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicAudio")) * 20);
        mixer.SetFloat("SfxVolume", Mathf.Log10(PlayerPrefs.GetFloat("EffectsAudio")) * 20);
    }

    public void StartMenu()
    {
        int i;

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
            missionList[index].quantity[i].text = player.obtenibles[mission[index].item[i]].ToString() + " / " + mission[index].quantity[i].ToString();
        }
        missionList[index].SetVisual(mission[index].cleared, isPosible(mission[index]));
    }

    void SetContainer(MissionContainer container, Mission m)
    {
        container.title.text = m.title;
        for (int i = 0; i < 3; i++)
        {
            container.item[i].sprite = itemSprites[m.item[i]];
            container.quantity[i].text = player.obtenibles[m.item[i]].ToString() + " / " + m.quantity[i].ToString();
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
            if(index != mission.Count - 1)
                missionClearedCount++;
            mission[index].cleared = true;
            for(int i = 0; i < mission[index].item.Length; i++)
                player.obtenibles[mission[index].item[i]] -= mission[index].quantity[i];
        }
        player.ClearMission(index, missionClearedCount == mission.Count - 1);
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
