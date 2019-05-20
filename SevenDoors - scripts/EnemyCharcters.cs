using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharcters : BasicController
{
    public enum EnemyType { Zombie, BigZombie, FlyDemon}
    public EnemyType type;
    [SerializeField]
    private Transform player_position;
    [SerializeField]
    private Vector3 last_position;

    [SerializeField]
    private float timer;
    [SerializeField]
    private bool see_player;
    [SerializeField]
    private bool attack;
    [SerializeField]
    private bool rush;
    [SerializeField]
    private bool can_hit;


    private void Start()
    {
        EnemySetup();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerHit"))
        {
            see_player = true;
            EnemyTakeDamage();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Flip"))
        {
            side *= -1;
            if (see_player && !can_hit)
                can_hit = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
            grounded = true;
    }

    private void EnemySetup()
    {
        Setup();

        fire_point.gameObject.SetActive(false);

        player_position = GameObject.FindGameObjectWithTag("Player").transform;
        see_player = false;

        Invoke("ResetEnemyCollider", 0.2f);
        StartCoroutine(AIWork());
    }

    public void ResetEnemyCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        rb_character.isKinematic = true;
        rb_character.velocity = Vector2.zero;
        collider.isTrigger = true;
        grounded = true;
    }

    public void ReuseEnemyUnit()
    {
        switch (type)
        {
            case (EnemyType.FlyDemon):
                health = 2;
                break;
            default:
                break;
        }
    }
    
    private void DistanceToPlayer()
    {
        float x = CheckDistance(false);
        
        if(Mathf.Abs(x) < 4f && !enemy_rush && Time.time >= timer && type == EnemyType.BigZombie)
        {
            last_position = player_position.localPosition;
            side = ((last_position - transform.localPosition).x > 0) ? 1 : -1;
            enemy_rush = true;
            speed *= 3f;
            HitCollider();
        }

        float s_distance = (type != EnemyType.BigZombie) ? 2.5f : 8f;

        if (Mathf.Abs(x) < s_distance && Mathf.Abs(CheckDistance(true)) <= 0.5f)
            see_player = true;
        else
            see_player = false;

        //melee
        if (type != EnemyType.FlyDemon)
        {
            if (Mathf.Abs(x) <= 1f && see_player)
            {
                side = 0;
                if (x < 0f)
                    Flip();
                attack = true;
            }
            else
                attack = false;
        }
        else
        {
            //range
            if (Mathf.Abs(x) < 7.5f && Time.time >= timer && see_player)
            {
                side = 0;
                if (x < 0f)
                    Flip();
                attack = true;
                timer = Time.time + 3f;
            }
            else
                attack = false;
        }
        
    }
    //only FlyDemon
    private void RangeFire()
    {
        attaking = !attaking;
        if(attaking)
        {
            Rigidbody2D projectile = GameController.init.GetObjectInPool(11).GetComponent<Rigidbody2D>();
            projectile.transform.position = transform.position;
            projectile.transform.rotation = (ch_sprite.flipX) ? Quaternion.Euler(Vector3.forward * 180f) : Quaternion.Euler(Vector3.zero);
            projectile.velocity = (ch_sprite.flipX)?Vector2.left * 5f: Vector2.right * 5f;
        }
    }

    //only BigZombie
    private void DistanceToPoint()
    {
        Vector2 dir = last_position - transform.position;
        if(dir.x < 2f && !anim_character.GetCurrentAnimatorStateInfo(2).IsName("Fire"))
        {
            HitCollider();
            attack = true;
        }
        if (dir.x < 1f)
        {
            speed /= 3f;
            enemy_rush = false;
            timer = Time.time + 4f;
        }
    }

    private float CheckDistance(bool axeY)
    {
        Vector2 dir = player_position.localPosition - transform.position;
        if (!axeY)
            return dir.x;
        else
            return dir.y;
    }

    private void HitCollider()
    {
        fire_point.gameObject.SetActive(!fire_point.gameObject.activeSelf);
        attaking = !attaking;
    }

    private void EnemyTakeDamage()
    {
        see_player = true;
        TakeDamage();
        if(gameObject.activeSelf)
            StartCoroutine(EnemyHit());
    }

    IEnumerator EnemyHit()
    {
        bool hiting = true;
        bool r_color = false;
        while (hiting)
        {
            if (ch_sprite.color.g > 0.15f && !r_color)
                ch_sprite.color = Color.Lerp(ch_sprite.color, Color.red, 15f * Time.deltaTime);
            else
                r_color = true;
            if (r_color)
            {
                if (ch_sprite.color.g < 0.9f)
                    ch_sprite.color = Color.Lerp(ch_sprite.color, Color.white, 25f * Time.deltaTime);
                else
                {
                    ch_sprite.color = Color.white;
                    hiting = false;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator AIWork()
    {
        side = (Random.Range(0f, 1f) >= 0.5f) ? 1 : -1;
        float timer = Random.Range(2.5f, 5f);

        while (gameObject.activeSelf)
        {
            if (attack)
            {
                Fire();
                if (type != EnemyType.FlyDemon)
                {
                    if (enemy_rush)
                        yield return new WaitForSeconds(0.35f);
                    else
                        yield return new WaitForSeconds(0.8f);
                    side = (ch_sprite.flipX) ? -1 : 1;
                }
            }
            else
            {
                if (!enemy_rush)
                {
                    if (see_player)
                    {
                        if (can_hit)
                        {
                            yield return new WaitForSeconds(1f);
                            can_hit = false;
                        }
                        else
                            yield return new WaitForSeconds(0.25f);
                        side = (CheckDistance(false) > 0) ? 1 : -1;
                    }
                    else
                    {
                        if (timer <= 0f)
                        {
                            side = 0;
                            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
                            side = (ch_sprite.flipX) ? -1 : 1;
                            timer = Random.Range(2.5f, 5f);
                        }
                        timer -= Time.deltaTime;
                    }
                }
            }
            if (enemy_rush)
                DistanceToPoint();
            else
                DistanceToPlayer();

            yield return new WaitForEndOfFrame();
        }
    }
    
}
