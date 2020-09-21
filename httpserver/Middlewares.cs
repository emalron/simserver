using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Text;

namespace httpserver
{
    public class Middlewares
    {
        public delegate void HandlerFunc(HttpListenerContext c);
        public delegate HandlerFunc Middleware(HandlerFunc next);
        private List<Middleware> middlewares;

        public Middlewares()
        {
            middlewares = new List<Middleware>();
        }

        public void init()
        {
            middlewares.Add(LogMiddleware);
            middlewares.Add(CorsMiddleware);
        }

        public List<Middleware> GetMiddlewares()
        {
            return middlewares;
        }

        public HandlerFunc LogMiddleware(HandlerFunc next)
        {
            return (context) =>
            {
                var rq = context.Request;
                var method = rq.HttpMethod;
                var pattern = rq.Url.LocalPath;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                next(context);
                stopwatch.Stop();
                var log = string.Format("Method: {0}, Pattern: {1} - {2} ms", method, pattern, stopwatch.ElapsedMilliseconds.ToString());
                Console.WriteLine(log);
            };
        }
        public HandlerFunc CorsMiddleware(HandlerFunc next)
        {
            return (context) =>
            {
                var response = context.Response;
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST, GET");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-type");
                if (context.Request.HttpMethod.Equals("OPTIONS"))
                {
                    response.StatusCode = 207;
                    Console.WriteLine("CORS Check");
                    response.OutputStream.Close();
                }
                else
                {
                    next(context);
                }
                
            };
        }

        public HandlerFunc AuthMiddleware (HandlerFunc next)
        {
            return (context) =>
            {
                next(context);
            };
        }
    }
}
