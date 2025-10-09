using UnityEngine;

namespace EphemerisDemo.Sim
{
    public class SatelliteTracker : MonoBehaviour
    {
        private GameObject _satelliteObject;

        public GameObject Satellite => _satelliteObject;

        // Update is called once per frame
        void LateUpdate()
        {
            if(_satelliteObject != null)
            {
                transform.position = _satelliteObject.transform.position;
                transform.rotation = _satelliteObject.transform.rotation;
            }
        }

        public void ShowSatellite(bool isVisible)
        {
            _satelliteObject?.SetActive(isVisible);
        }

        public void TrackSatellite(GameObject satelliteObject)
        {
            _satelliteObject = satelliteObject;
        }
    }
}
