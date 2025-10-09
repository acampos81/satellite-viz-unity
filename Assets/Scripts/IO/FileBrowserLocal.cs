using SFB;
using UnityEngine;
using EphemerisDemo.DI;
using Zenject;
using EphemerisDemo.Utilities;

namespace EphemerisDemo.IO
{
    public class FileBrowserLocal : IFileLoader
    {
        private readonly SignalBus _signalBus;

        [Inject]
        public FileBrowserLocal(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void BrowseForFile()
        {
            var extensions = new[]
            {
                new ExtensionFilter("CSV File Filter", "csv") 
            };

            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a file", "", extensions, false);
            if (paths.Length > 0)
            {
                if (paths.Length > 1)
                {
                    Debug.LogWarning($"More than one file selected, defaulting to first file:{paths[0]}");
                }
                
                string filePath = paths[0];
                _signalBus.Fire(new ParseFileSignal { fileName = DemoUtils.GetFileName(filePath), filePath = filePath });
            }
            else
            {
                Debug.LogWarning("No local files were selected.");
            }
        }
    }
}