using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class SimpleHttpServer
{
    // Global page state
    static string currentPage = "";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Select server mode:");
        Console.WriteLine("1 - Normal (unthreaded)");
        Console.WriteLine("2 - Threaded (each request handled in a new thread)");
        Console.Write("Enter choice (1 or 2): ");
        string choice = Console.ReadLine() ?? "1";

        // Initialize currentPage with landing page or default
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "html", "index.html");
        if (File.Exists(filePath))
            currentPage = await File.ReadAllTextAsync(filePath);
        else
            currentPage = "<html><body><h1>Welcome!</h1></body></html>";

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
#pragma warning disable CS4014
                Task.Run(() => HandleRequestAsync(context));
#pragma warning restore CS4014
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

    static async Task WriteResponseAsync(HttpListenerContext context, int delayMs = 0)
    {
        // Always send the currentPage as the response
        byte[] buffer = Encoding.UTF8.GetBytes(currentPage);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        if (delayMs > 0)
            await Task.Delay(delayMs);
        context.Response.OutputStream.Close();
    }

    static async Task HandleGetAsync(HttpListenerContext context)
    {
        // Just redraw the current page
        await WriteResponseAsync(context, 2000);
    }

    static async Task HandlePostAsync(HttpListenerContext context)
    {
        string postData;
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            postData = await reader.ReadToEndAsync();
        }
        UpdateCurrentPage($"<p>POST called. Payload:</p><pre>{WebUtility.HtmlEncode(postData)}</pre>");
        await WriteResponseAsync(context, 2000);
    }

    static async Task HandlePutAsync(HttpListenerContext context)
    {
        string putData;
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            putData = await reader.ReadToEndAsync();
        }
        UpdateCurrentPage($"<p>PUT called. Payload:</p><pre>{WebUtility.HtmlEncode(putData)}</pre>");
        await WriteResponseAsync(context, 2000);
    }

    static async Task HandleDeleteAsync(HttpListenerContext context)
    {
        UpdateCurrentPage("<p>DELETE called.</p>");
        await WriteResponseAsync(context, 2000);
    }

    static async Task HandleOtherAsync(HttpListenerContext context)
    {
        UpdateCurrentPage($"<p>{context.Request.HttpMethod} called.</p>");
        await WriteResponseAsync(context, 2000);
    }

    // Helper to update the current page with a new message, preserving the rest of the page
    static void UpdateCurrentPage(string messageHtml)
    {
        // Try to inject the message into the body, or just append if not found
        if (currentPage.Contains("</body>"))
        {
            int idx = currentPage.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            currentPage = currentPage.Insert(idx, messageHtml);
        }
        else
        {
            currentPage += messageHtml;
        }
    }
}