using System.Collections.Generic;
using UnityEngine;

namespace EphemerisDemo.Sim
{
    public class OrbitControls : MonoBehaviour
    {
        [SerializeField] private SatelliteTracker _satelliteTracker;
        [SerializeField] private GameObject _satelliteIcon;
        [SerializeField] private List<CameraSettings> _cameraSettings;

        private Transform _positionTarget;
        private Transform _lookTarget;
        private Camera _currentCamera;
        private float _currentPitch;
        private float _currentYaw;
        private float _currentDistance;
        private float _inputPitch;
        private float _inputYaw;
        private float _inputDistance;
        private Dictionary<ViewMode, CameraSettings> _settingsLookup;
        private CameraSettings _currentSettings;
        private ECIDataDisplay _currentEciDisplay;

        private void Start()
        {
            _settingsLookup = new Dictionary<ViewMode, CameraSettings>();
            foreach (CameraSettings settings in _cameraSettings)
            {
                _settingsLookup.Add(settings.viewMode, settings);
            }

            SetViewMode(ViewMode.Earth);
        }

        public void SetECIDisplay(ECIDataDisplay eciDisplay)
        {
            _currentEciDisplay = eciDisplay;
            _satelliteTracker.TrackSatellite(eciDisplay.SatelliteObject);
            SetViewMode(_currentSettings.viewMode);
        }

        public void SetViewMode(ViewMode viewMode)
        {
            if (_currentCamera != null)
            {
                _currentCamera.gameObject.SetActive(false);
            }

            _currentSettings = _settingsLookup[viewMode];
            _positionTarget = _currentSettings.positionTarget;
            _lookTarget = _currentSettings.lookTarget;
            _currentCamera = _currentSettings.camera;

            _currentCamera.gameObject.SetActive(true);

            _inputPitch = _currentPitch = _currentSettings.pitchDefault;
            _inputYaw = _currentYaw = _currentSettings.yawDefault;
            _inputDistance = _currentDistance = _currentSettings.defaultDistance;

            _satelliteTracker.ShowSatellite(_currentSettings.showSatellite);
            //_satelliteIcon.SetActive(_currentSettings.showIcon);
        }

        private void Update()
        {
            UpdateOrbit();
            UpdateDistance();
            UpdateCamera();
        }

        private void UpdateOrbit()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 mouseDelta = Input.mousePositionDelta;
                float sensitivityX = _currentSettings.orbitSensitivity.x;
                float sensitivityY = _currentSettings.orbitSensitivity.y;
                float pitchMin = _currentSettings.pitchMin;
                float pitchMax = _currentSettings.pitchMax;
                float yawMin = _currentSettings.yawMin;
                float yawMax = _currentSettings.yawMax;

                _inputPitch = Mathf.Clamp(_inputPitch + mouseDelta.y * sensitivityY, pitchMin, pitchMax);
                _inputYaw = Mathf.Clamp(_inputYaw + mouseDelta.x * sensitivityX, yawMin, yawMax);
            }

            float rateX = _currentSettings.orbitRate.x;
            float rateY = _currentSettings.orbitRate.y;
            _currentPitch = Mathf.Lerp(_currentPitch, _inputPitch, rateY * Time.deltaTime);
            _currentYaw = Mathf.Lerp(_currentYaw, _inputYaw, rateX * Time.deltaTime);
        }

        private void UpdateDistance()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            float distanceSensitivty = _currentSettings.distanceSensitivity;
            float distanceRate = _currentSettings.distanceRate;
            float minDistance = _currentSettings.minDistance;
            float maxDistance = _currentSettings.maxDistance;

            _inputDistance = Mathf.Clamp(_inputDistance - scrollDelta * distanceSensitivty, minDistance, maxDistance);
            _currentDistance = Mathf.Lerp(_currentDistance, _inputDistance, distanceRate * Time.deltaTime);
        }

        private void UpdateCamera()
        {
            Quaternion pitchRotation = Quaternion.AngleAxis(_currentPitch, _positionTarget.right);
            Quaternion yawRotation = Quaternion.AngleAxis(_currentYaw, _positionTarget.up);
            Quaternion combinedRotation = yawRotation * pitchRotation;
            Vector3 translationDirection = combinedRotation * _positionTarget.forward;
            Vector3 cameraPosition = _positionTarget.position + translationDirection * _currentDistance;
            Vector3 lookDirection = _lookTarget.position - cameraPosition;

            _currentCamera.transform.position = cameraPosition;
            _currentCamera.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
