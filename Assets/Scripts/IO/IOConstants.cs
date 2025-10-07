using System.IO;
using UnityEngine;

namespace EphemerisDemo.IO
{
    public static class IOConstants
    {
        public const string StorageURL = "https://j2000-ephemeris-files.s3.us-east-2.amazonaws.com";
        public static readonly string ListURL = $"{StorageURL}/?list-type=2";
        public static readonly string DownloadPath = Path.Combine(Application.persistentDataPath, "EphemerisFiles");
    }
}
