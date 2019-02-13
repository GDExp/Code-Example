using UnityEngine;

//основной класс управления юнитами
public class CharacterMechanics : MonoBehaviour
{
    //ссылки
    protected GameController game_controller;
    protected Animator animator;
    private CharacterController controller;

    //значения
    public float speed;
    [SerializeField]
    protected int health;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        game_controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();        
    }

    private void MoveCharacter()
    {
        Vector3 direction = Vector3.zero;

        if (!controller.isGrounded)
            direction = Vector3.up * -15f;
        else
            ChoseDirection(out direction);
        if(direction != Vector3.zero)
            controller.Move(direction * speed * Time.deltaTime);
    }
    public void OnFixedUpdate()
    {
        MoveCharacter();
    }
    public virtual void ChoseDirection(out Vector3 c_direction)
    {
        c_direction = Vector3.zero;
    }

    public virtual void DeadUnit()
    {
        //!!!
    }

    protected void TakeDamage(int damage)
    {
        if (health - Mathf.Abs(damage) > 0)
        {
            //+++отображение урона
            health += damage;
        }
        else
        {
            //+++возврат в пулл при необходимости и ресет
            health = 0;
            DeadUnit();
        }

    }
}
