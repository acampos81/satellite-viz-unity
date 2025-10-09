using EphemerisDemo.DI;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.IO
{
    public class FileDownloader : IFileLoader, IDisposable
    {
        // HttpClient is intended to be instantiated once per application.
        private static readonly HttpClient _client = new HttpClient();

        private readonly SignalBus _signalBus;

        [Inject]
        public FileDownloader(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<FileSelectedSignal>(HandleFileSelected);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<FileSelectedSignal>(HandleFileSelected);
        }

        public void BrowseForFile()
        {
            // Fire and forget async function.
            // The async function will capture the synchronization context.
            _ = GetFileList();
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        private async Task GetFileList()
        {
            // capture the main thread context
            var context = SynchronizationContext.Current;

            try
            {
                using HttpResponseMessage response = await _client.GetAsync(IOConstants.ListURL).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string xmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("az", "http://s3.amazonaws.com/doc/2006-03-01/");

                // select all the "Key" descendant elements in the xml doc
                XmlNodeList nodeList = xmlDoc.SelectNodes("//az:Contents/az:Key", nsmgr);

                string[] fileList = new string[nodeList.Count];
                for(int i=0; i<nodeList.Count; i++)
                {
                    fileList[i] = nodeList[i].InnerText;
                }

                // SignalBus is not thread-safe, so marshall it back to the main thread to fire.
                context.Post(_ => _signalBus.Fire(new FileListReadySignal { fileList = fileList }), null);
            }
            catch (HttpRequestException e)
            {
                LogError(e.Message);
            }
        }

        private void HandleFileSelected(FileSelectedSignal signal)
        {
            _ = DownloadFile(signal.fileName, IOConstants.DownloadPath);
        }

        private async Task DownloadFile(string fileName, string destinationPath)
        {
            // capture the main thread context
            var context = SynchronizationContext.Current;

            try
            {
                string fileURL = $"{IOConstants.StorageURL}/{fileName}";
                using HttpResponseMessage response = await _client.GetAsync(fileURL, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                // Ensure directory exists
                if(Directory.Exists(destinationPath) == false)
                {
                    Directory.CreateDirectory(destinationPath);
                }

                string filePath = Path.Combine(destinationPath, fileName);
                await using (Stream cs = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    await using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await cs.CopyToAsync(fs).ConfigureAwait(false);
                    }
                }

                // SignalBus is not thread-safe, so marshall it back to the main thread to fire.
                context.Post(_ => _signalBus.Fire(new ParseFileSignal { fileName = fileName, filePath = filePath }), null);
            }
            catch (HttpRequestException e)
            {
                LogError(e.Message);
            }
        }
    }
}
