/*
 * Copyright (c) 2024-2025 XDay
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace XDay
{
    public static class Common
    {
        public static int ToInt32(List<byte> list, int offset = 0)
        {
            return list[offset + 3] | (list[offset + 2] << 8) | (list[offset + 1] << 16) | (list[offset + 0] << 24);
        }

        public static int ToInt32(byte[] list, int offset = 0)
        {
            return list[offset + 3] | (list[offset + 2] << 8) | (list[offset + 1] << 16) | (list[offset + 0] << 24);
        }

        public static void WriteInt32(byte[] bytes, int val, int offset = 0)
        {
            bytes[offset + 0] = (byte)((val >> 24) & 0xff);
            bytes[offset + 1] = (byte)((val >> 16) & 0xff);
            bytes[offset + 2] = (byte)((val >> 8) & 0xff);
            bytes[offset + 3] = (byte)(val & 0xff);
        }

        public static uint DJB2Hash(string str)
        {
            long hash = 5381;
            foreach (var ch in str)
            {
                hash = ((hash << 5) + hash) + ch;
            }
            return (uint)hash;
        }

        public static void InterlockedSet(ref int target, int value)
        {
            int initial, computed;
            do
            {
                initial = target;
                computed = value;
            }
            while (Interlocked.CompareExchange(ref target, computed, initial) != initial);
        }

        public static Type[] QueryTypes<T>(bool isAbstract)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).Where(type => typeof(T).IsAssignableFrom(type) && type.IsAbstract == isAbstract).ToArray();
        }

        public static Type[] QueryTypes<T>(bool isAbstract, string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies) 
            {
                var name = asm.GetName().Name;
                if (name == assemblyName)
                {
                    return asm.GetTypes().Where(type => typeof(T).IsAssignableFrom(type) && type.IsAbstract == isAbstract).ToArray();
                }
            }
            return new Type[0];
        }
    }
}
