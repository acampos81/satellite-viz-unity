using UnityEngine;

public class PathPointsDisplay : MonoBehaviour
{
    public enum PointMode
    {
        All,
        Nearest
    }

    [SerializeField] private Transform _gcsPointsParent;
    [SerializeField] private Transform _eciPointsParent;
    [SerializeField] private LineRenderer _eciPathLine;
    [SerializeField] private int _neighborCount = 1;

    public int DataIndex { get; set; }

    private GameObject[] _gcsPoints;
    private GameObject[] _eciPoints;
    private int _dataIndex;
    private Vector3[] _pathPoints;
    private PointMode _pointMode;

    public void Initialize(int dataPoints)
    {
        _pathPoints = new Vector3[dataPoints];
        _eciPathLine.GetPositions(_pathPoints);

        _eciPoints = new GameObject[dataPoints];
        _gcsPoints = new GameObject[dataPoints];

        for(int i=0; i< dataPoints; i++)
        {
            _eciPoints[i] = _eciPointsParent.GetChild(i).gameObject;
            _gcsPoints[i] = _gcsPointsParent.GetChild(i).gameObject;
        }
    }

    public void SetDataIndex(int dataIndex)
    {
        _dataIndex = dataIndex;

        if(_pointMode == PointMode.Nearest)
        {
            int startIndex = _dataIndex-_neighborCount;
            int endIndex   = _dataIndex+_neighborCount;
            int count      = (endIndex - startIndex)+1;
        
            _eciPathLine.positionCount = count;
            int pathIndex = Mathf.Clamp(startIndex, 0, _pathPoints.Length);
            for (int i=startIndex; i<count; i++)
            {
                if(i<0){ continue; }
                _eciPathLine.SetPosition(pathIndex, _pathPoints[pathIndex]);
                pathIndex++;
            }

            startIndex = Mathf.Clamp(startIndex, 0, _pathPoints.Length);
            endIndex   = Mathf.Clamp(endIndex, _dataIndex, _pathPoints.Length);
            for(int i=0; i<_pathPoints.Length; i++)
            {
                _eciPoints[i].SetActive(i >= startIndex && i <= endIndex);
                _gcsPoints[i].SetActive(i >= startIndex && i <= endIndex);
            }
        }
    }

    public void SetPointsMode(PointMode pointMode)
    {
        _pointMode = pointMode;
        
        if(_pointMode == PointMode.All)
        {
            _eciPathLine.positionCount = _pathPoints.Length;
            _eciPathLine.SetPositions(_pathPoints);

            for(int i=0; i<_pathPoints.Length; i++)
            {
                _eciPoints[i].SetActive(true);
                _gcsPoints[i].SetActive(true);
            }
        }
        else
        {
            SetDataIndex(_dataIndex);
        }
    }
}

