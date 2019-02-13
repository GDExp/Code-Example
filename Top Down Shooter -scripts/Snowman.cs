using System.Collections.Generic;
using UnityEngine;

//юнит ИИ
public class Snowman : CharacterMechanics
{
    private CapsuleCollider hit_collider;
    private SphereCollider charge_collider;
    private Transform present_point;
    private List<Vector3> way;
    private Vector3 start_point;
    public Transform player;
    public Transform end;//test point

    private float timer_charge;
    private float timer_task;
    public byte move_status;
    public byte action_status;


    private void Start()
    {
        game_controller.Registration(this);

        hit_collider = transform.Find("Snowman/Spine_1/Spine_2/Arm_1.R/Arm_2.R/Heand.R").GetComponentInChildren<CapsuleCollider>();
        hit_collider.enabled = false;
        charge_collider = transform.Find("Charge").GetComponent<SphereCollider>();
        charge_collider.enabled = false;
        present_point = hit_collider.transform.Find("Point");
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //настройка
        way = new List<Vector3>();
        start_point = transform.localPosition;
        timer_task = Time.time + Random.Range(1f, 3f);
    }

    private void Update()
    {
        //test
        if (Input.GetKeyDown(KeyCode.Space) && way.Count == 0)
            game_controller.GetWayToPoint(ref way, transform.localPosition, end.localPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Projectile"))
        {
            TakeDamage(-Random.Range(15, 30));
        }
    }

    public override void ChoseDirection(out Vector3 c_direction)
    {
        base.ChoseDirection(out c_direction);
        if (action_status == 99) return;
        if (way.Count == 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {
            move_status = 0;
            animator.SetBool("Moving", false);
            if (action_status == 1)
            {
                timer_charge = Time.time + 4f;
                action_status = 0;
                charge_collider.enabled = false;
                WorkAnimation();
            }
            if (action_status == 2)
            {
                game_controller.GetWayToPoint(ref way, transform.localPosition, end.localPosition);
                game_controller.SetObjectInPool(0, present_point.GetChild(0));
                action_status = 0;
            }
            if (action_status == 0)
            {
                CheckInfo();
                if (Time.time >= timer_task && timer_task != 0f)
                {
                    timer_task = 0f;
                    game_controller.GetWayToPoint(ref way, transform.localPosition, end.localPosition);
                }
            }
            return;
        }

        CheckInfo();
        WorkAnimation();
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {
            float distance = (transform.localPosition - way[0]).magnitude;
            Quaternion look = Quaternion.LookRotation(way[0] - transform.localPosition);
            transform.localRotation = Quaternion.Euler(Vector3.up * look.eulerAngles.y);

            if (distance > 15f)
                c_direction = transform.forward;
            else
                way.Remove(way[0]);
        }
        else
            c_direction = Vector3.zero;

    }

    public override void DeadUnit()
    {
        base.DeadUnit();
        move_status = 0;
        action_status = 99;
        WorkAnimation();
        animator.SetBool("Moving", false);
        animator.SetTrigger("Dead");
    }

    private void ResetUnit()
    {
        //пока возврат в пулл
        game_controller.SetObjectInPool(1, transform);
        game_controller.DeregistrationActiveUnits(this);
        action_status = 0;
        way.Clear();
    }

    private void CheckInfo()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.localPosition + Vector3.up * 4f, transform.forward * 15f);
        //проверка на подарки
        Debug.DrawRay(transform.localPosition + Vector3.up * 4f, transform.forward * 15f, Color.blue);
        if (way.Count == 1 && Physics.Raycast(ray, out hit, 15f) && hit.transform.CompareTag("Bag"))
        {
            game_controller.GetObjectInPool(0, present_point);
            game_controller.GetWayToPoint(ref way, transform.localPosition, start_point);
            move_status = 2;//stealing
            action_status = 2;
        }
        //проверка на игрока
        if (action_status == 2) return;
        ray = new Ray(transform.localPosition + Vector3.up * 4f, (player.localPosition + Vector3.up * 5f - transform.localPosition) * 100f);
        if (Physics.Raycast(ray, out hit, 100f) && hit.transform.CompareTag("Player") && !hit.transform.CompareTag("Hit"))
        {
            Debug.DrawLine(transform.localPosition + Vector3.up * 4f, player.localPosition + Vector3.up * 5f, Color.red);
            float distance = (transform.localPosition - player.localPosition).magnitude;
            bool kill = (distance <= 20f) ? true : false;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hit") && !kill && action_status != 1)
            {
                game_controller.GetWayToPoint(ref way, transform.localPosition, player.localPosition);
                way.Remove(way[0]);
            }
            if (distance > 75f && distance < 100f && Time.time >= timer_charge && action_status == 0)
            {
                action_status = 1;
                move_status = 1;
                game_controller.GetWayToPoint(ref way, transform.localPosition, player.localPosition);
                charge_collider.enabled = true;
            }
            if (distance <= 20f && !animator.GetCurrentAnimatorStateInfo(0).IsName("Hit") && kill)
            {
                animator.SetBool("Moving", false);
                animator.SetTrigger("Hiting");
            }
        }
        else
        {
            if(timer_task == 0f)
                timer_task = Time.time + Random.Range(0.5f, 2f);
        }
    }
    //три вида анимаци и скорости
    private void WorkAnimation()
    {
        switch (move_status)
        {
            
            case (1)://убегает с подарком
                speed = 70f;
                animator.SetInteger("Move Stage", move_status);
                break;
            case (2)://нападение на игрока
                speed = 45f;
                animator.SetInteger("Move Stage", move_status);
                break;
            default:
                speed = 30f;
                animator.SetInteger("Move Stage", move_status);
                animator.SetBool("Moving", true);
                break;
        }
    }
}
