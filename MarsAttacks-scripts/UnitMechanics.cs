using System.Collections.Generic;
using UnityEngine;

//базовый класс всех юнитов - анимация, движение, стрельба
public class UnitMechanics : MonoBehaviour
{
    //основные ссылки
    protected GameController game_controller;
    protected PlayerController player_controller;
    protected Rigidbody rb_unit;
    private ZoneController current_zone;
    private Transform gun_point;
    protected Transform unit_ui;
    private Animator an_unit;

    //навигация
    protected List<Vector3> unit_way;
    private Rigidbody target;
    private Rigidbody bullet;

    //основные значения
    public float speed_unit;
    private float update_time;
    private float reload_time;
    protected short max_health;
    protected short current_health;
    public byte unit_type;
    protected bool iMove;

    private void Reset()
    {
        tag = "Unit";
    }

    private void Awake()
    {
        rb_unit = GetComponent<Rigidbody>();
        an_unit = GetComponent<Animator>();
        gun_point = transform.Find("Visual/Humanoid Armature/GunPole/Gun").GetChild(0);

        game_controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        player_controller = game_controller.PlayerReference();
        unit_way = new List<Vector3>();
        update_time = Time.time + 0.5f;

        if(this is EarthUnit)
            max_health = 75;
        else
            max_health = 50;

        reload_time = Time.time + 3.5f;

        current_health = max_health;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            rb_unit.useGravity = false;
            rb_unit.velocity = Vector3.zero;
            rb_unit.angularVelocity = Vector3.zero;
            iMove = true;
            if (unit_ui == null)
                unit_ui = player_controller.GetUnitUI(this);
            else
                unit_ui.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
        }

        if (other.CompareTag("Bullet"))
        {
            UnitTakeDamage((short)(Random.Range(15, 35)));
            game_controller.ReturnObjectInPool(other.GetComponent<Rigidbody>(), 0);
        }

        if (other.CompareTag("Flag") && current_zone != null)
            current_zone.RefreshCaptureZone(this);
    }

    private void FixedUpdate()
    {
        UnitMovement();
        if (Time.time >= update_time && current_zone != null)
        {
            update_time = Time.time + 1f;
            target = current_zone.GetNearUnit(this, transform.localPosition);
        }
    }

    //движение юнита
    private void UnitMovement()
    {
        if (!iMove) return;

        //проврека цели
        if (target != null && Time.time >= reload_time 
            && (transform.localPosition - target.transform.localPosition).sqrMagnitude < 1600f)
        {
            Vector3 dir = (target.position  - gun_point.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            rb_unit.rotation = Quaternion.Euler(Vector3.up * lookRotation.eulerAngles.y);

            an_unit.SetTrigger("Shooting");
            bullet = game_controller.GetBulletInPool(gun_point.position);
            if (this is EarthUnit && unit_way.Count == 0)
                reload_time = Time.time + 1.5f;
            else
                reload_time = Time.time + 3.5f;
        }

        //стрельба юнита
        if (unit_way.Count > 0 && !an_unit.GetCurrentAnimatorStateInfo(1).IsName("Shoot"))
        {
            //проверка на направление и дистанцию
            float distance = (transform.localPosition - unit_way[0]).sqrMagnitude;
            float angle = Vector3.Angle(transform.forward, (unit_way[0] - transform.localPosition));
            if(angle > 1f)
            {
                Vector3 dir = (unit_way[0] - transform.localPosition).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                rb_unit.rotation = Quaternion.Euler(Vector3.up * lookRotation.eulerAngles.y);
            }
            if (distance < 2f)
                unit_way.Remove(unit_way[0]);
            rb_unit.velocity = transform.forward * speed_unit;
        }
        else
        {
            rb_unit.velocity = Vector3.zero;
            rb_unit.angularVelocity = Vector3.zero;
            //стрельба в режиме укрепления
            if ((an_unit.GetCurrentAnimatorStateInfo(1).IsName("Shoot")|| an_unit.GetCurrentAnimatorStateInfo(1).IsName("Shoot Def"))
                && target != null)
            {
                Vector3 dir;
                if (target.velocity != Vector3.zero)
                {
                    float dis = Vector3.Distance(gun_point.position, target.position);
                    dir = ((target.position + target.velocity * (dis / 100f)) - gun_point.position).normalized;
                }
                else
                    dir = (target.position - gun_point.position).normalized;

                Quaternion lookRotation = Quaternion.LookRotation(dir);
                rb_unit.rotation = Quaternion.Euler(Vector3.up * lookRotation.eulerAngles.y);
            }
        }
        if(rb_unit.velocity != Vector3.zero)
            unit_ui.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
        //работа с анимацией 
        if (!rb_unit.useGravity && rb_unit.velocity != Vector3.zero)
        {
            an_unit.SetBool("Moving", true);
            if (this is EarthUnit)
                an_unit.SetBool("Defense", false);
        }
        else
        {
            an_unit.SetBool("Moving", false);
            if (this is EarthUnit && unit_way.Count == 0)
                an_unit.SetBool("Defense", true);
        }
    }
    //стрельба юнита
    private void UnitShoot()
    {
        if (bullet == null) return;
        bullet.transform.localPosition = gun_point.position;
        //ускорение
        if(this is MarsUnit && Random.Range(0f,1f) < game_controller.GetRandomMod())
        {
            Vector3 r_vector = new Vector3(Random.Range(0f, -1f), Random.Range(0f, -1f), 0f);
            bullet.velocity = (gun_point.forward + r_vector) * 100f;
        }
        else
            bullet.velocity = gun_point.forward * 100f;

        bullet.gameObject.SetActive(true);
    }
    //урон
    private void UnitTakeDamage(short damage)
    {
        //первичное обновление
        if(current_health == max_health)
            unit_ui.Find("Health").gameObject.SetActive(true);
        //обработка
        current_health -= damage;
        if (current_health > 0)
            player_controller.RefreshUnitUI(unit_ui, (float)current_health / max_health, 0);
        else
        {
            if(this is EarthUnit)
                player_controller.CheckSelectedUnit(this);
            unit_ui.gameObject.SetActive(false);
            if (an_unit.GetBool("Moving"))
                an_unit.SetBool("Moving", false);
            if (this is EarthUnit)
                an_unit.SetBool("Defense", false);
            rb_unit.velocity = Vector3.zero;
            iMove = false;
            an_unit.SetTrigger("Dead");
            if(current_zone != null)
                current_zone.ResetUnitList(this);
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
    //лечение
    public void UnitHealing(short health)
    {
        if (current_health >= max_health) return;
        short delta = (short)(max_health - current_health);
        if (health < delta)
            current_health += health;
        else
        {
            current_health += delta;
            unit_ui.Find("Health").gameObject.SetActive(false);
        }
        player_controller.RefreshUnitUI(unit_ui, (float)current_health / max_health, 0);
    }
    //обнуление всех параметров
    protected void ResetUnitValue()
    {
        unit_way.Clear();
        iMove = false;
        unit_ui.gameObject.SetActive(true);
        current_zone = null;
        target = null;
        GetComponent<CapsuleCollider>().enabled = true;
        current_health = max_health;
    }
    //смерть
    public virtual void UnitDead()
    {
        player_controller.RefreshUnitUI(unit_ui, 1f, 9);
        game_controller.ReturnObjectInPool(rb_unit, unit_type);
        ResetUnitValue();
    }
    public bool UnitIsMoving()
    {
        bool moving = (unit_way.Count > 0) ? true : false;
        return moving;
    }
    public void RegistrationZone(ZoneController zone)
    {
        current_zone = zone;
        if (target != null) target = null;
    }
}