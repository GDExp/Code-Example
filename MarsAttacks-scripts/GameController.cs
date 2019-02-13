using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//управление игрой и создание путей для любого юнита через NavMesh.CalculatePath
//система - poolobjects
public class GameController : MonoBehaviour
{
    private PlayerController player;
    private Transform spawn_zones;
    private Transform[] ai_ways;
    private Transform pool_bullet;
    private Transform pool_mars_units;
    private Transform pool_earth_units;

    //Заготовки
    public Rigidbody bullet;
    public Rigidbody[] mars_units;
    public Rigidbody[] earth_units;

    //wave test
    [SerializeField]
    private byte wave_numb;
    [SerializeField]
    private byte wave_count;
    [SerializeField]
    private byte all_mars;
    [SerializeField]
    private float timer;
    [SerializeField]
    private float random_mod;
    private bool wave_coming;

    private void Reset()
    {
        tag = "GameController";
    }

    private void Awake()
    {
        //тест
        timer = Time.time + 35f;
        random_mod = 0.65f;
        wave_count = 8;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        spawn_zones = GameObject.Find("Spawn Zones").transform;

        pool_bullet = transform.Find("Bullets");
        pool_mars_units = transform.Find("MarsUnits");
        pool_earth_units = transform.Find("EarthUnits");

        Transform ways = GameObject.Find("Ways").transform;
        ai_ways = new Transform[ways.childCount];
        for (int i = 0; i < ai_ways.Length; ++i)
            ai_ways[i] = ways.GetChild(i);
    }
    

    private void LateUpdate()
    {
        if (!wave_coming)
        {
            if(timer >= Time.time)
                player.RefreshGameInterface(4, timer - Time.time);
            else
                StartCoroutine(WaveMaker(2.5f));
        }
            
    }

    //формирование пути для юнита - ии
    public void GetUnitWay(Vector3 unit_position, ref List<Vector3> way, byte id_way)
    {
        if (way.Count == 0)
        {
            NavMeshPath _path = new NavMeshPath();
            for (int i = 0; i < ai_ways[id_way].childCount; ++i)
            {
                if (i == 0)
                    NavMesh.CalculatePath(unit_position, ai_ways[id_way].GetChild(i).localPosition, NavMesh.AllAreas, _path);
                else
                    NavMesh.CalculatePath(ai_ways[id_way].GetChild(i - 1).localPosition, ai_ways[id_way].GetChild(i).localPosition, NavMesh.AllAreas, _path);
                way.AddRange(_path.corners);
            }
        }
    }
    //аналогичный метод создания пути, для юнита игрока
    public void GetUnitWay(EarthUnit human, ref List<Vector3> way, Vector3 point_position)
    {
        if (way.Count == 0)
        {
            NavMeshPath _path = new NavMeshPath();
            NavMesh.CalculatePath(human.transform.localPosition, point_position, NavMesh.AllAreas, _path);
            way.AddRange(_path.corners);
        }
    }
    //работа с пулом снарядов
    public Rigidbody GetBulletInPool(Vector3 spawn_point)
    {
        Rigidbody n_bullet = null;
        if (pool_bullet.childCount > 0)
        {
            n_bullet = pool_bullet.GetChild(0).GetComponent<Rigidbody>();
            n_bullet.transform.SetParent(null);
            n_bullet.transform.position = spawn_point;
        }
        else
            n_bullet = Instantiate(bullet, spawn_point, Quaternion.identity);
        n_bullet.GetComponent<TrailRenderer>().enabled = true;
        return n_bullet;
    }
    //получение юнита из пула при наличие
    public UnitMechanics GetUnitInPool(byte id, bool human, Vector3 spawn_position)
    {
        //только для игрока!
        if (spawn_position == Vector3.zero)
            spawn_position = spawn_zones.Find("Earth").GetChild(0).localPosition;

        Transform pool = (human) ? pool_earth_units : pool_mars_units;
        UnitMechanics unit;
        if (pool.GetChild(id).childCount > 0)
        {
            unit = pool.GetChild(id).GetChild(0).GetComponent<UnitMechanics>();
            unit.transform.localPosition = spawn_position;
            unit.transform.SetParent(null);
            unit.gameObject.SetActive(true);
        }
        else
        {
            Rigidbody[] prefab_units = (human) ? earth_units : mars_units;
            unit = Instantiate(prefab_units[id], spawn_position, Quaternion.identity).GetComponent<UnitMechanics>();
        }
        return unit;
    }
    //возврат в пул объекта
    public void ReturnObjectInPool(Rigidbody obj, byte code_pool)
    {
        //стд. действия со всеми объектами
        obj.velocity = Vector3.zero;
        obj.angularVelocity = Vector3.zero;
        obj.position = Vector3.zero;
        obj.gameObject.SetActive(false);

        if (obj.CompareTag("Bullet"))
        {
            obj.transform.SetParent(pool_bullet);
            obj.GetComponent<TrailRenderer>().enabled = false;
        }
        else
        {
            UnitMechanics current_unit = obj.GetComponent<UnitMechanics>();
            Transform current_pool = (current_unit is MarsUnit) ? pool_mars_units : pool_earth_units;
            switch (code_pool)
            {
                //!!!Пока реализуется только 1-ый пул!!!
                case (1)://пул солдатов
                    current_unit.transform.SetParent(current_pool.GetChild(code_pool - 1));
                    break;
                case (2)://пул офицеров
                    break;
                case (3)://пул лег. тех.
                    break;
                case (4)://пул танков
                    break;
                case (5)://пул пво
                    break;
                default:
                    break;
            }
            obj.useGravity = true;
        }
    }
    //ссылка на игрока
    public PlayerController PlayerReference()
    {
        return player;
    }
    //обновление счетчика марсиан
    public void MarsUnitGo(bool dead)//+++
    {
        all_mars = (dead) ? --all_mars : ++all_mars;
        if (all_mars == 0)
        {
            timer = Time.time + 30f;
            wave_coming = false;
        }
    }
    //создание волны 
    IEnumerator WaveMaker(float timer)
    {
        wave_numb++;
        player.RefreshGameInterface(3, wave_numb);
        player.RefreshGameInterface(4, 0);
        player.ActiveText();
        wave_coming = true;

        int current_id = 0;
        while(current_id < wave_count)
        {
            //использование 2-х точек спавна на карте
            for(int i = 0; i < 2; ++i)
            {
                MarsUnit unit = GetUnitInPool(0, false, spawn_zones.Find("Mars").GetChild(i).localPosition) as MarsUnit;// 0 - солдат, для теста
                unit.ChoseWay((byte)i);
            }
            current_id++;
            yield return new WaitForSeconds(timer);
        }
        LevelDifficul();
    }
    //сложность уровня - тест
    private void LevelDifficul()
    {
        if (wave_count < 15)
            wave_count++;
        if (wave_numb % 2 == 0 && random_mod > 0.25f)
            random_mod -= 0.05f;
    }
    //random mod
    public float GetRandomMod()
    {
        return random_mod;
    }    
}
