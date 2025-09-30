using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SatelliteIcon : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _satellite;
    [SerializeField] private float _minAlphaDistance;
    [SerializeField] private float _maxAlphaDistance;

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
        transform.position = _camera.WorldToScreenPoint(_satellite.position);
    }

    private void DistanceAlpha()
    {
        var cameraDistance = (_camera.transform.position - _satellite.position).magnitude;
        if(cameraDistance > _minAlphaDistance)
        {
            float alphaLerp = (cameraDistance-_minAlphaDistance)/ _maxAlphaDistance;
            Color imageColor = _image.color;
            imageColor.a = Mathf.Clamp01(alphaLerp);
            _image.color = imageColor;
        }
    }
}
