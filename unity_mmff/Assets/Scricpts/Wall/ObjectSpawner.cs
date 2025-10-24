using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 15f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRandomObject();
            timer = 0f;
        }
    }

    void SpawnRandomObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogWarning("ยังไม่ได้กำหนด Prefab ของที่จะเสก!");
            return;
        }

        int randomIndex = Random.Range(0, objectPrefabs.Length);
        GameObject prefabToSpawn = objectPrefabs[randomIndex];
        Vector3 positionToSpawn = (spawnPoint != null) ? spawnPoint.position : this.transform.position;
        
        Instantiate(prefabToSpawn, positionToSpawn, Quaternion.identity);
    }
}