using EphemerisDemo.DI;
using EphemerisDemo.UI;
using UnityEngine;
using Zenject;

public class FrontPanel : MonoBehaviour
{
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
        fileSelector.transform.SetParent(transform, worldPositionStays:false);
        fileSelector.DisplayFileList(signal.fileList);
    }
}
