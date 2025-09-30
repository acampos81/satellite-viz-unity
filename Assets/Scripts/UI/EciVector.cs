using TMPro;
using UnityEngine;

public class EciVector : MonoBehaviour
{
    [SerializeField] private TMP_InputField _xField;
    [SerializeField] private TMP_InputField _yField;
    [SerializeField] private TMP_InputField _zField;

    public void UpdateVector3(Vector3 v3)
    {
        _xField.text = $"{v3.x:0.00}";
        _yField.text = $"{v3.y:0.00}";
        _zField.text = $"{v3.z:0.00}";
    }
}
