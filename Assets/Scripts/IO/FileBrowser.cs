using SFB;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class FileBrowser : MonoBehaviour, IDataLoader
{
    private const string pattern = @"[^\\/]+$";

    public event Action<string> OnFileSelected;

    public TMP_Text fileField;

    public void BrowseForFile()
    {
        var extensions = new[]
        {
            new ExtensionFilter("CSV Files", "csv")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a file", "", extensions, false);

        if (paths.Length > 0)
        {
            string firstFileName = paths[0];
            OnFileSelected(firstFileName);

            // Isolate the filename only for display
            Regex fileNameRegex = new Regex(pattern);
            MatchCollection matches = fileNameRegex.Matches(firstFileName);
            fileField.text = matches[0].Value;
        }
        else
        {
            Debug.LogWarning($"More than one file selected, defaulting to first file:{paths[0]}");
        }
    }
}
