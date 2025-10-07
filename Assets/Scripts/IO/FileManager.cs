using System;
using Zenject;
using EphemerisDemo.DI;
using System.Collections.Generic;
using EphemerisDemo.Utilities;

namespace EphemerisDemo.IO
{
    public class FileManager : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;

        private readonly float _simScale;

        private Dictionary<string, FileData> _fileDataTable;

        [Inject]
        public FileManager(SignalBus signalBus, [Inject(Id = "SimScale")] float simScale)
        {
            _signalBus = signalBus;
            _simScale = simScale;
            _fileDataTable = new Dictionary<string, FileData>();
        }

        public void Initialize()
        {
            _signalBus.Subscribe<ParseFileSignal>(HandleParseFile);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ParseFileSignal>(HandleParseFile);
        }

        public void HandleParseFile(ParseFileSignal signal)
        {
            var fileName  = signal.fileName;
            var filePath = signal.filePath;
            var dataRows = EphemerisParser.ParseFile(filePath);

            var fileData = new FileData
            {
                fileName = signal.fileName,
                filePath = signal.filePath,
                displayData = DemoUtils.ConvertToDisplayData(dataRows, _simScale),
            };

            if(_fileDataTable.TryAdd(fileName, fileData))
            {
                // new data added
                _signalBus.Fire(new FileDataReadySignal { fileData = fileData });
            }
            else
            {
                // data already exists
            }
        }
    }
}
