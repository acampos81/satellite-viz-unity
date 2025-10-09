using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SatelliteIcon : MonoBehaviour
{
    [SerializeField] private float _minAlphaDistance;
    [SerializeField] private float _maxAlphaDistance;

    public Transform SatelliteTransform { get; set; }

    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        ScreenPosition();
        DistanceAlpha();
    }

    private void ScreenPosition()
    {
        transform.position = Camera.main.WorldToScreenPoint(SatelliteTransform.position);
    }

    private void DistanceAlpha()
    {
        var cameraDistance = (Camera.main.transform.position - SatelliteTransform.position).magnitude;
        if(cameraDistance > _minAlphaDistance)
        {
            float alphaLerp = (cameraDistance-_minAlphaDistance)/ _maxAlphaDistance;
            Color imageColor = _image.color;
            imageColor.a = Mathf.Clamp01(alphaLerp);
            _image.color = imageColor;
        }
    }
}
