using System.Net;
using System.Net.Http;

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
    
    // Html to PDF generation here

    log.Info("Done with function execution");

    return new HttpResponseMessage(HttpStatusCode.OK);
}