using System;
using TMPro;
using UnityEngine;

public class DataBar : MonoBehaviour
{
    [SerializeField] TMP_InputField _dateTimeField;
    [SerializeField] EciVector      _eciPostion;
    [SerializeField] EciVector      _eciVelocity;
    [SerializeField] TMP_InputField _latitudeField;
    [SerializeField] TMP_InputField _longitudeField;

    public void SetData(DisplayData displayData)
    {
        _dateTimeField.text = displayData.ephemerisData.timeStamp.Date.ToString("dd/MM/yyyy HH:mm:ss");
        _eciPostion.UpdateVector3(displayData.ephemerisData.eciPositionKm);
        _eciVelocity.UpdateVector3(displayData.ephemerisData.eciPositionKm);
        _latitudeField.text = displayData.ephemerisData.gcsRadians.x.ToString();
        _longitudeField.text = displayData.ephemerisData.gcsRadians.y.ToString();
    }

    public void InterpolateData(DisplayData from, DisplayData to, float lerpValue)
    {
        DateTimeOffset date = LerpDate(from.ephemerisData.timeStamp, to.ephemerisData.timeStamp, (double)lerpValue);
        Vector3 position = Vector3.Lerp(from.ephemerisData.eciPositionKm, to.ephemerisData.eciPositionKm, lerpValue);
        Vector3 velocity = Vector3.Lerp(from.ephemerisData.eciVelocityKmPs, to.ephemerisData.eciVelocityKmPs, lerpValue);
        float latitude = Mathf.Lerp(from.ephemerisData.gcsRadians.x, to.ephemerisData.gcsRadians.x, lerpValue);
        float longitude = Mathf.Lerp(from.ephemerisData.gcsRadians.y, to.ephemerisData.gcsRadians.y, lerpValue);

        _dateTimeField.text = date.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss");
        _eciPostion.UpdateVector3(position);
        _eciVelocity.UpdateVector3(velocity);
        _latitudeField.text = latitude.ToString();
        _longitudeField.text = longitude.ToString();
    }

    private DateTimeOffset LerpDate(DateTimeOffset a, DateTimeOffset b, double t)
    {
        long ticks = (long)(a.Ticks + (b.Ticks - a.Ticks) * t);
        return new DateTimeOffset(ticks, a.Offset);
    }
}
