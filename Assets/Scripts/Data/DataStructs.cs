using System;
using UnityEngine;

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