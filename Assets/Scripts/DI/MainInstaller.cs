using EphemerisDemo.IO;
using EphemerisDemo.UI;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.DI
{
    public class MainInstaller : MonoInstaller<MainInstaller>
    {
        [SerializeField] private Main _main;
        [SerializeField] private EarthObject _earthObject;
        [SerializeField] private FileSelectorDialog _fileSelector;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<FileListReadySignal>();
            Container.DeclareSignal<FileSelectedSignal>();
            Container.DeclareSignal<ParseFileSignal>();
            Container.DeclareSignal<FileDataReadySignal>();

            Container.Bind<float>().WithId("SimScale").FromInstance(_earthObject.GetSimScale()).AsSingle();

            Container.BindFactory<FileSelectorDialog, FileSelectorDialog.Factory>().FromComponentInNewPrefab(_fileSelector);

            Container.BindInterfacesAndSelfTo<FileManager>().AsSingle();
        }
    }
}