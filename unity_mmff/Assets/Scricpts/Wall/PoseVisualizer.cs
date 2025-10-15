using System.Collections.Generic;
using UnityEngine;

public class PoseVisualizer : MonoBehaviour
{
    public UDPReceiver udpReceiver;
    public GameObject pointPrefab;
    public GameObject nosePrefab;

    public Dictionary<int, GameObject> points = new Dictionary<int, GameObject>();
    
    private const int NOSE_ID = 0;

    void Update()
    {
        if (udpReceiver.receivedData != null)
        {
            foreach (LandmarkData lm in udpReceiver.receivedData.landmarks)
            {
                if (!points.ContainsKey(lm.id))
                {
                    GameObject prefabToUse = (lm.id == NOSE_ID) ? nosePrefab : pointPrefab;
                    GameObject newPoint = Instantiate(prefabToUse, this.transform);
                    points[lm.id] = newPoint;
                }
                
                // ## นี่คือบรรทัดที่แก้ไข ##
                // เพิ่มเครื่องหมายลบ (-) ที่ lm.x เพื่อกลับด้านแกนแนวนอน
                Vector3 newPosition = new Vector3(-lm.x, -lm.y, 0f);

                // คุณอาจต้องปรับค่าตัวคูณนี้เพื่อให้เห็นการเคลื่อนไหวชัดเจนขึ้น
                points[lm.id].transform.localPosition = newPosition * 3f; 
            }
        }
    }
}