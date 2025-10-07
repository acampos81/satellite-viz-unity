using EphemerisDemo.UI;
using System;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.DI
{
    public class MainInstaller : MonoInstaller<MainInstaller>
    {
        [SerializeField] private Main _main;
        [SerializeField] private FileSelectorDialog _fileSelector;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<FileListReadySignal>();
            Container.DeclareSignal<FileSelectedSignal>();
            Container.DeclareSignal<ParseFileSignal>();

            Container.BindFactory<FileSelectorDialog, FileSelectorDialog.Factory>().FromComponentInNewPrefab(_fileSelector);

            //Container.Bind<IDataLoader>();
        }
    }
}