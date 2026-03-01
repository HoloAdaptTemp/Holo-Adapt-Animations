using UnityEngine;

public class thumbroot : MonoBehaviour
{
    public float prev_flex;
    public float deriv;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        prev_flex = 0;
        deriv = 0;
   }

    // Update is called once per frame
    void Update()
    {
        float flex2 = websocket.Instance.GloveFlex2;
        // transform.rotation = Quaternion.Euler(0, 0, -30 * flex1 - 90); 
        deriv = flex2 - prev_flex;
        prev_flex = flex2; 
        transform.Rotate(0, 0, deriv * -45, Space.Self);
    }
}
