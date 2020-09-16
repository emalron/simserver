﻿using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;

namespace httpserver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string[] V = { "http://localhost:8080/" };
            Server server = new Server();
            Middlewares middlewares = new Middlewares();
            middlewares.init();

            server.init(prefixes: V);
            server.AddHandler("GET", "/", Handler.PlainHandler);
            server.AddHandler("GET", "/test", Handler.TestHandler);
            server.Use(middlewares.GetMiddlewares());
            server.Run();
        }
    }
}
