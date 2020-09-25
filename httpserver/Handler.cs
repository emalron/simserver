using Cloo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Text.Json;

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
            Dictionary<string, string> patterns = new Dictionary<string, string>();
            patterns.Add("name", "Jason");
            byte[] buffer = Utility.Parser(@"public/index.html", patterns);
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
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
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
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
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
            int size = 100000;
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
                Console.WriteLine("program build completed");
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
            Console.WriteLine("kernal created");
            sumKernal.SetMemoryArgument(0, v1);
            sumKernal.SetMemoryArgument(1, v2);
            sumKernal.SetMemoryArgument(2, v3);
            commands.Execute(sumKernal, null, worker, null, null);
            Console.WriteLine("Executed");
            commands.ReadFromBuffer<float>(v3, ref v3_, false, null);
            StringBuilder sb = new StringBuilder();
            for(int i=0; i<size; i++)
            {
                sb.AppendFormat("{0} + {1} = {2}<br>", v1_[i].ToString(), v2_[i].ToString(), v3_[i].ToString());
            }
            var sum_expression_result = sb.ToString();
            string responseString = string.Format("<html><body>{0}</body></html>", sum_expression_result);
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
        class Human
        {
            public string name { get; set; }
            public int age { get; set; }
        }
        public static void AjaxHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            var data = reader.ReadToEnd();
            Console.WriteLine(data);
            var human = JsonSerializer.Deserialize<Human>(data);
            human.name += " II";
            human.age += 2000;
            var responseString = JsonSerializer.Serialize(human);
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
        class OpenCLPOCO
        {
            public string code { get; set; }
            public float[] v1 { get; set; }
            public float[] v2 { get; set; }
        }
        public static void CustomHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString;
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            var data = reader.ReadToEnd();
            var props = JsonSerializer.Deserialize<OpenCLPOCO>(data);
            string vecSum = props.code;
            int size = props.v1.Length;
            float[] v1_ = props.v1;
            float[] v2_ = props.v2;
            float[] v3_ = new float[size];
            var platform_ = ComputePlatform.Platforms[0];
            ComputeContextPropertyList properties = new ComputeContextPropertyList(platform_);
            ComputeContext ctx = new ComputeContext(ComputeDeviceTypes.Gpu, properties, null, IntPtr.Zero);
            ComputeCommandQueue commands = new ComputeCommandQueue(ctx, ctx.Devices[0], ComputeCommandQueueFlags.None);
            ComputeProgram program = new ComputeProgram(ctx, vecSum);
            try
            {
                program.Build(null, null, null, IntPtr.Zero);
                Console.WriteLine("program build completed");
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
            try
            {
                ComputeKernel sumKernal = program.CreateKernel("vectorSum");
                Console.WriteLine("kernal created");
                sumKernal.SetMemoryArgument(0, v1);
                sumKernal.SetMemoryArgument(1, v2);
                sumKernal.SetMemoryArgument(2, v3);
                commands.Execute(sumKernal, null, worker, null, null);
                Console.WriteLine("Executed");
                commands.ReadFromBuffer<float>(v3, ref v3_, false, null);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < size; i++)
                {
                    sb.AppendFormat("Ω({0}, {1}) = {2}<br>", v1_[i].ToString(), v2_[i].ToString(), v3_[i].ToString());
                }
                var sum_expression_result = sb.ToString();
                responseString = string.Format("{0}", sum_expression_result);
            } catch(Exception e)
            {
                responseString = e.Message;
            }
            
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        class HumanPOCO
        {
            public string name { get; set; }
            public int money { get; set; }
        }
        class RoomPOCO
        {
            public string name { get; set; }
            public HumanPOCO[] humans { get; set; }
        }
        class HousePOCO
        {
            public string name { get; set; }
            public RoomPOCO[] rooms { get; set; }
        }
        public static void HouseHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString;
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            var data = reader.ReadToEnd();
            var house = JsonSerializer.Deserialize<HousePOCO>(data);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<div class=\"house\">Welcome House {0}<br>", house.name);
            string desc_room;
            switch(house.rooms.Length)
            {
                case 0:
                    desc_room = "no room";
                    break;
                case 1:
                    desc_room = "a room";
                    break;
                default:
                    desc_room = string.Format("{0} rooms", house.rooms.Length);
                    break;
            }
            sb.AppendFormat("<span class=\"house-desc\">This house has {0}</span><br>", desc_room);
            foreach (var room in house.rooms)
            {
                var room_name_ = room.name;
                string human_desc;
                switch (room.humans.Length)
                {
                    case 0:
                        human_desc = "No human";
                        break;
                    case 1:
                        human_desc = "A human";
                        break;
                    default:
                        human_desc = string.Format("{0} humans", room.humans.Length);
                        break;
                }
                sb.AppendFormat("<div class=\"room\">-<span class=\"room-name\">{0}</span>: {1}<br>", room_name_, human_desc);
                foreach(var human in room.humans)
                {
                    var human_name_ = human.name;
                    string money_desc;
                    switch(human.money)
                    {
                        case 0:
                            money_desc = "no money";
                            break;
                        default:
                            money_desc = string.Format("${0}", human.money);
                            break;
                    }
                    sb.AppendFormat("<div class=\"human\">+ <span class=\"human-name\">{0}</span> has {1}</span><br></div>", human_name_, money_desc);
                }
                sb.Append("</div>");
            }
            sb.Append("</div>");
            responseString = sb.ToString();
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
