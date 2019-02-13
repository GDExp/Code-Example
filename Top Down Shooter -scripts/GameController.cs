using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    private Transform camera_position;
    private Transform player;
    [SerializeField]
    private List<CharacterMechanics> active_units;
    [SerializeField]
    private List<ParticleSystem> active_particles;
    //ссылки на пулы
    private Transform bullets_pool;
    private Transform particles_pool;
    private Transform presents_pool;
    private Transform snowmans_pool;

    //prefabs
    public Rigidbody[] projectiles;
    public GameObject[] presents;
    public ParticleSystem shoot_smoke;
    public ParticleSystem expl_particle;
    //test
    public GameObject marker;

    private Vector3 offset;

    private void Awake()
    {
        active_units = new List<CharacterMechanics>();
        active_particles = new List<ParticleSystem>();
    }

    private void Start()
    {
        transform.localPosition = Vector3.one * 999f;
        camera_position = Camera.main.transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        offset = camera_position.localPosition - player.localPosition;

        bullets_pool = transform.Find("Projectile");
        particles_pool = transform.Find("Particles");
        presents_pool = transform.Find("Presents");
        snowmans_pool = transform.Find("Snowmans");

        SetupGamePool();
    }

    private void FixedUpdate()
    {
        if (active_units.Count > 0)
        {
            for (int i = 0; i < active_units.Count; ++i)
                active_units[i].OnFixedUpdate();
        }
    }

    private void LateUpdate()
    {
        if(player != null)
            camera_position.localPosition = Vector3.Lerp(camera_position.localPosition, player.localPosition + offset, 0.7f);
        if(active_particles.Count != 0)
        {
            for(int i =0; i< active_particles.Count;++i)
            {
                if (active_particles[i].isStopped && active_particles[i].gameObject.activeSelf)
                {
                    active_particles[i].transform.SetParent(particles_pool.Find(active_particles[i].name));
                    active_particles[i].gameObject.SetActive(false);
                    active_particles.Remove(active_particles[i]);
                }
            }
        }
    }

    private void SetupGamePool()
    {
        //снаряды первого типа
        for(int i = 0; i < 5; ++i)
        {
            Instantiate(projectiles[Mathf.Abs(Random.Range(-3, 3))], bullets_pool.Find("1")).gameObject.SetActive(false);
        }
    }

    public void Registration(CharacterMechanics unit)
    {
        active_units.Add(unit);
    }

    public void DeregistrationActiveUnits(CharacterMechanics unit)
    {
        active_units.Remove(unit);
    }

    public Rigidbody GetProjectileInPool(int type, out ParticleSystem particle)
    {
        Rigidbody projectile = null;
        particle = null;
        Transform current_pool;
        switch (type)
        {
            case (1):
                //снаряд
                current_pool = bullets_pool.Find(type.ToString());
                if (current_pool.childCount > 0)
                    projectile = current_pool.GetChild(0).GetComponent<Rigidbody>();
                else
                    projectile = Instantiate(projectiles[Mathf.Abs(Random.Range(-3, 3))]) as Rigidbody;                
                break;
            default:
                break;
        }

        //частицы - один тип выстрела на все снаряды
        current_pool = particles_pool.Find((type - 1).ToString());
        if (current_pool.childCount > 0)
            particle = current_pool.GetChild(0).GetComponent<ParticleSystem>();
        else
        {
            particle = Instantiate(shoot_smoke);
            particle.name = (type - 1).ToString();
        }

        projectile.GetComponent<ProjectileMechanics>().SetIdProjectile(type);
        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(null);

        particle.gameObject.SetActive(false);
        particle.transform.SetParent(null);
        active_particles.Add(particle);
        return projectile;
    }

    public void SetProjectileInPool(int type, Rigidbody projectile)
    {
        projectile.velocity = Vector3.zero;
        projectile.angularVelocity = Vector3.zero;
        projectile.gameObject.SetActive(false);

        switch (type)
        {
            case (1):
                projectile.transform.SetParent(bullets_pool.Find(type.ToString()));
                break;
            default:
                break;
        }
    }

    public void GetObjectInPool(int id_object, Transform point)
    {
        Transform obj;
        switch (id_object)
        {
            default://подарки
                if (presents_pool.childCount == 0)
                    obj = Instantiate(presents[Random.Range(0, 2)]).transform;
                else
                    obj = presents_pool.GetChild(0);
                obj.SetParent(point);
                obj.localPosition = Vector3.zero;
                obj.localRotation = Quaternion.identity;
                obj.gameObject.SetActive(true);
                break;
        }
    }

    public void SetObjectInPool(int id_object, Transform obj)
    {
        obj.gameObject.SetActive(false);
        switch (id_object)
        {
            case (1)://снеговики
                obj.SetParent(snowmans_pool);
                break;
            default://подарки
                obj.SetParent(presents_pool);
                break;
        }

        obj.localPosition = Vector3.zero;
    }

    public ParticleSystem GetParticleInPool(int id_particle, Vector3 position)
    {
        ParticleSystem particle = null;
        Transform pool = particles_pool.Find(id_particle.ToString());
        if (pool.childCount > 0)
        {
            particle = pool.GetChild(0).GetComponent<ParticleSystem>();
            particle.transform.SetParent(null);
            particle.transform.localPosition = position;
            particle.gameObject.SetActive(true);
        }
        else
            particle = Instantiate(expl_particle, position, Quaternion.identity);

        particle.name = id_particle.ToString();
        active_particles.Add(particle);
        return particle;
    }

    public void GetWayToPoint(ref List<Vector3> way, Vector3 current_position, Vector3 end_position)
    {
        if (way.Count > 0) way.Clear();

        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(current_position, end_position, NavMesh.AllAreas, path);
        way.AddRange(path.corners);
    }
}
