using EphemerisDemo.IO;
using System;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.Sim
{
    public class ECIDataDisplay : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private LineRenderer _pathLine;

        [SerializeField]
        private Transform _pathPointsParent;

        [SerializeField]
        private GameObject _pointPrefab;

        [SerializeField]
        private SatelliteIcon _satelliteIconPrefab;

        private TimeControls _timeControls;
        private DisplayData[] _displayData;
        private GameObject _satelliteObject;
        private Vector3 _currentSatVelocity;
        private Vector3 _nextSatVelocity;
        private SatelliteIcon _satelliteIcon;

        public TimeControls TimeControls => _timeControls;
        public GameObject SatelliteObject
        {
            get => _satelliteObject;
            set
            {
                if(_satelliteObject != null)
                {
                    Destroy(_satelliteObject.gameObject);
                }
                _satelliteObject = value;
                _satelliteObject.transform.SetParent(transform);
            }
        }

        public SatelliteIcon Icon => _satelliteIcon;

        public void Initialize(DisplayData[] displayData, TimeControls timeControls, GameObject satelliteObject)
        {
            _displayData = displayData;
            _timeControls = timeControls;
            SatelliteObject = satelliteObject;

            _timeControls.IntervalElapsed += HandleIntervalElapsed;
            _timeControls.PathIndexModified += HandleIntervalElapsed;

            _currentSatVelocity = _displayData[_timeControls.CurrentPathIndex].scaledVelocityKmPs;
            _nextSatVelocity = _displayData[_timeControls.NextPathIndex].scaledVelocityKmPs;

            SatelliteObject.transform.position = _displayData[_timeControls.CurrentPathIndex].scaledPositionKm;
            _satelliteIcon = Instantiate(_satelliteIconPrefab);

            ClearChildren(_pathPointsParent);
            InitializePath(_displayData);
        }

        private void InitializePath(DisplayData[] displayData)
        {
            _pathLine.positionCount = displayData.Length;
            for (int i = 0; i < displayData.Length; i++)
            {
                DisplayData dData = displayData[i];

                _pathLine.SetPosition(i, dData.scaledPositionKm);

                var pathPoint = GameObject.Instantiate(_pointPrefab, _pathPointsParent);
                pathPoint.transform.position = dData.scaledPositionKm;
                pathPoint.name = $"PathPoint_{i}";
            }
        }

        private void HandleIntervalElapsed()
        {
            DisplayData currentData = _displayData[_timeControls.CurrentPathIndex];
            DisplayData nextData = _displayData[_timeControls.NextPathIndex];

            _currentSatVelocity = currentData.scaledVelocityKmPs;
            _nextSatVelocity = nextData.scaledVelocityKmPs;

            SatelliteObject.transform.position = currentData.scaledPositionKm;
        }

        void Update()
        {
            if (_timeControls.IsPaused == false)
            {
                Vector3 velocity = Vector3.Lerp(_currentSatVelocity, _nextSatVelocity, _timeControls.LerpValue);
                SatelliteObject.transform.position += velocity * _timeControls.TimeScale * Time.deltaTime;
                SatelliteObject.transform.rotation = Quaternion.LookRotation(-SatelliteObject.transform.position);
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

        public void Dispose()
        {
            ClearChildren(_pathPointsParent);
            Destroy(gameObject);
        }

        // Zenject factory pattern
        public class Factory : PlaceholderFactory<ECIDataDisplay> { }
    }
}
