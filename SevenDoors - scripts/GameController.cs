using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour
{
    public static GameController init;

    public GameObject shotgun_hit_FX;
    public GameObject box_crush_FX;
    public GameObject blood_FX;
    public GameObject meat_sprite;
    public GameObject demon_projectile;

    private Transform player_pool;
    private Transform box_pool;
    private Transform blood_pool;
    [SerializeField]
    private Transform demon_prj_pool;
    [SerializeField]
    private List<ParticleSystem> fx_in_scene;
    [SerializeField]
    private List<Rigidbody2D> prj_in_scene;

    [SerializeField]
    private Vector3 lvl_PSP;
    [SerializeField]
    private float prj_life_time;
    [SerializeField]
    private uint player_coins;//все монетки игрока
    public int current_lvl_id;


    public bool test_end;//test

    private void Awake()
    {
        if (init is null)
            init = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupGameController();
    }

    private void LateUpdate()
    {
        if (fx_in_scene.Count > 0)
        {
            for (int i = 0; i < fx_in_scene.Count; ++i)
            {
                if (fx_in_scene != null && !fx_in_scene[i].isPlaying)
                {
                    ReturnObjInPool(fx_in_scene[i].gameObject);
                    fx_in_scene.Remove(fx_in_scene[i]);
                }
            }
        }

        ProjectileController();
    }

    private void OnLevelWasLoaded(int level)
    {
        current_lvl_id = level;
        if (level != 0)
            Invoke("LateCanvasSetup", 0.5f);//not UI & point in load scene
        else
            player_coins = 0;
    }
    private void LateCanvasSetup()
    {
        lvl_PSP = GameObject.Find("PlayerStartPosition").transform.position;
        UIController.init.UIcmd(9);//lvl refresh player coins
    }

    private void SetupGameController()
    {
        fx_in_scene = new List<ParticleSystem>();
        prj_in_scene = new List<Rigidbody2D>();

        player_pool = transform.Find("PlayerFX");
        box_pool = transform.Find("BoxFX");
        blood_pool = transform.Find("BloodFX");

        demon_prj_pool = transform.Find("DemonProjectile");
        lvl_PSP = GameObject.Find("PlayerStartPosition").transform.position;
        

    }

    public Vector3 GetStartPosition()
    {
        return lvl_PSP;
    }

    public uint GetPlayerCoins()
    {
        return player_coins;
    }
    public void AddPlayerCoin()
    {
        player_coins++;
    }

    #region Objects Pool
    public GameObject GetObjectInPool(int id)
    {
        GameObject pool_obj = null;
        switch (id)
        {
            case (1)://player hit particle system
                if (player_pool.childCount != 0)
                {
                    pool_obj = player_pool.GetChild(0).gameObject;
                    goto case (999);
                }
                else
                    goto default;
                break;
            case (2)://box crush
                if (box_pool.childCount != 0)
                {
                    pool_obj = box_pool.GetChild(0).gameObject;
                    goto case (999);
                }
                else
                    goto default;
                break;
            case (3):
                if (blood_pool.childCount != 0)
                {
                    pool_obj = blood_pool.GetChild(0).gameObject;
                    goto case (999);
                }
                else
                    goto default;
                break;
            case (11)://demon projectile
                if (demon_prj_pool.childCount != 0)
                {
                    pool_obj = demon_prj_pool.GetChild(0).gameObject;
                    goto case (999);
                }
                else
                    goto default;
                break;
            case (999)://null parent
                pool_obj.transform.SetParent(null);
                break;
            default://instat new object
                pool_obj = InitNewPoolObject(id);
                break;
        }

        pool_obj.SetActive(true);//???
        if (pool_obj.GetComponent<ParticleSystem>())
            fx_in_scene.Add(pool_obj.GetComponent<ParticleSystem>());
        if (pool_obj.GetComponent<Rigidbody2D>())
            prj_in_scene.Add(pool_obj.GetComponent<Rigidbody2D>());

        return pool_obj;
    }

    private GameObject InitNewPoolObject(int id_init)
    {
        GameObject new_obj = null;
        GameObject current_prefabs = null;
        switch (id_init)
        {
            case (1)://new shotgun PS
                current_prefabs = shotgun_hit_FX;
                break;
            case (2)://box FX
                current_prefabs = box_crush_FX;
                break;
            case (3)://blood
                current_prefabs = blood_FX;
                break;
            case (11)://demon projectile
                current_prefabs = demon_projectile;
                break;
            default:
                break;
        }

        new_obj = Instantiate(current_prefabs, Vector3.one * 999f, Quaternion.identity);
        new_obj.name = id_init.ToString();

        return new_obj;
    }

    private void ReturnObjInPool(GameObject current_obj)
    {
        int id_name = System.Convert.ToInt32(current_obj.name);
        Transform current_pool = null;
        //print(id_name);

        switch (id_name)
        {
            case (1)://shotgun FX
                current_pool = player_pool;
                break;
            case (2):
                current_pool = box_pool;
                break;
            case (3):
                current_pool = blood_pool;
                break;
            case (11):
                current_pool = demon_prj_pool;
                break;
            default:
                break;
        }

        current_obj.SetActive(false);
        current_obj.transform.SetParent(current_pool);
        current_obj.transform.localPosition = Vector3.zero;
    }
    #endregion
    
    private void ProjectileController()
    {
        if (prj_in_scene.Count > 0 && prj_life_time == 0f)
            prj_life_time = Time.time + 2.5f;
        if (Time.time >= prj_life_time && prj_in_scene.Count != 0)
        {
            prj_in_scene[0].velocity = Vector2.zero;

            ReturnObjInPool(prj_in_scene[0].gameObject);
            prj_in_scene.Remove(prj_in_scene[0]);
            prj_life_time = Time.time + 2.5f;
        }

        if (prj_in_scene.Count == 0)
            prj_life_time = 0f;
    }
}
