using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private const float EquatorialDiameterKm = 12756f;

    public struct EphemerisRowData
    {
        public DateTimeOffset timeStamp;
        public Vector3        eciPositionKm;
        public Vector3        eciVelocityKmPs;
        public Vector2        gcsRadians;
    }

    public struct DisplayData
    {
        public EphemerisRowData ephemerisData;
        public float nextDataInterval;
        public Vector3 scaledPositionKm;
        public Vector3 scaledVelocityKmPs;
        public Vector3 scaledGcsPoint;
    }

    public Transform satellite;
    public Transform earthTransform;
    public GameObject pointPrefab;
    public Transform  pathPointsParent;
    public Transform  gcsPointsParent;
    public LineRenderer equatorLine;
    public LineRenderer primeMeridianLine;
    public LineRenderer pathLine;
    public Slider timeSlider;
    public TMP_InputField timeScaleField;

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
        var dataRows = ParseEphemerisData();
        var scale = earthTransform.localScale.x/EquatorialDiameterKm;
        _displayData = GetDisplayData(dataRows, scale);

        DrawData(_displayData);

        _pathIndex = 0;
        _currentInterval = _displayData[_pathIndex].nextDataInterval;
        _currentVel = _displayData[_pathIndex].scaledVelocityKmPs;
        _nextVel = _displayData[_pathIndex+1].scaledVelocityKmPs;

        satellite.position = _displayData[_pathIndex].scaledPositionKm;

        _currentEarthRotation = GetEarthRotation(_displayData[_pathIndex]);
        _nextEarthRotation = GetEarthRotation(_displayData[_pathIndex+1]);
    }

    void Update()
    {
        float.TryParse(timeScaleField.text, out float timeScale);
        timeScale = Mathf.Clamp(timeScale, 0.01f, 1000f);

        _elapsedInterval += Time.deltaTime;
        if(_elapsedInterval >= _currentInterval)
        {
            float delta = _elapsedInterval - _currentInterval;
            _elapsedInterval = delta;

            _pathIndex = (_pathIndex+1)%_displayData.Length;

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
        Vector3 vel = Vector3.Lerp(_currentVel, _nextVel, lerpValue);

        satellite.position += vel*Time.deltaTime;
        satellite.rotation = Quaternion.LookRotation(vel);

        earthTransform.localRotation = Quaternion.Slerp(_currentEarthRotation, _nextEarthRotation, lerpValue);
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

    private List<EphemerisRowData> ParseEphemerisData()
    {
        List<EphemerisRowData> dataRows = new List<EphemerisRowData>();
        string sourceFolder = Path.Combine(Application.dataPath, "Data");
        foreach (string filePath in Directory.EnumerateFiles(sourceFolder, "*.csv", SearchOption.TopDirectoryOnly))
        {
            CsvHelper.Configuration.CsvConfiguration csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfig.Delimiter = ",";
            csvConfig.AllowComments = true;
            csvConfig.Encoding = Encoding.UTF8;
            csvConfig.PrepareHeaderForMatch = args => args.Header.ToLowerInvariant();  // force all the csv headers to lowercase

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true))
            using (CsvReader csv = new CsvReader(sr, csvConfig))
            {
                csv.Read();
                csv.ReadHeader();  // first line is always the header line
                while(csv.Read())
                {
                    var posixSec  = double.Parse(csv.GetField<string>("utc_posix_sec"));
                    var timeStamp = DateTimeOffset.FromUnixTimeMilliseconds((long)Math.Round(posixSec*1000d));
                    var posX      = (float)double.Parse(csv.GetField<string>("eci_pos_x_km"));
                    var posY      = (float)double.Parse(csv.GetField<string>("eci_pos_y_km"));
                    var posZ      = (float)double.Parse(csv.GetField<string>("eci_pos_z_km"));
                    var velX      = (float)double.Parse(csv.GetField<string>("eci_vel_x_kmps"));
                    var velY      = (float)double.Parse(csv.GetField<string>("eci_vel_y_kmps"));
                    var velZ      = (float)double.Parse(csv.GetField<string>("eci_vel_z_kmps"));
                    var lat       = (float)double.Parse(csv.GetField<string>("latitude_rad"));
                    var lon       = (float)double.Parse(csv.GetField<string>("longitude_rad"));

                    EphemerisRowData rowData = new EphemerisRowData
                    {
                        timeStamp       = timeStamp,
                        eciPositionKm   = new Vector3(posX, posY, posZ),
                        eciVelocityKmPs = new Vector3(velX, velY, velZ),
                        gcsRadians      = new Vector2(lat,lon)
                    };

                    dataRows.Add(rowData);
                }
            }
        }
        return dataRows;
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
