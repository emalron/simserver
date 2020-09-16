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
                var log = string.Format("Method: {0}, Pattern: {1} - {2}", method, pattern, stopwatch.ElapsedMilliseconds.ToString());
                Console.WriteLine(log);
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
