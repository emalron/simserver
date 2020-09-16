using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace httpserver
{
    class Server
    {

        private HttpListener listener;
        private Dictionary<string, Dictionary<string, Middlewares.HandlerFunc>> methods;
        private List<Middlewares.Middleware> middlewares;
        private Middlewares.HandlerFunc handler;

        public Server()
        {
            listener = new HttpListener();
            methods = new Dictionary<string, Dictionary<string, Middlewares.HandlerFunc>>();
        }

        ~Server()
        {
            if(listener.IsListening)
            {
                listener.Stop();
            }
        }

        public void init(string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use HttpListener class");
            }

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
                Console.Write(s);
            }
        }

        public void AddHandler(string method, string pattern, Middlewares.HandlerFunc handlerFunc)
        {
            var handlers = getHandlers(method);
            if (handlers == null)
            {
                methods[method] = new Dictionary<string, Middlewares.HandlerFunc>();
                handlers = methods[method];
            }
            handlers.Add(pattern, handlerFunc);
            string result = string.Format("added handler {0}, {1}", method, pattern);
            Console.WriteLine(result);
        }

        private Dictionary<string, Middlewares.HandlerFunc> getHandlers(string method)
        {
            if (methods.ContainsKey(method))
            {
                return methods[method];
            }
            return null;
        }

        public void Run()
        {
            listener.Start();
            Console.WriteLine("Listening...");
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                this.handler(context);
            }
        }

        public void Use(List<Middlewares.Middleware> middlewares)
        {
            this.middlewares = middlewares;
            setMiddleware();
        }

        private void setMiddleware()
        {
            this.handler = apiHandler;
            for(int i=this.middlewares.Count-1; i>=0; i--)
            {
                this.handler = this.middlewares[i](this.handler);
            }
        }

        private void apiHandler(HttpListenerContext c)
        {
            HttpListenerRequest request = c.Request;
            string method = request.HttpMethod;
            string path = request.Url.LocalPath;
            var handler = findHandler(method, path);
            bool isHandler = handler != null;
            if(isHandler)
            {
                handler(c);
                return;
            }
            Console.WriteLine("No handler");
            Handler.NoPageHandler(c);
            return;
        }

        private Middlewares.HandlerFunc findHandler(string method, string pattern)
        {
            var handlers = getHandlers(method);
            bool hasHandler = handlers != null && handlers.ContainsKey(pattern);
            if (hasHandler)
            {
                return handlers[pattern];
            }
            return null;
        }
    }
}
