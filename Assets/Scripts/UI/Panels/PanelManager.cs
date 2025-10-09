using EphemerisDemo.DI;
using EphemerisDemo.IO;
using EphemerisDemo.Model;
using EphemerisDemo.UI;
using UnityEngine;
using Zenject;

public class PanelManager : MonoBehaviour
{
    [SerializeField]
    private Transform _ephermesisPanel;

    [SerializeField]
    private Transform _dialogPanel;

    [SerializeField]
    private GameObject _satelliteIconPrefab;

    [Inject]
    private SignalBus _signalBus;

    [Inject]
    private FileSelectorDialog.Factory _fileSelectorFactory;

    private void Start()
    {
        _signalBus.Subscribe<FileListReadySignal>(HandleFileListReady);
    }

    private void HandleFileListReady(FileListReadySignal signal)
    {
        FileSelectorDialog fileSelector = _fileSelectorFactory.Create();
        fileSelector.transform.SetParent(_dialogPanel, worldPositionStays:false);
        fileSelector.DisplayFileList(signal.fileList);
    }
}
