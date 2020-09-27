using Cloo;
using System;
using System.Collections.Generic;
using System.Text;

namespace httpserver
{
    public class Gpgpu
    {
        public string vectorSum()
        {
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
            for (int i = 0; i < size; i++)
            {
                sb.AppendFormat("{0} + {1} = {2}<br>", v1_[i].ToString(), v2_[i].ToString(), v3_[i].ToString());
            }
            var sum_expression_result = sb.ToString();
            return sum_expression_result;
        }
    }
}
