using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class GetRequest : MonoBehaviour
{
    private static readonly HttpClient client = new HttpClient();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Task<string> fileListTask = Task.Run(async ()=> await GetFileList().ConfigureAwait(false));
            string result = fileListTask.Result;
        }
    }

    private async Task<string> GetFileList()
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            using HttpResponseMessage response = await client.GetAsync("https://j2000-ephemeris-files.s3.us-east-2.amazonaws.com/?list-type=2");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);

            return responseBody;
        }
        catch (HttpRequestException e)
        {
            return e.Message;
        }
    }
}
