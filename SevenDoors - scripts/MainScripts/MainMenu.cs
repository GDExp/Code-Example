using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private GameObject menu;
    private GameObject panel;
    private GameObject settings;
    private GameObject info;
    private GameObject level;

    [SerializeField]
    private GameObject current_menu;

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        PlayerPrefs.DeleteAll();
        Setup();
        Transform icon = menu.transform.Find("Icon");
        for (int i = 0; i < icon.childCount; ++i)
            icon.GetChild(i).gameObject.SetActive(false);
        if (!PlayerPrefs.HasKey("Input"))
        {
            if (Application.platform == RuntimePlatform.Android)
                PlayerPrefs.SetInt("Input", 1);//mobile input
            else
                PlayerPrefs.SetInt("Input", 0);//PC input
        }
        icon.GetChild(PlayerPrefs.GetInt("Input")).gameObject.SetActive(true);
        if(GameController.init != null && GameController.init.test_end)
        {
            current_menu.SetActive(false);
            current_menu = info;
            SetCurrentMenu(info);
            OpenInfo();
            GameController.init.test_end = false;
        }
        
    }
    //only edit mod
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DataManager data = new DataManager();
            data.DeletData();
        }
        if (Input.GetKeyDown(KeyCode.T))
            UnityEngine.SceneManagement.SceneManager.LoadScene(3);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    private void Setup()
    {
        menu = transform.Find("Menu").gameObject;
        panel = transform.Find("Panel").gameObject;
        SetupPanel();
        panel.SetActive(false);
        settings = transform.Find("Settings").gameObject;
        SetupSettings();
        settings.SetActive(false);
        info = transform.Find("Info").gameObject;
        info.SetActive(false);
        level = transform.Find("Level").gameObject;
        level.SetActive(false);

        current_menu = menu;
        SetupMainButtons();
    }

    private void SetupMainButtons()
    {
        Transform buttons = menu.transform.Find("Buttons");
        Button current;
        //play
        current = buttons.Find("Play").GetComponent<Button>();
        current.onClick.AddListener(() => { SetCurrentMenu(level); OpenLevelPanel(); });
        //info - menu
        current = buttons.Find("Info").GetComponent<Button>();
        current.onClick.AddListener(() => { SetCurrentMenu(info); OpenInfo(); });
        //settings - menu
        current = buttons.Find("Settings").GetComponent<Button>();
        current.onClick.AddListener(() => { SetCurrentMenu(settings); OpenSettings(); });
        //swith
        current = buttons.Find("Swith").GetComponent<Button>();
        current.onClick.AddListener(() => SwithInputType(menu.transform.Find("Icon")));

        //boost
        current = buttons.Find("Boost").GetComponent<Button>();
        current.onClick.AddListener(() => BoostLevelToTester());
    }

    private void SetupPanel()
    {
        panel.GetComponentInChildren<Button>().onClick.AddListener(() => CloseCurrentMenu());
    }

    private void SetupSettings()
    {
        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetFloat("Music", 0.5f);
            PlayerPrefs.SetFloat("Sound", 0.5f);
        }
        Transform sliders = settings.transform.Find("Sliders");
        Slider current = sliders.GetChild(0).GetComponent<Slider>();
        current.value = PlayerPrefs.GetFloat("Music");
        current.onValueChanged.AddListener(delegate { ChangeMusic(settings.transform.Find("Sliders").GetChild(0).GetComponent<Slider>()); });
        current = sliders.GetChild(1).GetComponent<Slider>();
        current.value = PlayerPrefs.GetFloat("Sound");
        current.onValueChanged.AddListener(delegate { ChangeSound(settings.transform.Find("Sliders").GetChild(1).GetComponent<Slider>()); });
        
    }

    private void SetupLevelPanel()
    {
        DataManager data = new DataManager();
        data.LoadData();
        int opened_lvl = data.GetOpenLevel();

        Button[] lvl_buttons = level.transform.Find("Open").GetComponentsInChildren<Button>();
        Image[] locked = level.transform.Find("Close").GetComponentsInChildren<Image>();

        for(int i = 0; i < lvl_buttons.Length; ++i, opened_lvl--)
        {
            int index = i+1;
            lvl_buttons[i].onClick.AddListener(() => LoadLevel(index));
            if (opened_lvl > 0 )
            {
                if (i != 0)
                    locked[i-1].enabled = false;
            }
            else
                lvl_buttons[i].interactable = false;
        }
    }

    //open all activ lvl in game
    private void BoostLevelToTester()
    {
        DataManager data = new DataManager();
        data.SaveData(3);//current lvl 3
    }

    #region Buttons Actions

    private void SetCurrentMenu(GameObject input_menu)
    {
        current_menu.SetActive(false);//close main menu
        panel.SetActive(true);
        current_menu = input_menu;//set new menu
    }

    private void CloseCurrentMenu()
    {
        current_menu.SetActive(false);
        panel.SetActive(false);
        menu.SetActive(true);
        current_menu = menu;
    }

    private void OpenInfo()
    {
        info.SetActive(true);
        StartCoroutine(InfoTextWork());
    }

    private void OpenSettings()
    {
        settings.SetActive(true);
    }

    private void OpenLevelPanel()
    {
        SetupLevelPanel();
        level.SetActive(true);
    }

    private void LoadLevel(int index)
    {
        GameAnalytics.init.EventStartLvlInMainMenu(index);//chose player in start app
        UnityEngine.SceneManagement.SceneManager.LoadScene(index);
    }

    private void SwithInputType(Transform icon)
    {
        icon.GetChild(PlayerPrefs.GetInt("Input")).gameObject.SetActive(false);
        if (PlayerPrefs.GetInt("Input") == 0)
            PlayerPrefs.SetInt("Input", 1);//to mobile
        else
            PlayerPrefs.SetInt("Input", 0);//to PC
        icon.GetChild(PlayerPrefs.GetInt("Input")).gameObject.SetActive(true);
    }

    private void ChangeMusic(Slider c_slider)
    {
        PlayerPrefs.SetFloat("Music", c_slider.value);
        MusicManager.init.SetMusicValue();
    }

    private void ChangeSound(Slider c_slider)
    {
        PlayerPrefs.SetFloat("Sound", c_slider.value);
        MusicManager.init.SetSoundValue();
    }

    #endregion

    IEnumerator InfoTextWork()
    {
        Transform text = info.transform.Find("Mask/Text");
        text.localPosition = Vector3.down * 250f;
        while (info.activeSelf)
        {
            yield return new WaitForEndOfFrame();
            if(text.localPosition.y < 2260f)
                text.localPosition += Vector3.up * 150f * Time.deltaTime;

        }
        text.localPosition = Vector3.zero;
    }
}
