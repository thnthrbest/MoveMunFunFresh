using UnityEngine;

public class MoveAndDestroy : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime * -1, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wallkiller"))
        {
            Destroy(gameObject);
        }
    }
}