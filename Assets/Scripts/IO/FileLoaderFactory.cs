using System;
using Zenject;

namespace EphemerisDemo.IO
{
    public class FileLoaderFactory : PlaceholderFactory<BrowseType, IFileLoader>
    {
        private readonly DiContainer _container;

        [Inject]
        public FileLoaderFactory(DiContainer container)
        {
            _container = container;
        }

        public override IFileLoader Create(BrowseType param)
        {
            switch(param)
            {
                case BrowseType.Local: return _container.ResolveId<IFileLoader>(BrowseType.Local);
                case BrowseType.Download: return _container.ResolveId<IFileLoader>(BrowseType.Download);
                default: throw new ArgumentException($"BrowseType {nameof(param)} not recognized!");
            }
        }
    }
}
