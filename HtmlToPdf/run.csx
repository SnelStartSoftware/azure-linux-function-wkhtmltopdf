using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

    log.Info($"Creating temp files");

    var guid = Guid.NewGuid();
    var input = GetTempInputFileLocation(guid, html);
    var output = Path.GetTempPath() + guid + ".pdf";
    
    byte[] pdf = null;
    try
    {
        var executableLocation = "/home/site/wwwroot/HtmlToPdf/wkhtmltopdf";
        var wkHtmlToPdf = new Process();
        wkHtmlToPdf.StartInfo.FileName = executableLocation;
        wkHtmlToPdf.StartInfo.Arguments = $"--javascript-delay 0 {input} {output}";
        wkHtmlToPdf.StartInfo.UseShellExecute = false;

        wkHtmlToPdf.Start();
        wkHtmlToPdf.WaitForExit();

        pdf = File.ReadAllBytes(output);
    }
    catch (Exception exception)
    {
        log.Error("PDF generation failed due to an unknown error", exception);
    }
    finally
    {
        log.Info("Cleaning up temp files");

        Cleanup(new[] { input, output });
    }

    if (pdf == null)
    {
        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
    }

    var response = new HttpResponseMessage();
    response.Content = new ByteArrayContent(pdf);
    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
    response.StatusCode = HttpStatusCode.OK;
    
    log.Info("Done with function execution");

    return response;
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