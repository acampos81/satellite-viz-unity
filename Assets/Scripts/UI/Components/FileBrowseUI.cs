using EphemerisDemo.IO;
using EphemerisDemo.Model;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.UI
{
    public class FileBrowseUI : MonoBehaviour
    {
        [Inject]
        private AppViewModel _appViewModel;

        public void BrowseLocal()
        {
            _appViewModel.FileBrowse(BrowseType.Local);
        }

        public void BrowseDownload()
        {
            _appViewModel.FileBrowse(BrowseType.Download);
        }
    }
}
