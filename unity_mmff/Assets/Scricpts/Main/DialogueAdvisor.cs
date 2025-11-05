using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueAdvisor : MonoBehaviour
{
    public DashboardController Getpoint;
    public int[] point;
    public TextMeshProUGUI dialogueadvisor;

    void Start()
    {
        point = new int[5];
    }
}
