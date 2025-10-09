using EphemerisDemo.DI;
using EphemerisDemo.IO;
using EphemerisDemo.Model;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.Sim
{
    public class SimeViewModel : MonoBehaviour
    {
        [SerializeField]
        private Transform _eciDisplayParent;

        [SerializeField]
        private Transform _iconParent;

        [Inject]
        private AppModel _appModel;

        [Inject]
        private SignalBus _signalBus;

        [Inject]
        private ECIDataDisplay.Factory _eciDisplayFactory;

        [Inject]
        private SatelliteFactory _satelliteFactory;

        [Inject]
        private OrbitControls _orbitControls;

        [Inject]
        private EarthObject _earthObject;

        private Dictionary<string, ECIDataDisplay> _eciDisplayByFileName;

        private void Start()
        {
            _eciDisplayByFileName = new Dictionary<string, ECIDataDisplay>();

            _signalBus.Subscribe<DisplayDataReadySignal>(HandleDisplayDataReady);
            _signalBus.Subscribe<DisplayDataRemoveSignal>(HandleDisplayDataRemove);
            _signalBus.Subscribe<DisplayDataChangedSignal>(HandleDisplayDataChanged);
            _signalBus.Subscribe<SetViewModeSignal>(HandleSetViewMode);
        }

        private void HandleDisplayDataReady(DisplayDataReadySignal signal)
        {
            FileData fileData = _appModel.GetDisplayDataForFile(signal.fileName);
            ECIDataDisplay eciDisplay = _eciDisplayFactory.Create();
            GameObject satellite = _satelliteFactory.Create(0);

            eciDisplay.transform.SetParent(_eciDisplayParent);
            eciDisplay.Initialize(fileData.displayData, fileData.timeControls, satellite);
            eciDisplay.Icon.SatelliteTransform = satellite.transform;
            eciDisplay.Icon.transform.SetParent(_iconParent, worldPositionStays: false);
            _eciDisplayByFileName[signal.fileName] = eciDisplay;

            _earthObject.AddGCSPoints(signal.fileName, fileData.displayData);
            _earthObject.Initialize(fileData.timeControls, fileData.displayData);

            _orbitControls.SetECIDisplay(eciDisplay);
        }

        private void HandleDisplayDataChanged(DisplayDataChangedSignal signal)
        {
            if(_eciDisplayByFileName.TryGetValue(signal.fileName, out ECIDataDisplay eciDisplay))
            {
                FileData fileData = _appModel.GetDisplayDataForFile(signal.fileName);
                _earthObject.SetTimeControls(fileData.timeControls);
                _earthObject.SetDisplayData(fileData.displayData);
                _orbitControls.SetECIDisplay(eciDisplay);
            }
        }

        private void HandleDisplayDataRemove(DisplayDataRemoveSignal signal)
        {
            if(_eciDisplayByFileName.TryGetValue(signal.fileName, out ECIDataDisplay eciDisplay))
            {
                eciDisplay.Dispose();
            }

            _earthObject.RemoveGCSPoints(signal.fileName);
        }

        private void HandleSetViewMode(SetViewModeSignal signal)
        {
            _orbitControls.SetViewMode(signal.viewMode);
        }
    }
}
