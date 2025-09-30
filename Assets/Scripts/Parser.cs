using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class Parser
{
    public static List<EphemerisRowData> ParseEphemerisData(string filePath)
    {
        List<EphemerisRowData> dataRows = new List<EphemerisRowData>();
        string sourceFolder = Path.Combine(Application.dataPath, "Data");

        CsvHelper.Configuration.CsvConfiguration csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
        csvConfig.Delimiter = ",";
        csvConfig.AllowComments = true;
        csvConfig.Encoding = Encoding.UTF8;
        csvConfig.PrepareHeaderForMatch = args => args.Header.ToLowerInvariant();  // force all the csv headers to lowercase

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true))
        using (CsvReader csv = new CsvReader(sr, csvConfig))
        {
            csv.Read();
            csv.ReadHeader();  // first line is always the header line
            while (csv.Read())
            {
                var posixSec = double.Parse(csv.GetField<string>("utc_posix_sec"));
                var timeStamp = DateTimeOffset.FromUnixTimeMilliseconds((long)Math.Round(posixSec * 1000d));
                var posX = (float)double.Parse(csv.GetField<string>("eci_pos_x_km"));
                var posY = (float)double.Parse(csv.GetField<string>("eci_pos_y_km"));
                var posZ = (float)double.Parse(csv.GetField<string>("eci_pos_z_km"));
                var velX = (float)double.Parse(csv.GetField<string>("eci_vel_x_kmps"));
                var velY = (float)double.Parse(csv.GetField<string>("eci_vel_y_kmps"));
                var velZ = (float)double.Parse(csv.GetField<string>("eci_vel_z_kmps"));
                var lat = (float)double.Parse(csv.GetField<string>("latitude_rad"));
                var lon = (float)double.Parse(csv.GetField<string>("longitude_rad"));

                EphemerisRowData rowData = new EphemerisRowData
                {
                    timeStamp = timeStamp,
                    eciPositionKm = new Vector3(posX, posY, posZ),
                    eciVelocityKmPs = new Vector3(velX, velY, velZ),
                    gcsRadians = new Vector2(lat, lon)
                };

                dataRows.Add(rowData);
            }
        }
        
        return dataRows;
    }
}