using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicController : MonoBehaviour
{

    protected Rigidbody2D rb_character;
    protected Animator anim_character;
    protected SpriteRenderer ch_sprite;
    protected Transform fire_point;
    


    [SerializeField]
    protected float speed;
    [SerializeField]
    protected int side;
    [SerializeField]
    protected byte health;
    [SerializeField]
    protected bool grounded;
    [ SerializeField ]
    protected bool attaking = false;
    [SerializeField]
    protected bool enemy_rush = false;
    

    private void FixedUpdate()
    {
        Movement();
    }

    protected void Setup()
    {
        rb_character = GetComponent<Rigidbody2D>();
        ch_sprite = GetComponent<SpriteRenderer>();
        anim_character = GetComponent<Animator>();
        fire_point = transform.Find("FirePoint");
    }

    private void Movement()
    {
        float y_value = rb_character.velocity.y;
        if (side != 0)
        {
            if (enemy_rush)
                anim_character.SetInteger("State", 2);
            else
                anim_character.SetInteger("State", 1);
            Flip();
        }
        else
        {
            if (!anim_character.GetCurrentAnimatorStateInfo(1).IsName("Fire"))
                anim_character.SetInteger("State", 0);
        }
        
        if (grounded)
        {
            rb_character.velocity = Vector2.right * side * speed;
        }
        else
        {
            anim_character.SetInteger("State", 0);
            if (rb_character.velocity.y > -8f)
                rb_character.velocity += Vector2.down * 2 * 0.5f;
        }
    }

    protected void Flip()
    {
        ch_sprite.flipX = (side > 0) ? false : true;

        fire_point.localPosition = (side > 0) ? new Vector2(1.3f, -0.3f) : new Vector2(-1.3f, -0.3f);

        if (CompareTag("Player"))
            FlipWeaponParticle();
    }

    public virtual void FlipWeaponParticle()
    {
        //
    }

    public virtual void Fire()
    {
        if (attaking) return;
        anim_character.SetInteger("State", 0);
        if (enemy_rush)
            anim_character.SetTrigger("Rush");
        else
            anim_character.SetTrigger("Fire");
    }

    private void FireAnimReset()
    {
        anim_character.SetInteger("State", 0);
    }

    protected void TakeDamage()
    {
        if (health - 1 > 0)
            health--;
        else
            Dead();

    }
    

    public virtual void Dead()
    {
        BloodFX();
        MakeMeat();
        MusicManager.init.PlaySoundClip(2);
        gameObject.SetActive(false);
    }

    private void MakeMeat()
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, Vector2.down,2f);
        for(int i = 0; i < hit.Length; ++i)
        {
            if(hit[i].collider.CompareTag("Ground"))
            {
                Instantiate(GameController.init.meat_sprite, hit[i].point, Quaternion.identity).transform.localPosition += Vector3.forward * -2;
                return;
            }
        }
    }

    private void BloodFX()
    {
        var blood = GameController.init.GetObjectInPool(3).GetComponent<ParticleSystem>();
        blood.gameObject.transform.position = transform.position;
        blood.Play();
    }
}
