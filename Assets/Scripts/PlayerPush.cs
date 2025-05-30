using UnityEngine;

public class PlayerPusher : MonoBehaviour
{
    public int playerID = 1;
    public KeyCode pushKey = KeyCode.E;
    public PushAngel statue;

    private bool isInsideTrigger = false;

    private void Update()
    {
        if (isInsideTrigger)
        {
            bool isPushing = Input.GetKey(pushKey);
            //statue.SetPlayerPushing(playerID, isPushing, transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == statue.gameObject)
            isInsideTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == statue.gameObject)
        {
            isInsideTrigger = false;
            //statue.SetPlayerPushing(playerID, false, transform);
        }
    }
}