using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Zenject;
using EphemerisDemo.DI;

namespace EphemerisDemo.IO
{
    public class FileDownloader : MonoBehaviour, IFileLoader
    {
        // HttpClient is intended to be instantiated once per application.
        private static readonly HttpClient _client = new HttpClient();

        [Inject]
        private SignalBus _signalBus;

        private void Start()
        {
            _signalBus.Subscribe<FileSelectedSignal>(HandleFileSelected);
        }

        public void BrowseForFile()
        {
            StartCoroutine(WaitForFileList());
        }

        private IEnumerator WaitForFileList()
        {
            Task<string[]> getListTask = GetFileList();
            yield return new WaitUntil(()=>getListTask.IsCompleted);

            if (getListTask.IsFaulted)
            {
                // Rethrow the wrapped exception from the task
                throw getListTask.Exception.InnerException;
            }
            else
            {
                // Fire signal to show the file selector
                _signalBus.Fire(new FileListReadySignal { fileList = getListTask.Result });
            }
        }


        private async Task<string[]> GetFileList()
        {
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

                return fileList;
            }
            catch (HttpRequestException e)
            {
                throw new Exception(e.Message);
            }
        }

        private void HandleFileSelected(FileSelectedSignal signal)
        {
            StartCoroutine(WaitForDownload(signal.fileName, IOConstants.DownloadPath));
        }

        private IEnumerator WaitForDownload(string fileName, string destinationPath)
        {
            Task<string> downloadTask = DownloadFile(fileName, destinationPath);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            if (downloadTask.IsFaulted)
            {
                // Rethrow the wrapped exception from the task
                throw downloadTask.Exception.InnerException;
            }
            else
            {
                // Fire signal to parse the downloaded file
                _signalBus.Fire(new ParseFileSignal { fileName = fileName, filePath = downloadTask.Result });
            }
        }

        private async Task<string> DownloadFile(string fileName, string destinationPath)
        {
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

                return filePath;
            }
            catch (HttpRequestException e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
