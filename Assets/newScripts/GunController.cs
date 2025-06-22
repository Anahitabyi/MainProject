using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform gun;


    // Update is called once per frame
   void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - gun.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (direction.x >= 0)
        {
            // Face right
            gun.rotation = Quaternion.Euler(0, 0, angle);
            Vector3 scale = gun.localScale;
            scale.y = Mathf.Abs(scale.y);
            gun.localScale = scale;
        }
        else
        {
            // Face left → flip around Y axis and reverse angle
            gun.rotation = Quaternion.Euler(0, 180f, -angle);
            Vector3 scale = gun.localScale;
            scale.y = -Mathf.Abs(scale.y);
            gun.localScale = scale;
        }
    }
    public void Shoot(){
        
    }
}
