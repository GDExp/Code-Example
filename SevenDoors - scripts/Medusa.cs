using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medusa : MonoBehaviour
{
    public static Medusa init;
    //main
    private Rigidbody2D m_rigidbody;
    private Animator m_animator;
    private SpriteRenderer m_sprite;
    private ParticleSystem smoke;
    private Transform field;
    private Transform fire_point;
    private Transform player;
    private Transform teleport_points;

    //pool
    public GameObject key;
    public GameObject portal;
    public GameObject summon;
    public GameObject[] projectile;

    private Transform pool;
    private List<GameObject> all_projectiles;
    private List<float> projectile_timer;
    [SerializeField]
    private List<GameObject> all_demons;

    
    private float speed;
    private int health;//15
    private int side;
    private int stage;//0 - simple attack, 1 - air attack, 2 - summon
    private byte current_lvl;
    private bool grounded;
    public bool player_in_area;
    private bool attack;
    private bool teleport;
    private bool air_strike;
    private bool healing = false;

    private void Awake()
    {
        init = this;
    }

    private void Start()
    {
        SetupMedusa();
        SetupLocalPool();

        StartCoroutine(AIMedusa());
    }

    private void LateUpdate()
    {
        //return projectile in pool
        if (projectile_timer.Count > 0)
        {
            if (Time.time >= projectile_timer[0])
            {
                ReturnObjectInPool(all_projectiles[0]);
                all_projectiles.Remove(all_projectiles[0]);
                projectile_timer.Remove(projectile_timer[0]);
            }

        }

        if (!smoke.isPlaying && teleport)
        {
            teleport = false;
            smoke.transform.SetParent(transform);
            smoke.transform.localPosition = Vector3.zero;
        }
    }

    #region USE PHYSICS

    private void FixedUpdate()
    {
        MedusaMove();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            ResetCollider();
            grounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerHit"))
            TakeDamage();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Flip"))
            Flip();
    }

    #endregion


    #region SETUP

    private void SetupMedusa()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_sprite = GetComponent<SpriteRenderer>();
        smoke = GetComponentInChildren<ParticleSystem>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        field = transform.Find("Field");
        field.gameObject.SetActive(false);

        fire_point = transform.Find("FirePoint");
        teleport_points = GameObject.Find("TeleportPoints").transform;

        speed = 2f;
        health = 15;
        
    }

    private void SetupLocalPool()
    {
        pool = transform.Find("Pool");
        all_projectiles = new List<GameObject>();
        projectile_timer = new List<float>();
        all_demons = new List<GameObject>();
    }

    private void ResetCollider()
    {
        m_rigidbody.isKinematic = true;
        GetComponent<Collider2D>().isTrigger = true;
    }

    #endregion

    #region MOVEMENT

    private void MedusaMove()
    {
        if (side != 0)
            m_animator.SetInteger("State", 1);
        else
        {
            if (!m_animator.GetCurrentAnimatorStateInfo(1).IsName("Fire"))
                m_animator.SetInteger("State", 0);
        }

        if (grounded)
            m_rigidbody.velocity = Vector2.right * side * speed;
        else
        {
            if (m_rigidbody.velocity.y > -8f)
                m_rigidbody.velocity += Vector2.down * 2 * 0.55f;
        }
    }

    private void Flip()
    {
        side *= -1;
        m_sprite.flipX = (side > 0) ? true : false;

        fire_point.localPosition = (side > 0) ? new Vector2(2.5f, -0.75f) : new Vector2(-2.5f, -0.75f);
    }

    private void Teleport()
    {
        smoke.transform.SetParent(null);
        smoke.transform.localScale = Vector3.one;
        smoke.Play();
        //transform.position = test_point.position;
        //current_lvl
        {
            if (transform.position.y < 6f) current_lvl = 0;
            if (transform.position.y > 6f) current_lvl = 1;
        }
        Vector3 to_point = Vector3.zero;
        switch (stage)
        {
            case (1)://air - point
                to_point = teleport_points.GetChild(2).GetChild(0).position + Vector3.right * Random.Range(-5f, 5f);
                break;
            case (2)://summon - point
                to_point = teleport_points.GetChild(2).GetChild(0).position;
                break;
            default:
                to_point = teleport_points.GetChild((current_lvl > 0) ? 0 : 1).GetChild(Random.Range(0, 2)).position;
                break;
        }
        transform.position = to_point;
        teleport = true;
        side = (sbyte)((Random.Range(0f, 1f) >= 0.5f) ? 1 : -1);
        Flip();
    }

    IEnumerator EnemyHit()
    {
        bool hiting = true;
        bool r_color = false;
        while (hiting)
        {
            if (m_sprite.color.g > 0.15f && !r_color)
                m_sprite.color = Color.Lerp(m_sprite.color, Color.red, 15f * Time.deltaTime);
            else
                r_color = true;
            if (r_color)
            {
                if (m_sprite.color.g < 0.9f)
                    m_sprite.color = Color.Lerp(m_sprite.color, Color.white, 25f * Time.deltaTime);
                else
                {
                    m_sprite.color = Color.white;
                    hiting = false;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void TakeDamage()
    {
        StartCoroutine(EnemyHit());
        if (healing)
        {
            field.gameObject.SetActive(false);
            stage = 0;
            healing = false;
            EndSummonAndHealing();
            Teleport();
        }
        else
        {
            if (health - 1 < 0)
                DeadMedusa();
            else
            {
                health--;
                UIController.init.UIcmd(10);
            }
        }
    }

    private void BloodFX()
    {
        var blood = GameController.init.GetObjectInPool(3).GetComponent<ParticleSystem>();
        blood.gameObject.transform.position = transform.position;
        blood.Play();
    }

    private void MakeMeat()
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, Vector2.down, Mathf.Infinity);
        for (int i = 0; i < hit.Length; ++i)
        {
            if (hit[i].collider.CompareTag("Ground"))
            {
                Instantiate(GameController.init.meat_sprite, hit[i].point, Quaternion.identity).transform.localPosition += Vector3.forward * -2;
                Instantiate(key, hit[i].point + Vector2.up * 1f, Quaternion.identity).transform.localPosition += Vector3.forward * -2;
                return;
            }
        }
    }

    private void DeadMedusa()
    {
        BloodFX();
        MakeMeat();
        MusicManager.init.PlaySoundClip(2);
        UIController.init.CloseBossUI();
        GameAnalytics.init.CMDUnityAnalytics(777);// - win
        GameAnalytics.init.CMDUnityAnalytics(2);//end lvl
        Destroy(gameObject);
    }

    #endregion

    #region FIRE

    IEnumerator LineFire()
    {
        int count = Random.Range(3, 5);
        
        Rigidbody2D projectile;
        while(count > 0)
        {
            if ((player.position - transform.position).x < 0)
                Flip();
            m_animator.SetTrigger("Fire");
            projectile = GetObjectInPool(0).GetComponent<Rigidbody2D>();
            if (Random.Range(0,1f) >= 0.5f)
                projectile.position = fire_point.position + Vector3.up * 0.5f + Vector3.back * 2f;
            else
                projectile.position = fire_point.position + Vector3.down * 0.75f + Vector3.back * 2f;
            projectile.transform.rotation = (!m_sprite.flipX) ? Quaternion.Euler(Vector3.forward * 180f) : Quaternion.Euler(Vector3.zero);
            projectile.velocity = (!m_sprite.flipX) ? Vector2.left * 5f : Vector2.right * 5f;
            count--;
            yield return new WaitForSeconds(0.75f);
        }
        yield return null;
    }

    IEnumerator AirStrike()
    {
        byte count = (byte)Random.Range(3, 6);
        while (air_strike)
        {
            if ((player.position - transform.position).x > 0)
                Flip();
            m_animator.SetTrigger("Fire");
            side = 0;

            MakeShootAir();
            
            yield return new WaitForSeconds(2f);
            count--;
            if (count == 0)
                air_strike = false;
        }
        yield return null;
    }

    private void MakeShootAir()
    {
        Rigidbody2D projectile;
        float force = 6f;
        bool mirror = false;

        Vector3 direction = player.position - transform.position;
        Vector3 point = transform.position + direction * (2f / direction.magnitude);//point on line
        Debug.DrawRay(transform.position, direction, Color.red, Mathf.Infinity);

        Vector3 n_dir = Vector3.zero;

        for (int i = 3; i > 0; --i)
        {
            projectile = GetObjectInPool(0).GetComponent<Rigidbody2D>();
            projectile.transform.position = point + Vector3.back * 3f;
            if (i == 3)
            {
                projectile.transform.localRotation = Quaternion.Euler(direction.normalized);
                projectile.velocity = direction.normalized * force;
            }
            else
            {
                n_dir = direction + ((mirror) ? Vector3.up : Vector3.down) * 3f;
                projectile.transform.localRotation = Quaternion.Euler(n_dir.normalized);
                projectile.velocity = n_dir.normalized * force;
                mirror = true;
            }
        }
    }

    #endregion

    #region SUMMONING

    IEnumerator SummonDemons()
    {
        int count_portal = Random.Range(2, 4);
        int portal_lvl = 0;
        Vector3 portal_point = Vector3.zero;
        SpriteRenderer n_poratal;
        List<SpriteRenderer> portals = new List<SpriteRenderer>();
        //stage - 1 create portals
        while (count_portal > 0)
        {
            portal_point = new Vector3(Random.Range(0, 14f), 0f, -5f);
            float x_random = Random.Range(0, 1f);
            if (x_random < 0.25f) portal_lvl = 0;
            if (x_random > 0.5f) portal_lvl = 1;
            if (x_random > 0.75f) portal_lvl = 2;

            switch (portal_lvl)//clamp x 0 to 14
            {
                case (1)://lvl - 8.5
                    portal_point += Vector3.up * 8.5f;
                    break;
                case (2)://lvl - 5.0
                    portal_point += Vector3.up * 4.75f;
                    break;
                default://lvl - 2.5
                    portal_point += Vector3.up * 2.0f;
                    break;
            }

            n_poratal = GetObjectInPool(2).GetComponent<SpriteRenderer>();
            //zero value = close portal
            {
                n_poratal.color = Color.clear;
                n_poratal.transform.localScale = Vector3.zero;
                n_poratal.transform.position = portal_point;
            }
            portals.Add(n_poratal);

            yield return new WaitForEndOfFrame();
            count_portal--;
        }
        // stage 2 - open portals
        while (portals[0].transform.localScale.x < 0.9f)
        {
            for(int i =0; i < portals.Count; ++i)
            {
                portals[i].transform.localScale = Vector3.Lerp(portals[i].transform.localScale, Vector3.one, 2f * Time.deltaTime);
                portals[i].color = Color.Lerp(portals[i].color, Color.white, 2f * Time.deltaTime);
            }
            yield return new WaitForEndOfFrame();
        }
        // stage 3 -summon & close
        yield return new WaitForSeconds(1f);
        GameObject demon;
        for(int i = 0; i < portals.Count; ++i)
        {
            demon = GetObjectInPool(1);
            demon.transform.position = portals[i].transform.position - Vector3.forward * Random.Range(0.25f, 0.5f);
            all_demons.Add(demon);
            ReturnObjectInPool(portals[i].gameObject);
            yield return new WaitForEndOfFrame();
            demon.GetComponent<EnemyCharcters>().ResetEnemyCollider();
        }

        yield return null;
    }

    private void EndSummonAndHealing()
    {
        for (int i = 0; i < all_demons.Count; ++i)
        {
            if (!all_demons[i].activeSelf)
                continue;
            ReturnObjectInPool(all_demons[i]);
        }
        all_demons.Clear();
        UIController.init.UIcmd(12);//deactive UI slider timer
        if (!healing) return;
        //++ HP
        health += 4;
        for (int i = 0; i < 4; ++i)
            UIController.init.UIcmd(11);
        if (health > 15)
            health = 15;
        //telort low lvl
        attack = false;
        stage = 0;
    }

    #endregion

    #region LOCAL POOL MANAGER

    private GameObject GetObjectInPool(int id)
    {
        GameObject pool_obj = null;
        Transform current_pool;

        switch (id)
        {
            case (1)://demon
                current_pool = pool.GetChild(1);
                if (current_pool.childCount > 0)
                {
                    pool_obj = current_pool.GetChild(0).gameObject;
                    pool_obj.GetComponent<EnemyCharcters>().ReuseEnemyUnit();
                    pool_obj.transform.SetParent(null);
                }
                else
                    pool_obj = CreateNewObject(summon);
                break;
            case (2)://portal
                current_pool = pool.GetChild(2);
                if (current_pool.childCount > 0)
                {
                    pool_obj = current_pool.GetChild(0).gameObject;
                    pool_obj.transform.SetParent(null);
                }
                else
                    pool_obj = CreateNewObject(portal);
                break;
            default://projectile
                current_pool = pool.GetChild(id);
                if (current_pool.childCount > 0)
                {
                    pool_obj = current_pool.GetChild(0).gameObject;
                    pool_obj.transform.SetParent(null);
                }
                else
                    pool_obj = CreateNewObject(projectile[((Random.Range(0f, 1f) > 0.5f) ? 0 : 1)]);

                all_projectiles.Add(pool_obj);
                projectile_timer.Add(Time.time + 3f);
                break;
        }

        pool_obj.SetActive(true);
        pool_obj.name = id.ToString();

        return pool_obj;
    }

    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject n_object = Instantiate(prefab);
        return n_object;
    }

    private void ReturnObjectInPool(GameObject obj)
    {
        obj.SetActive(false);

        int id_obj = System.Convert.ToInt32(obj.name);

        obj.transform.SetParent(pool.GetChild(id_obj));
    }
    

    #endregion

    #region SENSOR

    private void DistanceToPlayer()
    {
        if (air_strike || stage == 2) return;
        Vector2 direction = (transform.position - player.position);
        print(direction);
        if (Mathf.Abs(direction.x) < 12f && Mathf.Abs(direction.y) < 1.2f)
            attack = true;
        else
        {
            attack = false;
            if (!player_in_area)//if player dead or start
                stage = 0;
        }
    }

    #endregion

    IEnumerator AIMedusa()
    {
        yield return new WaitForEndOfFrame();
        //active UI in canvas
        UIController.init.SetupBossPanel(health, 10f);//need ending - 7

        side = (sbyte)((Random.Range(0f,1f) >= 0.5f) ? 1 : -1);
        float timer = 5f;
        float teleport_timer = 5.5f;
        bool was_summoned = false;
        Flip();

        while (gameObject.activeSelf)
        {
            if (attack)
            {
                switch (stage)
                {
                    case (1)://air
                        side = 0;
                        air_strike = true;
                        yield return AirStrike();
                        yield return new WaitForSeconds(1.5f);
                        stage = 0;
                        Teleport();
                        side = (Random.Range(0f, 1f) > 0.5f) ? 1 : -1;
                        break;
                    case (2)://summon if player don't hit boss - boss healing
                        side = 0;
                        field.gameObject.SetActive(true);
                        if (healing)
                        {
                            if (timer > 0f)
                            {
                                timer -= Time.deltaTime;
                                UIController.init.UIcmd(13);//work UI slider timer
                            }
                            else
                            {
                                EndSummonAndHealing();
                                field.gameObject.SetActive(false);
                            }
                            yield return new WaitForEndOfFrame();
                        }
                        else
                        {
                            yield return SummonDemons();
                            healing = true;
                            was_summoned = true;
                            timer = 10f;// in UI controller slider max value
                        }
                        break;
                    default://simple
                        side = 0;
                        yield return LineFire();
                        side = (m_sprite.flipX) ? 1 : -1;
                        attack = false;
                        yield return new WaitForSeconds(3f);
                        break;
                }
            }

            if (player_in_area)
            {
                
                if (stage == 0)
                {
                    if (health > 6)//simple + air(50)
                    {
                        if(health < 12)
                        {
                            if (timer > 0f)
                                timer -= Time.deltaTime;
                            else
                            {
                                timer = 5f;
                                RandomStage();
                            }
                        }
                        
                    }
                    else//summon + air (75) + simple
                    {
                        if (timer > 0f)
                            timer -= Time.deltaTime;
                        else
                        {
                            if (was_summoned)
                            {
                                was_summoned = false;
                                timer = 3.5f;
                                RandomStage();
                            }
                            else
                            {
                                UIController.init.UIcmd(12);//active UI Slider timer
                                timer = 7.5f;
                                stage = 2;
                                attack = true;
                                Teleport();
                            }
                        }
                        print(timer);
                    }

                    //teleport time
                    if (teleport_timer > 0f)
                        teleport_timer -= Time.deltaTime;
                    else
                    {
                        teleport_timer = 4.5f;
                        Teleport();
                    }

                }
                else
                    teleport_timer = 4.5f;


                DistanceToPlayer();
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void RandomStage()
    {
        float x = Random.Range(0f, 1f);
        if (health > 7)
            stage = (x > 0.5f) ? 1 : 0;
        else
            stage = (x > 0.75) ? 0 : 1;
        if (stage == 1)
        {
            air_strike = true;
            attack = true;
            Teleport();
        }
    }

}
