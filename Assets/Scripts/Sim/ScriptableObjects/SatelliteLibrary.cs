using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SatelliteLibrary", menuName = "ECI Demo/Satellite Library")]
public class SatelliteLibrary : ScriptableObject
{
    [SerializeField]
    public List<GameObject> satelliteObjects;
}
