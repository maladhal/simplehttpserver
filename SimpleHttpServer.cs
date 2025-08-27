using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;

public class SimpleHttpServer
{
    // Global page state
    public static string currentPage = "";

    public static async Task StartAsync()
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
                Task.Run(() => RequestHandler.HandleRequestAsync(context));
#pragma warning restore CS4014
            }
        }
        else
        {
            // Normal (unthreaded) mode
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                await RequestHandler.HandleRequestAsync(context);
            }
        }
    }
}