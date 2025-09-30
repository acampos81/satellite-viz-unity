using UnityEngine;

public class AutoOrient : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.rotation = Quaternion.LookRotation(-transform.position.normalized);
    }
}
