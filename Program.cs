﻿using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        await SimpleHttpServer.StartAsync();
    }
}