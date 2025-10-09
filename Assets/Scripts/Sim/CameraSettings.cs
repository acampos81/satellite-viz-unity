using System;
using UnityEngine;

namespace EphemerisDemo.Sim
{
    [Serializable]
    public struct CameraSettings
    {
        public ViewMode viewMode;
        public Transform positionTarget;
        public Transform lookTarget;
        public Camera camera;
        public Vector2 orbitSensitivity;
        public Vector2 orbitRate;
        public float distanceSensitivity;
        public float distanceRate;
        public float minDistance;
        public float maxDistance;
        public float defaultDistance;
        public float pitchDefault;
        public float pitchMin;
        public float pitchMax;
        public float yawDefault;
        public float yawMin;
        public float yawMax;
        public bool showSatellite;
        public bool showIcon;
    }
}