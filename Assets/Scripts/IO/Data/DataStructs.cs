using EphemerisDemo.Sim;
using System;
using UnityEngine;

namespace EphemerisDemo.IO
{
    public struct EphemerisRowData
    {
        public DateTimeOffset timeStamp;
        public Vector3 eciPositionKm;
        public Vector3 eciVelocityKmPs;
        public Vector2 gcsRadians;
    }

    public struct DisplayData
    {
        public EphemerisRowData ephemerisData;
        public float nextDataInterval;
        public Vector3 scaledPositionKm;
        public Vector3 scaledVelocityKmPs;
        public Vector3 scaledGcsPoint;
    }

    public struct FileData
    {
        public string fileName;
        public DisplayData[] displayData;
        public TimeControls timeControls;
    }
}