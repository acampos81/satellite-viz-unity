using EphemerisDemo.IO;
using EphemerisDemo.UI;
using EphemerisDemo.Sim;
using EphemerisDemo.Model;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.DI
{
    public class MainInstaller : MonoInstaller<MainInstaller>
    {
        [SerializeField] private AppViewModel _appViewModel;
        [SerializeField] private OrbitControls _orbitControls;
        [SerializeField] private EarthObject _earthObject;
        [SerializeField] private ECIDataDisplay _eciDataDisplay;
        [SerializeField] private FileSelectorDialog _fileSelector;
        [SerializeField] private SatelliteLibrary _satelliteLibrary;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<FileListReadySignal>();
            Container.DeclareSignal<FileSelectedSignal>();
            Container.DeclareSignal<ParseFileSignal>();
            Container.DeclareSignal<DisplayDataReadySignal>();
            Container.DeclareSignal<DisplayDataRemoveSignal>();
            Container.DeclareSignal<DisplayDataChangedSignal>();
            Container.DeclareSignal<SetViewModeSignal>();

            Container.Bind<AppModel>().AsSingle();
            Container.Bind<AppViewModel>().FromInstance(_appViewModel).AsSingle();

            Container.Bind<IFileLoader>().WithId(BrowseType.Local).To<FileBrowserLocal>().AsSingle();
            Container.Bind<IFileLoader>().WithId(BrowseType.Download).To<FileDownloader>().AsSingle();
            Container.BindFactory<BrowseType, IFileLoader, FileLoaderFactory>()
                .FromMethod((container, type) => container.ResolveId<IFileLoader>(type));

            Container.Bind<OrbitControls>().FromInstance(_orbitControls).AsSingle();
            Container.Bind<EarthObject>().FromInstance(_earthObject).AsSingle();
            Container.BindInstance(_satelliteLibrary).AsSingle();

            Container.BindFactory<FileSelectorDialog, FileSelectorDialog.Factory>().FromComponentInNewPrefab(_fileSelector);
            Container.BindFactory<ECIDataDisplay, ECIDataDisplay.Factory>().FromComponentInNewPrefab(_eciDataDisplay);
            Container.BindFactory<int, GameObject, SatelliteFactory>();
            Container.BindFactory<TimeControls, TimeControls.Factory>().AsTransient();

        }
    }
}