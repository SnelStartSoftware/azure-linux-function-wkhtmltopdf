using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Starting function execution");

    var html = await req.Content.ReadAsStringAsync();

    if (string.IsNullOrWhiteSpace(html))
    {
        log.Warning("POST data was empty");

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("POST data must not be empty")
        };
    }

    log.Info($"Requested HTML: {html}");
    
    var guid = Guid.NewGuid();
    var input = GetTempInputFileLocation(guid, html);
    var output = Path.GetTempPath() + guid + ".pdf";
    
    // Html to PDF generation here
    
    Cleanup(new[] { input, output });

    log.Info("Done with function execution");

    return new HttpResponseMessage(HttpStatusCode.OK);
}

private static string GetTempInputFileLocation(Guid guid, string html)
{
    var tempLocation = Path.GetTempPath();
    var tempFileName = guid + ".html";

    var tempFile = File.CreateText(tempLocation + tempFileName);
    tempFile.Write(html);
    tempFile.Close();

    return tempLocation + tempFileName;
}

private static void Cleanup(string[] files)
{
    foreach (var file in files)
    {
        File.Delete(file);
    }
}