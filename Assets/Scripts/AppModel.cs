using System.Collections.Generic;
using EphemerisDemo.IO;
using EphemerisDemo.Sim;
using Zenject;

namespace EphemerisDemo.Model
{
    public class AppModel
    {
        public string CurrentFile { get; private set; } = string.Empty;

        private Dictionary<string, FileData> _fileDataByFileName;

        [Inject]
        public AppModel()
        {
            _fileDataByFileName = new Dictionary<string, FileData>();
        }

        public bool HasDataForFile(string fileName)
        {
            return _fileDataByFileName.ContainsKey(fileName);
        }

        internal void SetCurrentFile(string fileName)
        {
            CurrentFile = fileName;
        }

        /// <summary>
        /// Adds new data if it doesn't already exist in the app model
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="displayData"></param>
        /// <param name="timeControls"></param>
        /// <returns></returns>
        public void AddDataForFile(string fileName, DisplayData[] displayData, TimeControls timeControls)
        {
            var fileData = new FileData
            {
                fileName = fileName,
                displayData = displayData,
                timeControls = timeControls
            };

            _fileDataByFileName.Add(fileName, fileData);
        }

        /// <summary>
        /// Removes file data if it exists in the model
        /// </summary>
        /// <param name="fileName"></param>
        public void RemoveDataForFile(string fileName)
        {
            if(HasDataForFile(fileName))
            {
                FileData fileData = _fileDataByFileName[fileName];
                _fileDataByFileName.Remove(fileName);
            }
        }

        /// <summary>
        /// Returns data for the file name if it exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public FileData GetDisplayDataForFile(string fileName)
        {
            if(_fileDataByFileName.TryGetValue(fileName, out FileData fileData))
            {
                return fileData;
            }
            else
            {
                // TODO: log warning, or UI message instead?
                return default(FileData);
            }
        }
    }
}
