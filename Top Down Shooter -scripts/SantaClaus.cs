using UnityEngine;

//игрок
public class SantaClaus : CharacterMechanics
{
    [SerializeField]
    private Transform gun_point;
    private Camera _camera;
    [SerializeField]
    private Rigidbody bullet;
    [SerializeField]
    private ParticleSystem current_smoke;
    [SerializeField]
    private AudioSource foot_sound;
    public GameObject smoke;
    
    private float stun_timer;
    public int id_gun;
    [SerializeField]
    private bool iStunning;


    private void Start()
    {
        game_controller.Registration(this);
        foot_sound = GetComponent<AudioSource>();

        tag = "Player";
        _camera = Camera.main;
        gun_point = transform.Find("Santa Claus/Gun/Point");
        id_gun = 1;
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !animator.GetCurrentAnimatorStateInfo(1).IsName("Shoot") && !iStunning)
            animator.SetTrigger("Shooting");
        if (Time.time >= stun_timer && iStunning)
            iStunning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Charge") && !iStunning)
        {
            iStunning = true;
            smoke.SetActive(true);
            foot_sound.Stop();
            stun_timer = Time.time + 2.5f;
            animator.SetFloat("Vertical", 0f);
            animator.SetFloat("Horizontal", 0f);
        }
        if (other.transform.CompareTag("Hit"))
        {
            print("I hit");
            TakeDamage(-Random.Range(15,25));
        }
            
    }

    public override void ChoseDirection(out Vector3 c_direction)
    {
        base.ChoseDirection(out c_direction);
        if (!iStunning)
        {
            //поворот
            {
                Vector3 mouse = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.y - transform.position.y));
                Quaternion look_direction = Quaternion.LookRotation(mouse - transform.position);
                transform.localRotation = Quaternion.Euler(transform.up * look_direction.eulerAngles.y);
                transform.TransformDirection(mouse - transform.position);
            }
            //движение - анимация с использванием BlendTree 
            {
                if (Input.GetAxis("Horizontal") != 0)
                {
                    c_direction += transform.right * Input.GetAxis("Horizontal");
                    animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
                }
                if (Input.GetAxis("Vertical") != 0)
                {
                    c_direction += transform.forward * Input.GetAxis("Vertical");
                    animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
                }
                if (c_direction != Vector3.zero)
                {
                    if (!foot_sound.isPlaying)
                        foot_sound.Play();
                    smoke.SetActive(false);
                }
                else
                {
                    smoke.SetActive(true);
                    foot_sound.Stop();
                    animator.SetFloat("Horizontal", 0f);
                    animator.SetFloat("Vertical", 0f);
                }
            }
        }
    }

    private void TakeAim()
    {
        bullet = game_controller.GetProjectileInPool(id_gun, out current_smoke);
    }

    private void Shoot()
    {
        current_smoke.transform.position = gun_point.position;
        current_smoke.transform.rotation = Quaternion.Euler(gun_point.eulerAngles);
        current_smoke.gameObject.SetActive(true);

        bullet.gameObject.SetActive(true);
        bullet.position = gun_point.position;
        bullet.AddForce(gun_point.forward * 150f, ForceMode.Impulse);
    }
}
