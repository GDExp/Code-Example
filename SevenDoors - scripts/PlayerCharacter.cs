using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BasicController
{
    private GameObject fade_layer;
    private Transform ground_point_1;
    private Transform ground_point_2;

    private Collider2D ch_collider;
    private Camera _camera;
    private Trigger current_active_obj;
    private GameObject reload_sprite;
    private ParticleSystem reload_shotgun;
    [SerializeField]
    private float jump;
    private byte ammo_count;
    private bool activeObj;
    private bool camera_move;
    private bool in_fade_layer;
    [SerializeField]
    private bool have_key;

    private void Start()
    {
        PlayerSetup();
        ch_collider = GetComponent<Collider2D>();

        StartCoroutine(RuntimeChecking());
    }
    
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            UIController.init.UIcmd(7);
            GameController.init.test_end = true;
        }
        if (Application.platform != RuntimePlatform.Android && PlayerPrefs.GetInt("Input") != 1)
        {
            if (!anim_character.GetCurrentAnimatorStateInfo(1).IsName("Fire"))
            {
                if (Input.GetButtonDown("Fire") && grounded && !attaking && ammo_count != 0 && side == 0)
                {
                    Fire();
                    attaking = true;
                }

                if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    if (rb_character.velocity == Vector2.zero && !grounded)
                        grounded = true;
                    if (grounded)
                    {
                        side = (Input.GetAxisRaw("Horizontal") > 0) ? 1 : -1;
                        ChangePlayerValue();
                    }
                }
                else
                {
                    side = 0;
                }
                    

                if (Input.GetButtonDown("Jump"))
                    Jump();

                if (Input.GetKeyDown(KeyCode.E) && activeObj)
                {
                    current_active_obj.UseActivObject();
                }
            }
        }

    }

    IEnumerator RuntimeChecking()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForFixedUpdate();
            if(side != 0)
                CheckGround();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hit"))
        {
            TakeDamage();
            UIController.init.UIcmd(4);
            ReturnToPSP();
        }
        if(collision.CompareTag("Projectile"))
        {
            TakeDamage();
            UIController.init.UIcmd(4);
            collision.transform.parent.gameObject.SetActive(false);
            ReturnToPSP();
        }
        if(collision.CompareTag("Health") && health != 3)
        {
            Healing();
            Destroy(collision.gameObject);
        }
        if (collision.CompareTag("Coin"))
        {
            UIController.init.UIcmd(0);
            Destroy(collision.gameObject);
        }
        if (collision.CompareTag("Key"))
        {
            UIController.init.UIcmd(3);
            Destroy(collision.gameObject);
            have_key = true;
        }
        if (collision.CompareTag("RedDoor") && have_key)
        {
            UIController.init.OpenDoor(collision.GetComponent<SpriteRenderer>(), 0);
            DataManager data = new DataManager();
            data.SaveData(GameController.init.current_lvl_id + 1);
        }
        if(collision.CompareTag("Check"))
            SpecialGroundPlatform(collision.transform.parent.GetComponent<Collider2D>());
        if (collision.CompareTag("Trigger"))
        {
            activeObj = true;
            current_active_obj = collision.GetComponent<Trigger>();
            current_active_obj.TriggerIsActive();
            UIController.init.SwithButtons();
        }

        if (collision.CompareTag("Area"))
        {
            Medusa.init.player_in_area = true;
        }

        //test end game
        if(collision.CompareTag("End"))
        {
            UIController.init.UIcmd(7);
            GameController.init.test_end = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Check"))
            SpecialGroundPlatform(collision.transform.parent.GetComponent<Collider2D>());
        if (collision.CompareTag("Trigger"))
        {
            activeObj = false;
            current_active_obj.TriggerIsActive();
            current_active_obj = null;
            UIController.init.SwithButtons();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Checking();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Checking();
    }

    private void SpecialGroundPlatform(Collider2D special_ground)
    {
        special_ground.isTrigger = !special_ground.isTrigger;
    }

    private void PlayerSetup()
    {
        Setup();

        fade_layer = GameObject.Find("BG_Fade");
        FadeLayer();

        ground_point_1 = transform.Find("GroundPoint_1");
        ground_point_2 = transform.Find("GroundPoint_2");

        _camera = Camera.main;
        reload_sprite = transform.Find("AmmoReload").gameObject;
        reload_sprite.SetActive(false);
        reload_shotgun = GetComponentInChildren<ParticleSystem>();
        ammo_count = 5;

        StartCoroutine(ReloadShotgun());
    }

    private void ChangePlayerValue()
    {
        ground_point_1.localPosition = (side > 0) ? new Vector2(-0.2f, -1.8f) : new Vector2(0.2f, -1.8f);
        ground_point_2.localPosition = (side > 0) ? new Vector2(-0.7f, -1.8f) : new Vector2(0.7f, -1.8f);
        ch_collider.offset = (side < 0) ? new Vector2(0.45f, ch_collider.offset.y) : new Vector2(-0.45f, ch_collider.offset.y);
    }

    private void Jump()
    {
        if (!grounded) return;
        rb_character.velocity += Vector2.up * jump * 3f;
        grounded = false;
        
    }

    private void Checking()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(fire_point.position, 0.1f);
        if (hits.Length > 0)
        {
            grounded = false;
            rb_character.AddForce((ch_sprite.flipX) ? Vector2.right * 0.5f : Vector2.left * 0.5f, ForceMode2D.Impulse);
        }

        CheckGround();
    }

    private void CheckGround()
    {

        Collider2D[] _hits = Physics2D.OverlapAreaAll(ground_point_1.position, ground_point_2.position);
        //Collider2D[] _hits = Physics2D.OverlapPointAll(ground_point_2.position);
        if (_hits.Length > 0)
        {
            for (int i = 0; i < _hits.Length; ++i)
            {
                if (_hits[i].CompareTag("Ground") || _hits[i].CompareTag("Box") || _hits[i].CompareTag("Platform"))
                {
                    grounded = true;
                    if (_hits[i].CompareTag("Platform"))
                    {
                        _hits[i].GetComponent<Rigidbody2D>().isKinematic = false;
                        Invoke("CheckGround", 0.2f);
                        Destroy(_hits[i].gameObject, 2.5f);
                    }
                    if (_hits[i].gameObject.layer == 8 && !in_fade_layer)
                    {
                        in_fade_layer = true;
                        FadeLayer();
                    }
                    if (_hits[i].gameObject.layer == 9 && in_fade_layer)
                    {
                        in_fade_layer = false;
                        FadeLayer();
                    }
                    return;
                }
            }
            grounded = false;
        }
        else
            grounded = false;
    }
   

    private void FadeLayer()
    {
        fade_layer.SetActive(!fade_layer.activeSelf);
    }

    #region FIRE

    public override void Fire()
    {
        base.Fire();
        PlayerShoot();
        Rebound();
        
    }

    private void PlayerShoot()
    {
        StartCoroutine(CameraJolt());
        if (reload_sprite.activeSelf)
            ReloadAnim();

        RaycastHit2D hit = Physics2D.Raycast(fire_point.position, (ch_sprite.flipX) ? Vector2.left : Vector2.right, Mathf.Infinity, 9);
        if (hit.transform is null) return;
        print(hit.transform.tag);
        if (hit.transform.CompareTag("Box") || hit.transform.CompareTag("Enemy") || hit.transform.CompareTag("Ground"))
        {
            if (hit.collider.CompareTag("Box"))
            {
                GameObject crush = GameController.init.GetObjectInPool(2);
                crush.transform.localPosition = hit.transform.position;
                crush.GetComponent<ParticleSystem>().Play();
                Destroy(hit.transform.gameObject);
            }

            ParticleSystem fx_hit = GameController.init.GetObjectInPool(1).GetComponent<ParticleSystem>();
            fx_hit.transform.localPosition = hit.point;
            fx_hit.transform.localPosition += Vector3.forward * -2f;
            fx_hit.Emit(1);
        }
        
    }

    private void Rebound()
    {
        rb_character.transform.localPosition += Vector3.right * ((ch_sprite.flipX) ? 1 : -1) * 0.1f;
    }

    public override void FlipWeaponParticle()
    {
        base.FlipWeaponParticle();
        reload_shotgun.transform.localPosition = (ch_sprite.flipX) ? new Vector3(-0.325f, -0.27f, -1f) : new Vector3(0.325f, -0.27f, -1f);
        reload_shotgun.transform.localRotation = (ch_sprite.flipX) ? Quaternion.Euler(-160f, -90f, 0f) : Quaternion.Euler(-160f, 90f, 0f);
    }

    private void PSShotgun()
    {
        UIController.init.UIcmd(1);//
        --ammo_count;

        reload_shotgun.Emit(1);
        attaking = false;

    }

    private void ReloadAnim()
    {
        anim_character.SetBool("Reload", !reload_sprite.activeSelf);
        reload_sprite.SetActive(!reload_sprite.activeSelf);
    }

    private void ReturnToPSP()
    {
        if (fade_layer.activeSelf)
        {
            in_fade_layer = false;
            FadeLayer();
        }

        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 3)
            _camera.GetComponent<CameraController>().ResetCamera();
        if (ammo_count != 5)
        {
            for (int i = ammo_count; i < 5; ++i)
                UIController.init.UIcmd(2);
            ammo_count = 5;
        }
        if (reload_sprite.activeSelf)
            ReloadAnim();
        rb_character.velocity = Vector2.zero;
        rb_character.transform.position = GameController.init.GetStartPosition();
    }

    IEnumerator ReloadShotgun()
    {
        float reload_time = 1f;
        bool reload = true;
        while (true)
        {
            if (anim_character.GetCurrentAnimatorStateInfo(1).IsName("Fire") || !grounded)
            {
                if (reload_sprite.activeSelf)
                    ReloadAnim();
                reload = true;
                reload_time = 1f;
            }
            if (side != 0)
            {
                if (reload_sprite.activeSelf)
                {
                    ReloadAnim();
                    reload_time = 1f;
                    reload = true;
                }
            }
            else
            {
                if (reload)
                {
                    if (reload_time <= 0f && ammo_count != 5)
                    {
                        reload = false;
                        reload_time = 0.75f;
                    }
                }
                else
                {
                    if (!reload_sprite.activeSelf)
                        ReloadAnim();
                    if (reload_time <= 0f && ammo_count != 5)
                    {
                        ammo_count++;
                        reload_time = 0.75f;
                        UIController.init.UIcmd(2);
                    }
                    if (ammo_count == 5)
                    {
                        ReloadAnim();
                        reload = true;
                        reload_time = 1f;
                    }
                }
                reload_time -= Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator CameraJolt()
    {
        float timer = 0.4f;
        float camera_angle = 0f;
        bool recur = false;
        while(timer > 0)
        {
            yield return new WaitForEndOfFrame();
            if (camera_angle < 0.15f && !recur)
                camera_angle += Time.deltaTime * 15f;
            else
                recur = true;
            if (camera_angle > -0.15f && recur)
                camera_angle -= Time.deltaTime * 15f;
            else
                recur = false;

            _camera.transform.localRotation = Quaternion.Euler(Vector3.forward * camera_angle);
            timer -= Time.deltaTime;
        }

        _camera.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    #endregion

    private void Healing()
    {
        health++;
        UIController.init.UIcmd(5);
    }

    public override void Dead()
    {
        //base.Dead();
        grounded = false;
        if (Medusa.init != null)
            Medusa.init.player_in_area = false;
        UIController.init.UIcmd(4);
        gameObject.SetActive(false);
        
    }


    public void MobileSide(int i_side)
    {
        if (rb_character.velocity == Vector2.zero && !grounded)
            grounded = true;

        if (!anim_character.GetCurrentAnimatorStateInfo(1).IsName("Fire"))
            side = i_side;
        ChangePlayerValue();
    }

    public void MobileJump()
    {
        if (!anim_character.GetCurrentAnimatorStateInfo(1).IsName("Fire"))
            Jump();
    }

    public void MobileFire()
    {
        if (!anim_character.GetCurrentAnimatorStateInfo(1).IsName("Fire"))
        {
            if (Input.GetButtonDown("Fire") && grounded && !attaking && ammo_count != 0 && side == 0)
            {
                Fire();
                attaking = true;
            }
        }
    }

    public void MobileUse()
    {
        current_active_obj.UseActivObject();
    }



    //Test
    IEnumerator DrawCall(Vector3 point)
    {
        float time = 1f;
        while (time > 0)
        {
            time -= Time.deltaTime;
            Debug.DrawLine(fire_point.position, point, Color.red);
            yield return new WaitForFixedUpdate();
        }
    }
}
