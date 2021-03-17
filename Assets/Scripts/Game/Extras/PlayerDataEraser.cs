using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataEraser : MonoBehaviour
{
    [Range(.0001f, 1)]
    public float gAStart;
    [Range(.0001f, 1)]
    public float mAStart;
    [Range(.0001f, 1)]
    public float eAStart;

    private void Awake()
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag("NotDestroy");
        if (list.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetInt("GameState", 0);
        PlayerPrefs.SetFloat("GeneralAudio", gAStart);
        PlayerPrefs.SetFloat("MusicAudio", mAStart);
        PlayerPrefs.SetFloat("EffectsAudio", eAStart);
        PlayerPrefs.Save();
    }
}
