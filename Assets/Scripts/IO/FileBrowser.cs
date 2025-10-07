using SFB;
using UnityEngine;
using EphemerisDemo.DI;
using Zenject;

namespace EphemerisDemo.IO
{
    public class FileBrowser : MonoBehaviour, IDataLoader
    {
        // Regex to isolate the file name item in a full system file path
        //private const string _fileNamePattern = @"[^\\/]+$";

        [Inject]
        private SignalBus _signalBus;

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
                
                _signalBus.Fire(new ParseFileSignal { filePath = paths[0] });
            }
            else
            {
                Debug.LogWarning("No local files were selected.");
            }

            // Isolate the filename only for display
            //Regex fileNameRegex = new Regex(_fileNamePattern);
            //MatchCollection matches = fileNameRegex.Matches(firstFileName);
        }
    }
}