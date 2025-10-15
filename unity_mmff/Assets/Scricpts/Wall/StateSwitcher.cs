using UnityEngine;

public class StateSwitcher : MonoBehaviour
{
    public GameObject pointObject;
    public GameObject damageObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            if (pointObject != null)
            {
                pointObject.SetActive(false);
            }

            if (damageObject != null)
            {
                damageObject.SetActive(true);
            }
        }
    }
}