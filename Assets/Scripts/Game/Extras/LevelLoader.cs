using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public AudioMixer mixer;

    public Slider general;
    public Slider music;
    public Slider sfx;

    public GameObject mainMenu;
    public GameObject mainMenuBg;
    public GameObject victory;
    public GameObject victoryBg;
    public GameObject defeat;
    public GameObject defeatBg;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 1;
        AudioListener.pause = false;

        // Win = 1, Loose = 2, Nothing = 0
        SetMenu(PlayerPrefs.GetInt("GameState"));

        general.value = PlayerPrefs.GetFloat("GeneralAudio");
        music.value = PlayerPrefs.GetFloat("MusicAudio");
        sfx.value = PlayerPrefs.GetFloat("EffectsAudio");

        SetAudio();
    }

    public void SetValuesFromMenu(int i)
    {
        if (i == 0)
            PlayerPrefs.SetFloat("GeneralAudio", general.value);
        else if (i == 1)
            PlayerPrefs.SetFloat("MusicAudio", music.value);
        else if (i == 2)
            PlayerPrefs.SetFloat("EffectsAudio", sfx.value);
        PlayerPrefs.Save();

        SetAudio();
    }

    void SetAudio()
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicAudio")) * 20);
        mixer.SetFloat("MainVolume", Mathf.Log10(PlayerPrefs.GetFloat("GeneralAudio")) * 20);
        mixer.SetFloat("SfxVolume", Mathf.Log10(PlayerPrefs.GetFloat("EffectsAudio")) * 20);
    }

    void SetMenu(int i)
    {
        mainMenu.SetActive(false);
        mainMenuBg.SetActive(false);
        victory.SetActive(false);
        victoryBg.SetActive(false);
        defeat.SetActive(false);
        defeatBg.SetActive(false);


        if (i == 0)
        {
            mainMenu.SetActive(true);
            mainMenuBg.SetActive(true);
        }
        else if (i == 1)
        {
            victory.SetActive(true);
            victoryBg.SetActive(true);
        }
        else if (i == 2)
        {
            defeat.SetActive(true);
            defeatBg.SetActive(true);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quitting");
    }

    public void LoadLevel(int sceneIndex)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            Debug.Log(progress);
            yield return null;
        }
    }
}
