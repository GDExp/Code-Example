using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapsMechanics : MonoBehaviour
{
    public enum TypeTrap { SawTrap, ArrowTrap, AxeTrap};
    public TypeTrap type_trap;
    private Rigidbody2D rb_trap;
    private Transform point;
    private float speed;
    public float axe_speed;

    private void Start()
    {
        switch (type_trap)
        {
            case (TypeTrap.ArrowTrap):
                SetupArrowTrap();
                break;
            case (TypeTrap.AxeTrap):
                SetupAxe();
                break;
            default://saw
                SetupSaw();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flip"))
            FlipMoveTrap();
    }

    #region SAW-Trap

    private void SetupSaw()
    {
        speed = Random.Range(3f, 6f);
        rb_trap = GetComponent<Rigidbody2D>();
        rb_trap.velocity = Vector2.right * speed;
        rb_trap.angularVelocity = -360f;
    }

    private void FlipMoveTrap()
    {
        var sprite = GetComponent<SpriteRenderer>();
        sprite.flipX = !sprite.flipX;
        rb_trap.angularVelocity *= -1f;
        rb_trap.velocity = Vector2.right * ((sprite.flipX) ? -1 : 1) * speed;
    }

    #endregion

    #region Arrow-Trap

    private void SetupArrowTrap()
    {
        
        rb_trap = GetComponentInChildren<Rigidbody2D>();
        point = transform.Find("Point");
        speed = Random.Range(3.5f, 6f);
        ActiveArrow();

        StartCoroutine(ArrowTrapWork(0));
    }

    private void ActiveArrow()
    {
        //reset arrow
        if (rb_trap.gameObject.activeSelf)
        {
            rb_trap.transform.localPosition = Vector2.zero;
            rb_trap.transform.localPosition += Vector3.forward;
        }
        rb_trap.gameObject.SetActive(!rb_trap.gameObject.activeSelf);
    }

    IEnumerator ArrowTrapWork(byte shoot_side)
    {
        float timer = Random.Range(0.5f, 2f);
        while (gameObject.activeSelf)
        {
            if (!rb_trap.gameObject.activeSelf)
            {
                yield return new WaitForSeconds(timer);
                {
                    ActiveArrow();
                    rb_trap.velocity = transform.up * speed;
                }
            }
            else
            {
                if ((rb_trap.transform.localPosition - point.localPosition).magnitude <= 2f )
                    ActiveArrow();
                yield return new WaitForFixedUpdate();
            }
        }
    }

    #endregion

    #region AXE-Trap

    private void SetupAxe()
    {
        rb_trap = GetComponentInChildren<Rigidbody2D>();
        rb_trap.angularVelocity = (Random.Range(0f, 1f) >= 0.5f) ? axe_speed : -axe_speed;
    }

    #endregion
}
