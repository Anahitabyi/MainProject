using UnityEngine;

public class enemySpawner : MonoBehaviour
{
    [Tooltip("Assign each patrol pair object (with PointA and PointB inside)")]
    public GameObject[] patrolPairs;
    public GameObject enemyPrefab;

    void Start()
    {
        foreach (GameObject pair in patrolPairs)
        {
            Transform pointA = pair.transform.Find("PointA");
            Transform pointB = pair.transform.Find("PointB");

            if (pointA != null && pointB != null)
            {
                Vector3 spawnPos = (pointA.position + pointB.position) / 2f;
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                patrollingEnemy script = enemy.GetComponent<patrollingEnemy>();
                script.pointA = pointA.gameObject;
                script.pointB = pointB.gameObject;
            }
            else
            {
                Debug.LogWarning($"Patrol pair '{pair.name}' is missing PointA or PointB.");
            }
        }
    }
}
