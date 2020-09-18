using Cloo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace httpserver
{
    public class Handler
    {
        public static void TestHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = "<html><body>You are in Test page.</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static void PlainHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = "<html><body>Hello, world!</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
        public static void NoPageHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = "<html><body>404! No page! >_<</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
        public static void OpenCLHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            
            var devices =
                from platform in ComputePlatform.Platforms
                from device in platform.Devices
                select device.Name;
            var content = "no device >_<";
            bool isDevice = devices.Count() > 0;
            if(isDevice)
            {
                content = devices.Aggregate((current, next) => current + string.Format("{0}<br>", next));
            }
            string responseString = string.Format("<html><body>machine info: {0}</body></html>", content);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
        public static void VectorSumHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string vecSum = @"
                __kernel void vectorSum(__global float *v1, __global float *v2, __global float *v3) {
                    int i = get_global_id(0);
                    v3[i] = v1[i] + v2[i];
                }
            ";
            int size = 100;
            float[] v1_ = new float[size];
            float[] v2_ = new float[size];
            float[] v3_ = new float[size];
            for (var i = 0; i < size; i++)
            {
                v1_[i] = (float)i;
                v2_[i] = (float).5f;
            }
            var platform_ = ComputePlatform.Platforms[0];
            ComputeContextPropertyList properties = new ComputeContextPropertyList(platform_);
            ComputeContext ctx = new ComputeContext(ComputeDeviceTypes.Gpu, properties, null, IntPtr.Zero);
            ComputeCommandQueue commands = new ComputeCommandQueue(ctx, ctx.Devices[0], ComputeCommandQueueFlags.None);
            ComputeProgram program = new ComputeProgram(ctx, vecSum);
            try
            {
                program.Build(null, null, null, IntPtr.Zero);
            }
            catch
            {
                string log = program.GetBuildLog(ctx.Devices[0]);
            }
            ComputeBuffer<float> v1, v2, v3;
            v1 = new ComputeBuffer<float>(ctx, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, v1_);
            v2 = new ComputeBuffer<float>(ctx, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, v2_);
            v3 = new ComputeBuffer<float>(ctx, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, v3_);
            long[] worker = { size };
            commands.WriteToBuffer(v1_, v1, false, null);
            commands.WriteToBuffer(v2_, v2, false, null);
            ComputeKernel sumKernal = program.CreateKernel("vectorSum");
            sumKernal.SetMemoryArgument(0, v1);
            sumKernal.SetMemoryArgument(1, v2);
            sumKernal.SetMemoryArgument(2, v3);
            commands.Execute(sumKernal, null, worker, null, null);
            commands.ReadFromBuffer<float>(v3, ref v3_, false, null);
            var test = v1_
                .Zip(v2_, (x, y) => string.Format("{0} + {1} = ", x.ToString(), y.ToString()))
                .Zip(v3_, (x, y) => string.Format("{0} {1}", x, y.ToString()));
            var result = v3_
                .Select(c => c.ToString())
                .Aggregate((current, next) => current + string.Format("<br>{0}", next));
            var result2 = test
                .Aggregate((current, next) => current + string.Format("<br>{0}", next));
            string responseString = string.Format("<html><body>{0}</body></html>", result2);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
