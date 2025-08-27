using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Text;

public static class RequestHandler
{
    public static async Task HandleRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        Console.WriteLine($"{request.HttpMethod} {request.Url}");

        switch (request.HttpMethod)
        {
            case "GET":
                await HandleGetAsync(context);
                break;
            case "POST":
                await HandlePostAsync(context);
                break;
            case "PUT":
                await HandlePutAsync(context);
                break;
            case "DELETE":
                await HandleDeleteAsync(context);
                break;
            default:
                await HandleOtherAsync(context);
                break;
        }
    }

    public static async Task WriteResponseAsync(HttpListenerContext context, int delayMs = 0)
    {
        // Always send the currentPage as the response
        byte[] buffer = Encoding.UTF8.GetBytes(SimpleHttpServer.currentPage);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        if (delayMs > 0)
            await Task.Delay(delayMs);
        context.Response.OutputStream.Close();
    }

    public static async Task HandleGetAsync(HttpListenerContext context)
    {
        // Just redraw the current page
        await WriteResponseAsync(context, 2000);
    }

    public static async Task HandlePostAsync(HttpListenerContext context)
    {
        string postData;
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            postData = await reader.ReadToEndAsync();
        }
        PageState.UpdateCurrentPage($"<p>POST called. Payload:</p><pre>{WebUtility.HtmlEncode(postData)}</pre>");
        await WriteResponseAsync(context, 2000);
    }

    public static async Task HandlePutAsync(HttpListenerContext context)
    {
        string putData;
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            putData = await reader.ReadToEndAsync();
        }
        PageState.UpdateCurrentPage($"<p>PUT called. Payload:</p><pre>{WebUtility.HtmlEncode(putData)}</pre>");
        await WriteResponseAsync(context, 2000);
    }

    public static async Task HandleDeleteAsync(HttpListenerContext context)
    {
        PageState.UpdateCurrentPage("<p>DELETE called.</p>");
        await WriteResponseAsync(context, 2000);
    }

    public static async Task HandleOtherAsync(HttpListenerContext context)
    {
        PageState.UpdateCurrentPage($"<p>{context.Request.HttpMethod} called.</p>");
        await WriteResponseAsync(context, 2000);
    }
}