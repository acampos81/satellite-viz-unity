using EphemerisDemo.DI;
using EphemerisDemo.IO;
using EphemerisDemo.Sim;
using EphemerisDemo.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.Model
{
    public class AppViewModel : MonoBehaviour
    {
        [Inject]
        private SignalBus _signalBus;

        [Inject]
        private AppModel _appModel;

        [Inject]
        private FileLoaderFactory _fileLoaderFactory;

        [Inject]
        private ECIDataDisplay.Factory _eciDisplayFactory;

        [Inject]
        private TimeControls.Factory _timeControlsFactory;

        [Inject]
        private EarthObject _earthObject;

        void Start()
        {
            _signalBus.Subscribe<ParseFileSignal>(HandleParseFileSignal);
        }

        public void FileBrowse(BrowseType browseType)
        {
            IFileLoader loader = _fileLoaderFactory.Create(browseType);
            loader.BrowseForFile();
        }

        public void FileListReady(string[] fileList)
        {
            _signalBus.Fire(new FileListReadySignal { fileList = fileList });
        }

        public void HandleParseFileSignal(ParseFileSignal signal)
        {
            if (_appModel.HasDataForFile(signal.fileName) == false)
            {
                // Stop the current time controsl if a current file is set
                if (_appModel.HasDataForFile(_appModel.CurrentFile))
                {
                    FileData currentFileData = _appModel.GetDisplayDataForFile(_appModel.CurrentFile);
                    currentFileData.timeControls.Pause();
                }

                List<EphemerisRowData> dataRows = EphemerisParser.ParseFile(signal.filePath);
                DisplayData[] displayData = DemoUtils.ConvertToDisplayData(dataRows, _earthObject.GetSimScale());
                TimeControls timeControls = _timeControlsFactory.Create();
                timeControls.SetDisplayData(displayData);
                _appModel.SetCurrentFile(signal.fileName);
                _appModel.AddDataForFile(signal.fileName, displayData, timeControls);

                _signalBus.Fire(new DisplayDataReadySignal { fileName = signal.fileName });

                timeControls.Play();
            }
        }

        public void ChangeDisplayData(string fileName)
        {
            if (_appModel.HasDataForFile(fileName))
            {
                string currentFile = _appModel.CurrentFile;
                FileData currentFileData = _appModel.GetDisplayDataForFile(currentFile);
                currentFileData.timeControls.Pause();

                _appModel.SetCurrentFile(fileName);
                currentFileData = _appModel.GetDisplayDataForFile(fileName);
                currentFileData.timeControls.Play();

                _signalBus.Fire(new DisplayDataChangedSignal { fileName = fileName });
            }
        }

        public void RemoveDisplayData(string fileName)
        {
            if (_appModel.HasDataForFile(fileName))
            {
                FileData fileData = _appModel.GetDisplayDataForFile(fileName);
                _timeControlsFactory.Destory(fileData.timeControls);
                _appModel.RemoveDataForFile(fileName);
                _signalBus.Fire(new DisplayDataRemoveSignal { fileName = fileName });
            }
        }

        public void SetTimeScale(float newValue)
        {
            if(_appModel.HasDataForFile(_appModel.CurrentFile))
            {
                FileData fileData = _appModel.GetDisplayDataForFile(_appModel.CurrentFile);
                fileData.timeControls.ModifyTimeScale(newValue);
            }
        }

        public void SetPlayPause(bool isPlaying)
        {
            if (_appModel.HasDataForFile(_appModel.CurrentFile))
            {
                FileData fileData = _appModel.GetDisplayDataForFile(_appModel.CurrentFile);
                if(isPlaying) fileData.timeControls.Play();
                if(!isPlaying) fileData.timeControls.Pause();
            }
        }

        public void ModifyPathIndex(int amount)
        {
            if (_appModel.HasDataForFile(_appModel.CurrentFile))
            {
                FileData fileData = _appModel.GetDisplayDataForFile(_appModel.CurrentFile);
                fileData.timeControls.ModifyPathIndex(amount);
            }
        }
    }
}