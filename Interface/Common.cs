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
