using EphemerisDemo.DI;
using EphemerisDemo.IO;
using EphemerisDemo.Model;
using EphemerisDemo.Sim;
using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.UI
{
    public class DataBarUI : MonoBehaviour
    {
        [SerializeField] TMP_InputField _dateTimeField;
        [SerializeField] EciVector      _eciPostion;
        [SerializeField] EciVector      _eciVelocity;
        [SerializeField] TMP_InputField _latitudeField;
        [SerializeField] TMP_InputField _longitudeField;

        [Inject]
        private AppModel _appModel;

        [Inject]
        private SignalBus _signalBus;

        private TimeControls _timeControls;
        private DisplayData[] _displayData;
        private DisplayData _currentData;
        private DisplayData _nextData;

        private void Start()
        {
            _signalBus.Subscribe<DisplayDataReadySignal>(HandleDisplayDataReady);
            _signalBus.Subscribe<DisplayDataChangedSignal>(HandleDisplayDataChanged);
        }

        private void HandleDisplayDataReady(DisplayDataReadySignal signal)
        {
            InitializeData(signal.fileName);
        }

        private void HandleDisplayDataChanged(DisplayDataChangedSignal signal)
        {
            InitializeData(signal.fileName);
        }

        private void InitializeData(string fileName)
        {
            var fileData = _appModel.GetDisplayDataForFile(fileName);
            SetTimeControls(fileData.timeControls);
            _displayData = fileData.displayData;

            _currentData = _displayData[_timeControls.CurrentPathIndex];
            _nextData = _displayData[_timeControls.NextPathIndex];

            _dateTimeField.text = _currentData.ephemerisData.timeStamp.Date.ToString("dd/MM/yyyy HH:mm:ss");
            _eciPostion.UpdateVector3(_currentData.ephemerisData.eciPositionKm);
            _eciVelocity.UpdateVector3(_currentData.ephemerisData.eciPositionKm);
            _latitudeField.text = _currentData.ephemerisData.gcsRadians.x.ToString();
            _longitudeField.text = _currentData.ephemerisData.gcsRadians.y.ToString();
        }

        private void HandleIntervalElapsed()
        {
            _currentData = _displayData[_timeControls.CurrentPathIndex];
            _nextData = _displayData[_timeControls.NextPathIndex];
        }

        private void HandlePathIndexModified()
        {
            HandleIntervalElapsed();
            InterpolateData(_currentData, _nextData, 0);
        }

        private void SetTimeControls(TimeControls newTimeControls)
        {
            if (_timeControls != null)
            {
                _timeControls.IntervalElapsed -= HandleIntervalElapsed;
                _timeControls.PathIndexModified -= HandlePathIndexModified;
            }
            _timeControls = newTimeControls;
            _timeControls.IntervalElapsed += HandleIntervalElapsed;
            _timeControls.PathIndexModified += HandlePathIndexModified;
        }

        private void Update()
        {
            if(_timeControls != null && _timeControls.IsPaused == false)
            {
                InterpolateData(_currentData, _nextData, _timeControls.LerpValue);
            }
        }

        private void InterpolateData(DisplayData from, DisplayData to, float lerpValue)
        {
            DateTimeOffset date = LerpDate(from.ephemerisData.timeStamp, to.ephemerisData.timeStamp, (double)lerpValue);
            Vector3 position = Vector3.Lerp(from.ephemerisData.eciPositionKm, to.ephemerisData.eciPositionKm, lerpValue);
            Vector3 velocity = Vector3.Lerp(from.ephemerisData.eciVelocityKmPs, to.ephemerisData.eciVelocityKmPs, lerpValue);
            float latitude = Mathf.Lerp(from.ephemerisData.gcsRadians.x, to.ephemerisData.gcsRadians.x, lerpValue);
            float longitude = Mathf.Lerp(from.ephemerisData.gcsRadians.y, to.ephemerisData.gcsRadians.y, lerpValue);

            _dateTimeField.text = date.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            _eciPostion.UpdateVector3(position);
            _eciVelocity.UpdateVector3(velocity);
            _latitudeField.text = latitude.ToString();
            _longitudeField.text = longitude.ToString();
        }

        private DateTimeOffset LerpDate(DateTimeOffset a, DateTimeOffset b, double t)
        {
            long ticks = (long)(a.Ticks + (b.Ticks - a.Ticks) * t);
            return new DateTimeOffset(ticks, a.Offset);
        }
    }
}