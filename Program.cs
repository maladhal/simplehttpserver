using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class SimpleHttpServer
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Select server mode:");
        Console.WriteLine("1 - Normal (unthreaded)");
        Console.WriteLine("2 - Threaded (each request handled in a new thread)");
        Console.Write("Enter choice (1 or 2): ");
        string choice = Console.ReadLine();

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();
        Console.WriteLine("Server started at http://localhost:5000/");

        if (choice == "2")
        {
            // Threaded mode
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                Task.Run(() => HandleRequestAsync(context));
            }
        }
        else
        {
            // Normal (unthreaded) mode
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                await HandleRequestAsync(context);
            }
        }
    }

    static async Task HandleRequestAsync(HttpListenerContext context)
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

    static async Task WriteResponseAsync(HttpListenerContext context, string title, string comment, int delayMs = 0)
    {
        string autoRefreshMeta = "<meta http-equiv=\"refresh\" content=\"2\">";
        string responseString = $"<html><head>{autoRefreshMeta}</head><body><h1>{title}</h1>{comment}</body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        if (delayMs > 0)
            await Task.Delay(delayMs);
        context.Response.OutputStream.Close();
    }

    static async Task HandleGetAsync(HttpListenerContext context)
    {
        string comment = "<p>GET called.</p>";
        await WriteResponseAsync(context, "Hello from C# HTTP Server! (GET)", comment);
    }

    static async Task HandlePostAsync(HttpListenerContext context)
    {
        string postData;
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            postData = await reader.ReadToEndAsync();
        }
        string comment = $"<p>POST called. Payload:</p><pre>{WebUtility.HtmlEncode(postData)}</pre>";
        await WriteResponseAsync(context, "POST data received:", comment);
    }

    static async Task HandlePutAsync(HttpListenerContext context)
    {
        string putData;
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            putData = await reader.ReadToEndAsync();
        }
        string comment = $"<p>PUT called. Payload:</p><pre>{WebUtility.HtmlEncode(putData)}</pre>";
        Console.WriteLine($"Response: {putData}");
        await WriteResponseAsync(context, "PUT data received:", comment, 2000);
    }

    static async Task HandleDeleteAsync(HttpListenerContext context)
    {
        string comment = "<p>DELETE called.</p>";
        await WriteResponseAsync(context, "DELETE received", comment);
    }

    static async Task HandleOtherAsync(HttpListenerContext context)
    {
        string comment = $"<p>{context.Request.HttpMethod} called.</p>";
        await WriteResponseAsync(context, "Method not supported", comment);
    }
}