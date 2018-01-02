#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    dynamic data = await req.Content.ReadAsAsync<object>();
    string name = data?.name;

    if (name == null)
    {
        return new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.BadRequest,
                           Content = new StringContent("Please pass a name in the request body")
                       };
    }

    return new HttpResponseMessage(HttpStatusCode.OK);
}