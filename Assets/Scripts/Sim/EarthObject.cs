using EphemerisDemo.IO;
using EphemerisDemo.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace EphemerisDemo.Sim
{
    public class EarthObject : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _primeMeridianLine;

        [SerializeField]
        private LineRenderer _equatorLine;

        [SerializeField]
        private Transform _gcsPointsParent;

        [SerializeField]
        private GameObject _pointPrefab;

        private TimeControls _timeControls;
        private DisplayData[] _displayData;
        private Dictionary<string, Transform> _parentsByFileName;
        private Quaternion _currentEarthRotation;
        private Quaternion _nextEarthRotation;

        private void Start()
        {
            _parentsByFileName = new Dictionary<string, Transform>();
        }

        /// <summary>
        /// Provides the scale of the simulation coordinate space basd on the local scale of the earth game object
        /// </summary>
        /// <returns></returns>
        public float GetSimScale()
        {
            return transform.localScale.x / DemoUtils.EquatorialDiameterKm;
        }

        public void SetTimeControls(TimeControls newTimeControls)
        {
            if(_timeControls != null)
            {
                _timeControls.IntervalElapsed -= HandleIntervalElapsed;
                _timeControls.PathIndexModified -= HandlePathIndexModified;
            }
            _timeControls = newTimeControls;
            _timeControls.IntervalElapsed += HandleIntervalElapsed;
            _timeControls.PathIndexModified += HandlePathIndexModified;
        }

        public void SetDisplayData(DisplayData[] displayData)
        {
            _displayData = displayData;
        }

        public void Initialize(TimeControls timeControls, DisplayData[] displayData)
        {
            SetTimeControls(timeControls);
            SetDisplayData(displayData);
            _currentEarthRotation = GetEarthRotation(_displayData[_timeControls.CurrentPathIndex]);
            _nextEarthRotation = GetEarthRotation(_displayData[_timeControls.NextPathIndex]);
        }

        private void HandleIntervalElapsed()
        {
            DisplayData currentData = _displayData[_timeControls.CurrentPathIndex];
            DisplayData nextData = _displayData[_timeControls.NextPathIndex];

            _currentEarthRotation = GetEarthRotation(currentData);
            _nextEarthRotation = GetEarthRotation(nextData);
        }

        private void HandlePathIndexModified()
        {
            HandleIntervalElapsed();
            transform.localRotation = _currentEarthRotation;
        }

        public void AddGCSPoints(string fileName, DisplayData[] displayData)
        {
            GameObject fileParent = new GameObject(fileName);
            fileParent.transform.SetParent(_gcsPointsParent);

            for (int i = 0; i < displayData.Length; i++)
            {
                DisplayData dData = displayData[i];
                var gcsPoint = GameObject.Instantiate(_pointPrefab, fileParent.transform);
                gcsPoint.transform.position = dData.scaledGcsPoint;
                gcsPoint.name = $"GCSPoint_{i}";
            }

            _parentsByFileName[fileName] = fileParent.transform;
        }

        public void RemoveGCSPoints(string fileName)
        {
            if (_parentsByFileName.TryGetValue(fileName, out Transform pointsParent))
            {
                ClearChildren(pointsParent);
                Destroy(pointsParent.gameObject);
                _parentsByFileName.Remove(fileName);
            }
        }

        public void TogglePointsByFileName(string fileName, bool isEnabled)
        {
            if(_parentsByFileName.TryGetValue(fileName, out Transform pointsParent))
            {
                pointsParent.gameObject.SetActive(isEnabled);
            }
        }

        private void ClearChildren(Transform parent)
        {
            int children = parent.childCount;
            for (int i = 0; i < children; i++)
            {
                Destroy(parent.GetChild(0).gameObject);
            }
        }

        private void Update()
        {
            if (_timeControls != null && _timeControls.IsPaused == false)
            {
                transform.localRotation = Quaternion.Slerp(_currentEarthRotation, _nextEarthRotation, _timeControls.LerpValue);
            }
        }

        private Quaternion GetEarthRotation(DisplayData displayData)
        {
            // Get the lat/long earth rotation
            var gcsRotation = DemoUtils.GcsRadiansToRotation(displayData.ephemerisData.gcsRadians);

            // Calculate the spherical delta from the lat/long origin to the current eci position.
            // This assumes the earth's model's lat/long origin is initially aligned with the J2000 X axis.
            var eciRotation = Quaternion.LookRotation(displayData.scaledPositionKm.normalized);
            var eciDelta = eciRotation * Quaternion.Inverse(Quaternion.LookRotation(Vector3.right));

            // Calculate the rotation from current lat/long rotation to the ECI rotation relative to the J2000 x axis.
            return eciDelta * Quaternion.Inverse(gcsRotation);
        }

#if UNITY_EDITOR
        public void DrawEarthLines()
        {
            int resolution = 256;
            float increment = 360f / resolution;

            _primeMeridianLine.positionCount = resolution;
            _equatorLine.positionCount = resolution;

            for (int i = 0; i < resolution; i++)
            {
                var pmRotation = Quaternion.AngleAxis(increment * i, transform.forward);
                var eqRotation = Quaternion.AngleAxis(increment * i, transform.up);
                var pmPoint = pmRotation * Vector3.right * transform.localScale.x * 0.501f;
                var eqPoint = eqRotation * Vector3.right * transform.localScale.x * 0.501f;
                _primeMeridianLine.SetPosition(i, pmPoint);
                _equatorLine.SetPosition(i, eqPoint);
            }
        }
#endif
    }
}
