using UnityEngine;

public class thumbmid : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float flex2 = websocket.Instance.GloveFlex2;
        transform.rotation = Quaternion.Euler(0, 0, -90 * flex2 - 90); 
    }
}
