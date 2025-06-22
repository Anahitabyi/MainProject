using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 5f;

   // [HideInInspector]
   // public BossShooterDevice boss

    private void Start()
    {

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth ph = collision.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            //if(shooterDevice.Registerdamage(collision.transform)){
           // ph.TakeDamage(damage);
            
          //  }
            
        }
        Destroy(gameObject);
    }
}
