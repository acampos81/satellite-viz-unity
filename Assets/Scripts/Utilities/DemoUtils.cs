using EphemerisDemo.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EphemerisDemo.Utilities
{
    public static class DemoUtils
    {
        private const string _fileNamePattern = @"[^\\/]+$";
        public const float EquatorialDiameterKm = 12756f;

        /// <summary>
        /// Converts global coordinate system radians to combined quaternion rotation
        /// </summary>
        /// <param name="gcsRadians">x is latitude, y is longitude</param>
        /// <returns></returns>
        public static Quaternion GcsRadiansToRotation(Vector2 gcsRadians)
        {
            var gcsDegrees = new Vector2
            {
                x = gcsRadians.x * Mathf.Rad2Deg,
                y = gcsRadians.y * Mathf.Rad2Deg
            };
            var latitudeRotation = Quaternion.AngleAxis(gcsDegrees.x, Vector3.forward);
            var longitudeRotation = Quaternion.AngleAxis(-gcsDegrees.y, Vector3.up);
            return longitudeRotation * latitudeRotation;
        }

        /// <summary>
        /// Converts global coordinate system radians to position on earth's surface
        /// </summary>
        /// <param name="gcsRadians">x is latitude, y is longitude</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Vector3 GcsRadiansToPosition(Vector2 gcsRadians, float scale)
        {
            Quaternion rotation = GcsRadiansToRotation(gcsRadians);
            float scaledEarthRadius = EquatorialDiameterKm * scale * 0.5f;
            return rotation * Vector3.right * scaledEarthRadius;
        }

        /// <summary>
        /// Converts a list of EphemerisRowData parsed directly from a .csv file to an array of DisplayData used for simulation
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static DisplayData[] ConvertToDisplayData(List<EphemerisRowData> rowData, float scale)
        {
            var displayData = new DisplayData[rowData.Count];
            for (int i = 0; i < rowData.Count; i++)
            {
                EphemerisRowData eData = rowData[i];

                float nextDataInterval = 0f;
                if (i < rowData.Count - 1)
                {
                    long currentMs = eData.timeStamp.ToUnixTimeMilliseconds();
                    long nextMs = rowData[i + 1].timeStamp.ToUnixTimeMilliseconds();
                    float intervalMs = Convert.ToSingle(nextMs - currentMs);
                    nextDataInterval = intervalMs / 1000f;
                }

                DisplayData dData = new DisplayData
                {
                    ephemerisData = eData,
                    nextDataInterval = nextDataInterval,
                    scaledPositionKm = eData.eciPositionKm * scale,
                    scaledVelocityKmPs = eData.eciVelocityKmPs * scale,
                    scaledGcsPoint = DemoUtils.GcsRadiansToPosition(eData.gcsRadians, scale),
                };

                displayData[i] = dData;
            }

            return displayData;
        }

        /// <summary>
        /// Isolates the file name from a system file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileName(string filePath)
        {
            Regex fileNameRegex = new Regex(_fileNamePattern);
            MatchCollection matches = fileNameRegex.Matches(filePath);
            return matches[0].Value;
        }
    }
}
