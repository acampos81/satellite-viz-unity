using UnityEngine;
using UnityEngine.UI;

public class ViewControls : MonoBehaviour
{
    [SerializeField] private GameObject _eciPointsParent;
    [SerializeField] private GameObject _gcsPointsParent;
    [SerializeField] private GameObject _j2000Parent;
    [SerializeField] private OrbitControls _orbitControls;
    [SerializeField] private PathPointsDisplay _pathPointsDisplay;

    public void SetInteractivity(bool isEnabled)
    {
        Toggle[] allToggles =  GetComponentsInChildren<Toggle>();
        for(int i=0; i<allToggles.Length; i++)
        {
            allToggles[i].interactable = isEnabled;
        }
    }

    public void ToggleEarthViewMode(bool isOn)
    {
        if(isOn)
        {
            _orbitControls.SetViewMode(OrbitControls.ViewMode.Earth);
        }
    }

    public void ToggleSatelliteViewMode(bool isOn)
    {
        if (isOn)
        {
            _orbitControls.SetViewMode(OrbitControls.ViewMode.Satellite);
        }
    }

    public void TogglePovViewMode(bool isOn)
    {
        if (isOn)
        {
            _orbitControls.SetViewMode(OrbitControls.ViewMode.FirstPerson);
        }
    }

    public void ToggleEciPoints(bool isOn)
    {
        _eciPointsParent.SetActive(isOn);
    }

    public void ToggleGcsPonts(bool isOn)
    {
        _gcsPointsParent.SetActive(isOn);
    }

    public void ToggleJ2000(bool isOn)
    {
        _j2000Parent.SetActive(isOn);
    }

    public void ToggleAllPoints(bool isOn)
    {
        if(isOn)
        {
            _pathPointsDisplay.SetPointsMode(PathPointsDisplay.PointMode.All);
        }
    }

    public void ToggleNearestPoints(bool isOn)
    {
        if(isOn)
        {
            _pathPointsDisplay.SetPointsMode(PathPointsDisplay.PointMode.Nearest);
        }
    }
}
