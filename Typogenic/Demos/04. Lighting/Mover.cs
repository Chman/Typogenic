
using UnityEngine;

public class Mover : MonoBehaviour
{
    /*
     *  Moves the light source back and fourth between -4.00 and +4.00.
     */
    public void Update ()
    {
        transform.position = new Vector3(4.00f * Mathf.Sin(Time.realtimeSinceStartup * 2.00f), 1.00f, -4.00f);
    }
}
