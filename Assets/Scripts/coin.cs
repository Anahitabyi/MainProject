using UnityEngine;

public class Fruit : MonoBehaviour, IItem
{
    public void Collect()
    {
        Destroy(gameObject);
    }
}
