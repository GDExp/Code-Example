using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//управление игрока - юниты + камера
//работа с UI проекта
public class PlayerController : MonoBehaviour
{
    private GameController game_controller;
    private Transform main_camera;
    private Image unit_avatar;
    private Image aid_block;
    private Image seller;

    //UI - элементы управления игрока
    private GameObject player_menu;
    private GameObject selector_ui;
    private GameObject unit_menu;
    private EarthUnit selected_unit;
    private PointInfo current_point;

    //UI - элементы информации 
    private Animator coming;
    [SerializeField]
    private Transform statistic;
    private Text info_base;
    private Text info_lp;
    private Text info_wave;
    private Text info_clock;
    
    //заготовка UI - элемента точки
    public GameObject points_ui;
    public GameObject[] units_ui;
    public Sprite[] avatar_images;
    
    private int current_button_index;
    [SerializeField]
    private short kill_mars;
    private short player_lp;
    public short[] player_unit_cost;
    //camera
    private sbyte camera_vector;
    private byte id_vector;
    private bool useTouch;

    private void Reset()
    {
        tag = "Player";
    }

    private void Start()
    {
        main_camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        player_menu = transform.Find("Player Menu").gameObject;

        game_controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        selector_ui = GameObject.FindGameObjectWithTag("Scene Canvas").transform.Find("Selector").gameObject;//+++

        //настройка
        player_lp = 130;
        SetupPlayerMenu();
        SetupSceneGameMenu();
    }

    private void LateUpdate()
    {
        //управление камерой
        {
            if (useTouch)
            {
                if (id_vector > 0)
                    CameraMove(ref camera_vector, ref id_vector);
            }
            else
            {
                if (Input.GetAxis("Horizontal") != 0)
                {
                    id_vector = 1;
                    if (Input.GetAxis("Horizontal") > 0) camera_vector = -1;
                    if (Input.GetAxis("Horizontal") < 0) camera_vector = 1;
                    CameraMove(ref camera_vector, ref id_vector);
                }
                if (Input.GetAxis("Vertical") != 0)
                {
                    id_vector = 2;
                    if (Input.GetAxis("Vertical") > 0) camera_vector = -1;
                    if (Input.GetAxis("Vertical") < 0) camera_vector = 1;
                    CameraMove(ref camera_vector, ref id_vector);
                }

            }
        }
            if (selected_unit != null)
                AidBlocking();
        
    }
    //настройка меню выбора юнита игрока
    private void SetupPlayerMenu()
    {
        EventTrigger.Entry entry;
        for (int i =0; i< player_menu.transform.childCount; ++i)
        {
            int index = i + 1;
            Transform button = player_menu.transform.GetChild(i);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener(delegate { SelectButton(button.GetComponent<Image>(), index); });
            button.GetComponent<EventTrigger>().triggers.Add(entry);

            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entry.callback.AddListener(delegate { DeselectButton(button.GetComponent<Image>()); });
            button.GetComponent<EventTrigger>().triggers.Add(entry);
        }
        
    }
    //выделение выбранной кнопки в меню игрока
    public void SelectButton(Image s_image, int index)
    {
        s_image.color = Color.gray;
        s_image.transform.GetChild(0).gameObject.SetActive(true);
        s_image.transform.GetChild(0).GetComponentInChildren<Text>().text = "- " + player_unit_cost[index - 1];
        s_image.transform.GetChild(0).GetComponentInChildren<Text>().color = Color.red;
        current_button_index = index;
    }
    //снятие выделения с кнопки
    public void DeselectButton(Image s_image)
    {
        s_image.color = Color.white;
        s_image.transform.GetChild(0).gameObject.SetActive(false);
        current_button_index = 0;
    }
    //получение ссылки на UI области
    public Transform ZoneUIReference(string name)
    {
        return GameObject.FindGameObjectWithTag("Scene Canvas").transform.Find("Zones/" + name);
    }
    //создание и настройка EventTrigger для точек
    public void SetupPointUI(PointInfo point)
    {
        Transform point_ui_pool = GameObject.FindGameObjectWithTag("Scene Canvas").transform.Find("Points UI");
        GameObject _point = Instantiate(points_ui, point.point_position, Quaternion.Euler(90f, 0f, 0f), point_ui_pool);
        point.SetPointImage(_point.transform);//+++

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener(delegate { PlayerPointDown(_point.GetComponent<Image>(), point); });
        _point.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entry.callback.AddListener(delegate { PlayerPointUp(_point.GetComponent<Image>()); });
        _point.GetComponent<EventTrigger>().triggers.Add(entry);
    }
    //событие точки - PointerDown
    public void PlayerPointDown(Image point_image, PointInfo s_point)
    {
        if (s_point.GetPointStatus()) return;
        if(selected_unit != null)
        {
            selected_unit.PlayerChosePoint(s_point, point_image.transform);
            PlayerDeselectUnit();
            return;
        }
        current_point = s_point;
        point_image.color = Color.white;//непрозрачный яркий
        player_menu.transform.position = main_camera.GetComponent<Camera>().WorldToScreenPoint(point_image.transform.position);
        player_menu.SetActive(!player_menu.activeSelf);
    }
    //событие точки - PointerUp
    public void PlayerPointUp(Image point_image)
    {
        if (current_point == null) return;
        point_image.color = new Color(1f, 1f, 1f, 0.4f);//прозрачный тусклый
        //+++
        if (selected_unit == null && !current_point.GetPointStatus())
        {
            player_menu.SetActive(!player_menu.activeSelf);
            if (current_button_index > 0 && CheckPlayerLP(player_unit_cost[current_button_index - 1]))
            {
                selected_unit = game_controller.GetUnitInPool((byte)(current_button_index - 1), true, Vector3.zero) as EarthUnit;//временно - отключаем др. кнопки!
                DeselectButton(player_menu.transform.GetChild(current_button_index - 1).GetComponent<Image>());
                selected_unit.PlayerChosePoint(current_point, point_image.transform);
                player_lp -= player_unit_cost[selected_unit.unit_type - 1];//+++
                RefreshGameInterface(2, 0);//+++
            }
        }
        selected_unit = null;
        current_point = null;
    }
    //отметка занятых точек
    public void PointIsBooked(UnitMechanics unit, Transform point)
    {
        if (unit == null)
            point.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.4f);
        else
            point.GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.4f);
    }
    //получение ui для юнита
    public Transform GetUnitUI(UnitMechanics unit)
    {
        Transform ui;
        if(unit is EarthUnit)
        {
            ui = Instantiate(units_ui[0], unit.transform.position, Quaternion.Euler(90f, 0f, 0f), 
                GameObject.FindGameObjectWithTag("Scene Canvas").transform.Find("Units UI")).transform;

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener(delegate { PlayerSelectUnit(unit); });
            ui.GetComponent<EventTrigger>().triggers.Add(entry);
        }
        else
        {
            //пока назначение UI для юнита
            ui = Instantiate(units_ui[1], unit.transform.position, Quaternion.Euler(90f, 0f, 0f), 
                GameObject.FindGameObjectWithTag("Scene Canvas").transform.Find("Units UI")).transform;
        }
        ui.position = new Vector3(transform.position.x, 0.2f, transform.position.z);

        SetupUnitUI(ui);
        return ui;
    }
    //настройка UI элемента юнитов
    private void SetupUnitUI(Transform _ui)
    {
        Image current_image = _ui.Find("Health").GetComponent<Image>();
        current_image.fillAmount = 1f;
        current_image.gameObject.SetActive(false);

        if(_ui.childCount > 1)
        {
            //настройка полосы скила у юнитов игрока
            current_image = _ui.Find("Skill").GetComponent<Image>();
            current_image.fillAmount = 0f;
        }
    }
    //обновление UI юнита
    public void RefreshUnitUI(Transform ui, float value, int id)
    {
        Image current_image;
        switch (id)
        {
            case (1)://skill
                break;
            case (9)://reset
                ui.transform.localPosition = Vector3.zero;
                current_image = ui.Find("Health").GetComponent<Image>();
                current_image.fillAmount = value;
                current_image.gameObject.SetActive(false);

                if (ui.childCount > 1)
                {
                    current_image = ui.Find("Skill").GetComponent<Image>();
                    current_image.fillAmount = 0f;
                }
                break;
            default://health
                current_image = ui.Find("Health").GetComponent<Image>();
                current_image.fillAmount = value;
                break;
        }
    }
    //выбор юнита при нажатие на UI элемент - событие PointerDown
    public void PlayerSelectUnit(UnitMechanics unit)//+++
    {
        if (unit.UnitIsMoving()) return;
        if (selected_unit != null)
            PlayerDeselectUnit();
        selected_unit = unit as EarthUnit;
        selector_ui.transform.position = unit.transform.localPosition + Vector3.up * 1f;
        //selector_ui.transform.localPosition += Vector3.up * 30f;//смещение по оси Y в зависимости от камеры
        selector_ui.SetActive(true);
        ChangeAvatarImage(unit.unit_type);
    }
    //сброс выбора юнита
    private void PlayerDeselectUnit()//+++
    {
        selected_unit = null;
        selector_ui.SetActive(false);
        ChangeAvatarImage(0);
    }
    //+++
    public void CheckSelectedUnit(UnitMechanics unit)
    {
        if (selected_unit == null) return;
        if (selected_unit == unit)
            PlayerDeselectUnit();
    }

    //настройка игрового меню сцены
    private void SetupSceneGameMenu()//+++
    {
        Transform cur_menu = transform.Find("Select Unit");
        unit_avatar = cur_menu.Find("Radio/Avatar").GetComponent<Image>();
        unit_menu = cur_menu.Find("Buttons").gameObject;
        statistic = transform.Find("Game Over/Statistic");
        //настройка кнопок
        {
            unit_menu.transform.Find("Cancel").GetComponent<Button>().onClick.AddListener(delegate { PlayerDeselectUnit(); });
            unit_menu.transform.Find("Aid").GetComponent<Button>().onClick.AddListener(delegate { FirstAid(); });
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener(delegate { SeeInfoCost(unit_menu.transform.Find("Aid/Panel").gameObject, -15); });
            unit_menu.transform.Find("Aid").GetComponent<EventTrigger>().triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entry.callback.AddListener(delegate { SeeInfoCost(unit_menu.transform.Find("Aid/Panel").gameObject, 0); });
            unit_menu.transform.Find("Aid").GetComponent<EventTrigger>().triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener(delegate { SeeInfoCost(unit_menu.transform.Find("Aid/Panel").gameObject, 0); });
            unit_menu.transform.Find("Aid").GetComponent<EventTrigger>().triggers.Add(entry);
            aid_block = unit_menu.transform.Find("Aid/Reload").GetComponent<Image>();


            unit_menu.transform.Find("Sell").GetComponent<Button>().onClick.AddListener(delegate { Sell(); });
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener(delegate { SeeInfoCost(unit_menu.transform.Find("Sell/Panel").gameObject, (short)(player_unit_cost[selected_unit.unit_type - 1]/2)); });
            unit_menu.transform.Find("Sell").GetComponent<EventTrigger>().triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entry.callback.AddListener(delegate { SeeInfoCost(unit_menu.transform.Find("Sell/Panel").gameObject, 0); });
            unit_menu.transform.Find("Sell").GetComponent<EventTrigger>().triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener(delegate { SeeInfoCost(unit_menu.transform.Find("Sell/Panel").gameObject, 0); });
            unit_menu.transform.Find("Sell").GetComponent<EventTrigger>().triggers.Add(entry);
        }
        unit_menu.SetActive(false);

        //настройка информации
        {
            cur_menu = transform.Find("Game Interface");
            info_base = cur_menu.Find("Base Health").GetComponentInChildren<Text>();
            info_lp = cur_menu.Find("LP").GetComponentInChildren<Text>();
            info_wave = cur_menu.Find("Waves").GetComponentInChildren<Text>();
            info_clock = cur_menu.Find("Clock").GetComponentInChildren<Text>();

            RefreshGameInterface(0, 0);//нач. настройка
        }

        //настройка движения камеры
        {
            string[] b_name = { "Up", "Down", "Right", "Left" };
            cur_menu = transform.Find("CameraUI/Buttons");
            EventTrigger x_button;
            for (int i = 0; i < b_name.Length; ++i)
            {
                sbyte stage;
                byte id;
                x_button = cur_menu.Find(b_name[i]).GetComponent<EventTrigger>();
                switch (i)
                {
                    case (1)://Down
                        stage = 1;
                        id = 2;
                        break;
                    case (2)://Right
                        stage = 1;
                        id = 1;
                        break;
                    case (3)://Left
                        stage = -1;
                        id = 1;
                        break;
                    default://Up
                        stage = -1;
                        id = 2;
                        break;
                }

                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown
                };
                entry.callback.AddListener(delegate { CameraButtonDown(stage, id); });
                x_button.triggers.Add(entry);
                entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerUp
                };
                entry.callback.AddListener(delegate { CameraButtonReset(); });
                x_button.triggers.Add(entry);
            }
        }

        coming = transform.Find("Text Coming").GetComponent<Animator>();
    }
    
    private void ChangeAvatarImage(int image_index)
    {
        bool activ = (image_index > 0) ? true : false;
        unit_menu.SetActive(activ);
        unit_avatar.sprite = avatar_images[image_index];
    }

    private void FirstAid()
    {
        if (selected_unit == null && CheckPlayerLP(15)) return;
        player_lp -= 15;
        RefreshGameInterface(2, 0);
        selected_unit.UnitHealing(50);
        selected_unit.SetAidReloadTime();
        aid_block.GetComponentInParent<Button>().interactable = false;
        aid_block.gameObject.SetActive(true);
        PlayerDeselectUnit();
    }

    private void AidBlocking()
    {
        float c_time = selected_unit.GetAidReloadTime();
        if (c_time >= Time.time)
            aid_block.GetComponentInChildren<Text>().text = (c_time - Time.time).ToString("0");
        else
        {
            aid_block.gameObject.SetActive(false);
            aid_block.GetComponentInParent<Button>().interactable = true;
         }
    }

    private void Sell()
    {
        //При продаже не проигрывается анимация!!!
        if (selected_unit == null) return;
        RefreshGameInterface(2, player_unit_cost[selected_unit.unit_type - 1] / 2);
        selected_unit.UnitDead();
        PlayerDeselectUnit();
    }
    //+++
    private void SeeInfoCost(GameObject panel, short cost)
    {
        if (cost == 0)
            panel.SetActive(false);
        else
        {
            panel.SetActive(true);
            panel.GetComponentInChildren<Text>().text = cost.ToString();
            panel.GetComponentInChildren<Text>().color = (cost > 0) ? Color.green : Color.red;
        }
    }
    //+++
    private bool CheckPlayerLP(short cost)
    {
        bool iCanBuy = (cost <= player_lp) ? true : false;
        return iCanBuy;
    }
    //+++
    public void RefreshZoneFlag(Transform zone, float value)
    {
        if (zone.childCount != 0)
        {
            Image flag = zone.GetChild(0).GetComponent<Image>();
            flag.fillAmount = value;
            flag = flag.transform.GetChild(0).GetComponent<Image>();
            flag.fillAmount = 1 - value;
        }
        else
            RefreshGameInterface(1, 100 * value);
        if (value == 0)
            zone.GetComponent<Image>().color = Color.red;
    }
    //+++
    public void RefreshGameInterface(int id, float value)
    {
        switch (id)
        {
            case (1)://base - hp
                if (value <= 25)
                    info_base.color = Color.red;
                else
                    info_base.color = Color.white;
                if (value <= 0)
                    GameOver();
                info_base.text = value.ToString("0");
                break;
            case (2):// LP
                if (value > 0 && player_lp < 999)
                    player_lp += (short)value;
                info_lp.text = player_lp.ToString("0");
                break;
            case (3):// waves
                info_wave.text = "WAVE - " + value;
                break;
            case (4):// clock
                if (value > 0)
                    info_clock.transform.parent.gameObject.SetActive(true);
                else
                {
                    info_clock.transform.parent.gameObject.SetActive(false);
                    info_clock.color = Color.white;
                }
                float min = 0;
                float sec = 0;
                sec = value % 60;
                min = (value - sec) / 60;
                info_clock.text = min.ToString("0") + "." + sec.ToString("0");
                if (value <= 10) info_clock.color = Color.red;
                break;
            default://reset
                info_base.text = 100.ToString();//от сложности - тест
                info_lp.text = player_lp.ToString("0");//от сложности - тест
                info_wave.text = "WAVE - " + 0;
                info_clock.text = "0.00";
                break;
        }
    }
    //+++
    public void ActiveText()
    {
        coming.SetTrigger("Active");
    }
    //+++
    public void KillingMarsUnit()
    {
        kill_mars++;
    }
    //+++
    private void GameOver()
    {
        statistic.parent.gameObject.SetActive(true);
        Time.timeScale = 0f;

        float min = 0;
        float sec = 0;
        sec = Time.time % 60;
        min = (Time.time - sec) / 60;
        statistic.Find("Clock").GetComponentInChildren<Text>().text = min.ToString("0") + "." + sec.ToString("0");

        statistic.Find("Mars").GetComponentInChildren<Text>().text = kill_mars.ToString();
    }
    //+++
    private void CameraButtonDown(sbyte move_stage, byte id)
    {
        camera_vector = move_stage;
        id_vector = id;
        useTouch = true;
    }
    //+++
    private void CameraButtonReset()
    {
        camera_vector = 0;
        id_vector = 0;
        useTouch = false;
    }
    //+++
    private void CameraMove(ref sbyte vector, ref byte id)
    {
        //Х - движение лево-право, предел право = -115, предел лево = 100 -- 1
        //Z - движение верх-вниз, предел верх = -30, предел вниз = 280 --2
        Vector3 move = Vector3.zero;
        if (id < 2)
        {
            move = Vector3.right * vector * 100f * Time.deltaTime;
            if ((main_camera.localPosition + move).x < -115 || (main_camera.localPosition + move).x > 100) return;
        }
        else
        {
            move = Vector3.forward * vector * 100f * Time.deltaTime;
            if ((main_camera.localPosition + move).z < -30 || (main_camera.localPosition + move).z > 280) return;
        }

        main_camera.localPosition += move;
    }
}