using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private const float EquatorialDiameterKm = 12756f;

    public Transform satellite;
    public Transform earthTransform;
    public GameObject pointPrefab;
    public Transform  pathPointsParent;
    public Transform  gcsPointsParent;
    public LineRenderer equatorLine;
    public LineRenderer primeMeridianLine;
    public LineRenderer pathLine;
    public TMP_InputField timeScaleField;
    public FileLoader fileLoader;
    public TimeControls timeControls;
    public DataBar dataBar;

    private DisplayData[] _displayData;

    private float _currentInterval;
    private float _elapsedInterval;
    private int _pathIndex;
    private Vector3 _currentVel;
    private Vector3 _nextVel;
    private Quaternion _currentEarthRotation;
    private Quaternion _nextEarthRotation;

    void Start()
    {
        fileLoader.OnFileSelected += HandleFileSelected;
    }

    private void HandleFileSelected(string filePath)
    { 

        var dataRows = Parser.ParseEphemerisData(filePath);

        var scale = earthTransform.localScale.x/EquatorialDiameterKm;
        _displayData = GetDisplayData(dataRows, scale);

        _pathIndex = 0;
        _currentInterval = _displayData[_pathIndex].nextDataInterval;
        _currentVel = _displayData[_pathIndex].scaledVelocityKmPs;
        _nextVel = _displayData[_pathIndex+1].scaledVelocityKmPs;

        satellite.position = _displayData[_pathIndex].scaledPositionKm;

        _currentEarthRotation = GetEarthRotation(_displayData[_pathIndex]);
        _nextEarthRotation = GetEarthRotation(_displayData[_pathIndex+1]);

        ClearChildren(pathPointsParent);
        ClearChildren(gcsPointsParent);
        DrawData(_displayData);
    }

    void Update()
    {
        if(_displayData != null)
        {
            _elapsedInterval += timeControls.TimeScale * Time.deltaTime;
            if(_elapsedInterval >= _currentInterval)
            {
                float delta = _elapsedInterval - _currentInterval;
            
                _pathIndex = (_pathIndex+1)%_displayData.Length;
                float nextInterval = _displayData[_pathIndex].nextDataInterval;

                while(delta > nextInterval)
                {
                    delta -= nextInterval;
                    _pathIndex = (_pathIndex + 1) % _displayData.Length;
                    nextInterval = _displayData[_pathIndex].nextDataInterval;
                }

                _elapsedInterval = delta;

                DisplayData currentData = _displayData[_pathIndex];

                _currentInterval = currentData.nextDataInterval;
                _currentVel = currentData.scaledVelocityKmPs;

                int nextIndex = (_pathIndex+1)%_displayData.Length;
                DisplayData nextData = _displayData[nextIndex];
                _nextVel = nextData.scaledVelocityKmPs;

                satellite.position = currentData.scaledPositionKm;

                _currentEarthRotation = GetEarthRotation(currentData);
                _nextEarthRotation = GetEarthRotation(nextData);
            }

            float lerpValue = _elapsedInterval/_currentInterval;
            Vector3 velocity = Vector3.Lerp(_currentVel, _nextVel, lerpValue);

            satellite.position += velocity * timeControls.TimeScale * Time.deltaTime;
            satellite.rotation = Quaternion.LookRotation(velocity.normalized);

            earthTransform.localRotation = Quaternion.Slerp(_currentEarthRotation, _nextEarthRotation, lerpValue);

            dataBar.InterpolateData(_displayData[_pathIndex], _displayData[_pathIndex+1], lerpValue);
        }
    }

    private Quaternion GetEarthRotation(DisplayData displayData)
    {
        var axisRotation = Quaternion.AngleAxis(23.4f, Vector3.right);
        var latitudeRotation = Quaternion.AngleAxis(-displayData.ephemerisData.gcsRadians.x * Mathf.Rad2Deg, Vector3.forward);
        var longitudeRotation = Quaternion.AngleAxis(displayData.ephemerisData.gcsRadians.y * Mathf.Rad2Deg, Vector3.up);

        var eciRotation = Quaternion.LookRotation(displayData.scaledPositionKm.normalized);
        var eciDelta = eciRotation * Quaternion.Inverse(Quaternion.LookRotation(Vector3.right));

        return eciDelta * latitudeRotation * longitudeRotation;
    }

    private void DrawData(DisplayData[] dataList)
    {
        pathLine.positionCount = dataList.Length;
        for(int i=0; i<dataList.Length; i++)
        {
            DisplayData dData = dataList[i];

            pathLine.SetPosition(i, dData.scaledPositionKm);

            var pathPoint = GameObject.Instantiate(pointPrefab, pathPointsParent);
            pathPoint.transform.position = dData.scaledPositionKm;
            pathPoint.name = $"PathPoint_{i}";

            var gcsPoint = GameObject.Instantiate(pointPrefab, gcsPointsParent);
            gcsPoint.transform.position = dData.scaledGcsPoint;
            gcsPoint.name = $"GCSPoint_{i}";
        }
    }

    private Quaternion GcsRadiansToRotation(Vector2 gcsRadians)
    {
        var gcsDegrees = new Vector2
        {
            x = gcsRadians.x * Mathf.Rad2Deg,
            y = gcsRadians.y * Mathf.Rad2Deg
        };
        var latitudeRotation = Quaternion.AngleAxis(gcsDegrees.x, Vector3.forward);
        var longitudeRotation = Quaternion.AngleAxis(-gcsDegrees.y, Vector3.up);
        return longitudeRotation * latitudeRotation;
    }

    private Vector3 GcsRadiansToPosition(Vector2 gcsRadians)
    {
        Quaternion rotation = GcsRadiansToRotation(gcsRadians);
        return rotation * Vector3.right * earthTransform.localScale.x * 0.5f;
    }

    private DisplayData[] GetDisplayData(List<EphemerisRowData> rowData, float scale)
    {
        var displayData = new DisplayData[rowData.Count];
        for (int i=0; i< rowData.Count; i++)
        {
            EphemerisRowData eData = rowData[i];

            float nextDataInterval = 0f;
            if (i < rowData.Count - 1)
            {
                long currentMs = eData.timeStamp.ToUnixTimeMilliseconds();
                long nextMs = rowData[i + 1].timeStamp.ToUnixTimeMilliseconds();
                float intervalMs = Convert.ToSingle(nextMs - currentMs);
                nextDataInterval = intervalMs / 1000f;
            }

            DisplayData dData = new DisplayData
            {
                ephemerisData = eData,
                nextDataInterval = nextDataInterval,
                scaledPositionKm = eData.eciPositionKm * scale,
                scaledVelocityKmPs = eData.eciVelocityKmPs * scale,
                scaledGcsPoint = GcsRadiansToPosition(eData.gcsRadians),
            };

            displayData[i] = dData;
        }

        return displayData;
    }

    private void ClearChildren(Transform parent)
    {
        int children = parent.childCount;
        for (int i=0; i<children; i++)
        {
            Destroy(pathPointsParent.GetChild(0).gameObject);
        }
    }

    public void DrawEarthLines()
    {
        int resolution = 256;
        float increment = 360f/resolution;

        primeMeridianLine.positionCount = resolution;
        equatorLine.positionCount = resolution;

        for(int i=0; i<resolution; i++)
        {
            var pmRotation = Quaternion.AngleAxis(increment*i, earthTransform.forward);
            var eqRotation = Quaternion.AngleAxis(increment*i, earthTransform.up);
            var pmPoint = pmRotation * Vector3.right * earthTransform.localScale.x * 0.5f;
            var eqPoint = eqRotation * Vector3.right * earthTransform.localScale.x * 0.5f;
            primeMeridianLine.SetPosition(i, pmPoint);
            equatorLine.SetPosition(i, eqPoint);
        }
    }
}
