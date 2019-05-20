using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
    public static UIController init;

    public Sprite red_door_open;
    private GameObject mobile_input;
    private GameObject dead_panel;
    private GameObject win_panel;
    private Image hit_screen;
    private Image ammo_image;
    private Image player_health;
    [SerializeField]
    private Image key;
    [SerializeField]
    private Text coin_value;

    //boss ui value
    [SerializeField]
    private Transform boss_hp;
    [SerializeField]
    private Slider boss_timer;
    private int current_boss_hp;

    private bool game_run;
    private bool b_swith;//mobile fire or use


    private void Awake()
    {
        if (init == null)
            init = this;
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        game_run = true;
        //main buttons
        {
            //reset
            transform.Find("Reset").GetComponent<Button>().onClick.AddListener(() => UIcmd(6));
            //main menu
            transform.Find("Menu").GetComponent<Button>().onClick.AddListener(() => UIcmd(7));
        }
        
        dead_panel = transform.Find("Dead").gameObject;
        dead_panel.GetComponentInChildren<Button>().onClick.AddListener(() => UIcmd(6));
        dead_panel.SetActive(false);

        hit_screen = transform.Find("Hit").GetComponent<Image>();
        hit_screen.enabled = false;
        
        ammo_image = transform.Find("PlayerInfo/Ammo").GetComponent<Image>();
        player_health = transform.Find("PlayerInfo/Health").GetComponent<Image>();
        key = transform.Find("PlayerInfo/Key").GetComponent<Image>();
        key.enabled = false;
        coin_value = transform.Find("PlayerInfo/CoinPanel").GetComponentInChildren<Text>();
        coin_value.text = " - 0";
        UIcmd(9);

        mobile_input = transform.Find("MobileInput").gameObject;
        mobile_input.SetActive(false);

        //boss value panel
        GameObject boss_panel = transform.Find("BossPanel").gameObject;
        boss_hp = boss_panel.transform.GetChild(0);
        boss_timer = boss_panel.GetComponentInChildren<Slider>();
        boss_panel.SetActive(false);

        //if android
        if(Application.platform == RuntimePlatform.Android || PlayerPrefs.GetInt("Input") == 1)
            SetupMobileInput();

        StartCoroutine(PlayerHit());
    }

    private void SetupMobileInput()
    {
        mobile_input.SetActive(true);

        PlayerCharacter player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();
        //left
        EventTrigger trigger = mobile_input.transform.Find("Left").GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        {
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(delegate { player.MobileSide(-1); });//down button
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener(delegate { player.MobileSide(0); });//up button
            trigger.triggers.Add(entry);
        }
        //right
        trigger = mobile_input.transform.Find("Right").GetComponent<EventTrigger>();
        {
            trigger.triggers.Add(entry);//up button
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(delegate { player.MobileSide(1); });//down button
            trigger.triggers.Add(entry);
        }
        //jump
        trigger = mobile_input.transform.Find("Jump").GetComponent<EventTrigger>();
        {
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(delegate { player.MobileJump(); });//jump
            trigger.triggers.Add(entry);
        }
        //fire xor use - prime
        trigger = mobile_input.transform.Find("Fire").GetComponent<EventTrigger>();
        {
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(delegate { player.MobileFire(); });//fire
            trigger.triggers.Add(entry);
        }
        //use xor fire
        trigger = mobile_input.transform.Find("Use").GetComponent<EventTrigger>();
        {
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(delegate { player.MobileUse(); });
            trigger.triggers.Add(entry);
        }
        trigger.gameObject.SetActive(false);
    }
    

    private void StartGame()
    {
        Time.timeScale = 1f;
        Screen.fullScreen = true;
    }

    public void SwithButtons()
    {
        mobile_input.transform.Find("Fire").gameObject.SetActive(b_swith);
        b_swith = !b_swith;
        mobile_input.transform.Find("Use").gameObject.SetActive(b_swith);
    }

    public void UIcmd(int id_cmd)
    {
        switch (id_cmd)
        {
            case (1)://shoot
                MusicManager.init.PlaySoundClip(0);//fire
                if (ammo_image.fillAmount > 0)
                    ammo_image.fillAmount -= 0.2f;
                
                break;
            case (2)://reload ammo
                MusicManager.init.PlaySoundClip(1);//reload
                ammo_image.fillAmount += 0.2f;
                
                break;
            case (3)://key
                MusicManager.init.PlaySoundClip(3);//bonus
                key.enabled = !key.enabled;
                
                break;
            case (4)://damage
                if (player_health.fillAmount > 0.1f)
                {
                    MusicManager.init.PlaySoundClip(4);//damage
                    player_health.fillAmount -= 0.33f;
                    if (player_health.fillAmount > 0.1f)
                        goto case (999);
                }
                else
                {
                    dead_panel.SetActive(true);
                    Time.timeScale = 0f;
                }
                break;

            case (5)://healing
                if (player_health.fillAmount < 1f)
                {
                    MusicManager.init.PlaySoundClip(3);//bonus
                    player_health.fillAmount += 0.33f;
                }
                else
                    player_health.fillAmount = 1f;
                break;
            case (6)://reload lvl
                //analytics
                GameAnalytics.init.CMDUnityAnalytics(3);//reload ++ dead
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case (7)://back main menu
                //analytics
                Time.timeScale = 1f;
                SceneManager.LoadScene(0);
                break;
            case (8)://not use
                break;
            case (9)://text coin
                coin_value.text = string.Format(" - {0}", GameController.init.GetPlayerCoins());
                break;
            case (10):// - boss HP
                LoseHP();
                break;
            case (11):// + boss HP
                PlusHP();
                break;
            case (12):// on/off timer boss
                TimerActive();
                break;
            case (13):// timer work boss
                TimeSliderWork();
                break;
            case (999)://hit screen
                hit_screen.enabled = true;
                break;
            default://coin plus
                MusicManager.init.PlaySoundClip(3);//bonus
                GameController.init.AddPlayerCoin();
                goto case (9);
        }
    }
    #region BOOS UI

    public void SetupBossPanel(int health, float time)
    {
        current_boss_hp = health;
        boss_hp.parent.gameObject.SetActive(true);
        //hp
        GameObject hp_pref = boss_hp.transform.GetChild(0).gameObject;
        while (health > 0)
        {
            Instantiate(hp_pref, boss_hp);
            --health;
        }
        //timer
        boss_timer.value = 0f;
        boss_timer.maxValue = time;
        boss_timer.gameObject.SetActive(false);
    }

    public void CloseBossUI()
    {
        boss_hp.parent.gameObject.SetActive(false);
    }

    private void LoseHP()
    {
        boss_hp.GetChild(current_boss_hp).GetChild(0).gameObject.SetActive(false);
        current_boss_hp--;
    }

    private void PlusHP()
    {
        current_boss_hp++;
        boss_hp.GetChild(current_boss_hp).GetChild(0).gameObject.SetActive(true);
        
    }

    private void TimerActive()
    {
        boss_timer.value = 0f;
        boss_timer.gameObject.SetActive(!boss_timer.gameObject.activeSelf);
    }

    private void TimeSliderWork()
    {
        boss_timer.value += Time.deltaTime;
    }

    #endregion

    //Test Door
    public void OpenDoor(SpriteRenderer door_sprite, int id_door)
    {
        switch (id_door)
        {
            default://red door
                if (key.enabled)
                {
                    print(door_sprite);
                    door_sprite.sprite = red_door_open;
                    UIcmd(3);
                }
                break;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameController.init.current_lvl_id + 1);
        GameAnalytics.init.CMDUnityAnalytics(2);// - end lvl
        GameAnalytics.init.CMDUnityAnalytics(1);// - analytics new lvl
    }

    IEnumerator PlayerHit()
    {
        while (game_run)
        {
            if (hit_screen.enabled)
            {
                if (hit_screen.color.a < 0.5f)
                    hit_screen.color = Color.Lerp(hit_screen.color, Color.white, 2.5f * Time.deltaTime);
                else
                {
                    hit_screen.color *= Color.clear;
                    hit_screen.enabled = false;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
