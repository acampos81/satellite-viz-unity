using EphemerisDemo.DI;
using EphemerisDemo.UI;
using UnityEngine;
using Zenject;

public class PanelManager : MonoBehaviour
{
    [SerializeField]
    private Transform _ephermesisPanel;

    [SerializeField]
    private Transform _dialogPanel;

    [Inject]
    private SignalBus _signalBus;

    [Inject]
    private FileSelectorDialog.Factory _fileSelectorFactory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
