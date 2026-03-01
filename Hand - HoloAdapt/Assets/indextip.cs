using UnityEngine;

public class indextip : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float flex1 = websocket.Instance.GloveFlex1;
        //transform.rotation = Quaternion.Euler(0, 0, -90 * flex1 - 90); 
        transform.Rotate(0, 0, flex1 * -90, Space.Self);

    }
}