using EphemerisDemo.Utilities;
using UnityEngine;

namespace EphemerisDemo
{
    public class EarthObject : MonoBehaviour
    {
        /// <summary>
        /// Provides the scale of the simulation coordinate space basd on the local scale of the earth game object
        /// </summary>
        /// <returns></returns>
        public float GetSimScale()
        {
            return transform.localScale.x / DemoUtils.EquatorialDiameterKm;
        }
    }
}
