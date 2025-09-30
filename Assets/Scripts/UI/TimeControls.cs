using TMPro;
using UnityEngine;

public class TimeControls : MonoBehaviour
{
    public TMP_InputField timeScaleField;
    public float TimeScale { get; private set; }

    private void Awake()
    {
        TimeScale = 1f;
    }

    public void ModifyTimeScale(float increment)
    {
        TimeScale = Mathf.Clamp(TimeScale+increment, 0f, 1000);
        timeScaleField.text = TimeScale.ToString();
    }
}
