using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EphemerisDemo.Sim
{
    public class SatelliteFactory : PlaceholderFactory<int, GameObject>
    {
        private readonly DiContainer _container;
        private readonly List<GameObject> _prefabs; 

        [Inject]
        public SatelliteFactory(DiContainer container, SatelliteLibrary prefabLibrary)
        {
            _container = container;
            _prefabs = prefabLibrary.satelliteObjects;
        }

        public override GameObject Create(int index)
        {
            if(index >= _prefabs.Count)
            {
                throw new ArgumentException($"Index {index} is out of list bounds. List count is {_prefabs.Count}");
            }

            return _container.InstantiatePrefab(_prefabs[index]);
        }
    }
}
