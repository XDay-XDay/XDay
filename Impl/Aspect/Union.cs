/*
 * Copyright (c) 2024-2025 XDay
 *
 * Permission is hereby granted, free of charge, to union person obtaining
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

#if PLATFORM_UNITY
using UnityEngine;
#endif


namespace XDay
{
    public enum UnionType
    {
        Undefined = -1,
        Color,
        Quaternion,
        Vector2,
        Vector3,
        Vector4,
        Enum,
        Array,
        Boolean,
        Object,
        Single,
        String,
        UInt32,
        Int32,
        UInt64,
        Int64,
    }

    public class Union
    {
        public UnionType Type => m_Type;

        private Union()
        {
        }

        public static Union CreateEmpty()
        {
            return new Union();
        }

#if PLATFORM_UNITY
        public static Union FromColor(Color value)
        {
            var union = new Union();
            union.SetColor(value);
            return union;
        }

        public static Union FromQuaternion(Quaternion value)
        {
            var union = new Union();
            union.SetQuaternion(value);
            return union;
        }

        public static Union FromVector2(Vector2 value)
        {
            var union = new Union();
            union.SetVector2(value);
            return union;
        }

        public static Union FromVector3(Vector3 value)
        {
            var union = new Union();
            union.SetVector3(value);
            return union;
        }

        public static Union FromVector4(Vector4 value)
        {
            var union = new Union();
            union.SetVector4(value);
            return union;
        }
#endif

        public static Union FromEnum<T>(T value) where T : System.Enum
        {
            var union = new Union();
            union.SetEnum(value);
            return union;
        }

        public static Union FromArray<T>(T[] value, bool makeCopy)
        {
            var union = new Union();
            if (makeCopy)
            {
                var copy = value.Clone() as T[];
                value = copy;
            }
            union.SetArray(value);
            return union;
        }

        public static Union FromBoolean(bool value)
        {
            var union = new Union();
            union.SetBoolean(value);
            return union;
        }

        public static Union FromObject(object value)
        {
            var union = new Union();
            union.SetObject(value);
            return union;
        }

        public static Union FromSingle(float value)
        {
            var union = new Union();
            union.SetSingle(value);
            return union;
        }

        public static Union FromUInt32(uint value)
        {
            var union = new Union();
            union.SetUInt32(value);
            return union;
        }

        public static Union FromUInt64(ulong value)
        {
            var union = new Union();
            union.SetUInt64(value);
            return union;
        }

        public static Union FromInt32(int value)
        {
            var union = new Union();
            union.SetInt32(value);
            return union;
        }

        public static Union FromInt64(long value)
        {
            var union = new Union();
            union.SetInt64(value);
            return union;
        }

        public static Union FromString(string value)
        {
            var union = new Union();
            union.SetString(value);
            return union;
        }

#if PLATFORM_UNITY
        public void SetColor(Color value)
        {
            m_Type = UnionType.Color;
            m_Value = value;
        }

        public Color GetColor()
        {
            if (m_Type == UnionType.Color)
            {
                return (Color)m_Value;
            }
            Debug.LogError("GetColor failed");
            return Color.white;
        }

        public void SetQuaternion(Quaternion value)
        {
            m_Type = UnionType.Quaternion;
            m_Value = value;
        }

        public Quaternion GetQuaternion()
        {
            if (m_Type == UnionType.Quaternion)
            {
                return (Quaternion)m_Value;
            }
            Debug.LogError("GetQuaternion failed");
            return Quaternion.identity;
        }

        public void SetVector2(Vector2 value)
        {
            m_Type = UnionType.Vector2;
            m_Value = value;
        }

        public Vector2 GetVector2()
        {
            if (m_Type == UnionType.Vector2)
            {
                return (Vector2)m_Value;
            }
            Debug.LogError("GetVector2 failed");
            return Vector2.zero;
        }

        public void SetVector3(Vector3 value)
        {
            m_Type = UnionType.Vector3;
            m_Value = value;
        }

        public Vector3 GetVector3()
        {
            if (m_Type == UnionType.Vector3)
            {
                return (Vector3)m_Value;
            }
            Debug.LogError("GetVector3 failed");
            return Vector3.zero;
        }

        public void SetVector4(Vector4 value)
        {
            m_Type = UnionType.Vector4;
            m_Value = value;
        }

        public Vector4 GetVector4()
        {
            if (m_Type == UnionType.Vector4)
            {
                return (Vector4)m_Value;
            }
            Debug.LogError("GetVector4 failed");
            return Vector4.zero;
        }
#endif

        public void SetEnum<T>(T value) where T : System.Enum
        {
            m_Type = UnionType.Enum;
            m_Value = value;
        }

        public T GetEnum<T>() where T : System.Enum
        {
            if (m_Type == UnionType.Enum)
            {
                return (T)System.Enum.ToObject(typeof(T), m_Value);
            }

            Log.Instance?.Error($"GetEnum failed");
            return default;
        }

        public void SetArray<T>(T[] value)
        {
            m_Type = UnionType.Array;
            m_Value = value;
        }

        public T[] GetArray<T>()
        {
            if (m_Type == UnionType.Array)
            {
                return m_Value as T[];
            }

            Log.Instance?.Error($"GetArray failed");
            return null;
        }

        public void SetBoolean(bool value)
        {
            m_Type = UnionType.Boolean;
            m_Value = value;
        }

        public bool GetBoolean()
        {
            if (m_Type == UnionType.Boolean)
            {
                return System.Convert.ToBoolean(m_Value);
            }
            Log.Instance?.Error($"GetBoolean failed");
            return false;
        }

        public void SetSingle(float value)
        {
            m_Type = UnionType.Single;
            m_Value = value;
        }

        public float GetSingle()
        {
            if (m_Type == UnionType.Single)
            {
                return System.Convert.ToSingle(m_Value);
            }
            Log.Instance?.Error($"GetSingle failed");
            return 0;
        }

        public void SetObject(object value)
        {
            m_Type = UnionType.Object;
            m_Value = value;
        }

        public object GetObject()
        {
            if (m_Type == UnionType.Object)
            {
                return m_Value;
            }
            Log.Instance?.Error($"Get object failed!");
            return false;
        }

        public void SetString(string value)
        {
            m_Type = UnionType.String;
            m_Value = value;
        }

        public string GetString()
        {
            if (m_Type == UnionType.String)
            {
                return m_Value as string;
            }
            Log.Instance?.Error($"GetString failed");
            return "";
        }

        public void SetUInt32(uint value)
        {
            m_Type = UnionType.UInt32;
            m_Value = value;
        }

        public void SetUInt64(ulong value)
        {
            m_Type = UnionType.UInt64;
            m_Value = value;
        }

        public uint GetUInt32()
        {
            if (m_Type == UnionType.Int32)
            {
                return System.Convert.ToUInt32(m_Value);
            }
            Log.Instance?.Error($"GetUInt32 failed");
            return 0;
        }

        public ulong GetUInt64()
        {
            if (m_Type == UnionType.Int64)
            {
                return System.Convert.ToUInt64(m_Value);
            }
            Log.Instance?.Error($"GetUInt64 failed");
            return 0;
        }

        public void SetInt32(int value)
        {
            m_Type = UnionType.Int32;
            m_Value = value;
        }

        public void SetInt64(long value)
        {
            m_Type = UnionType.Int64;
            m_Value = value;
        }

        public int GetInt32()
        {
            if (m_Type == UnionType.Int32)
            {
                return System.Convert.ToInt32(m_Value);
            }
            Log.Instance?.Error($"GetInt32 failed");
            return 0;
        }

        public long GetInt64()
        {
            if (m_Type == UnionType.Int64)
            {
                return System.Convert.ToInt64(m_Value);
            }
            Log.Instance?.Error($"GetInt64 failed");
            return 0;
        }

        public int GetSize()
        {
            switch (m_Type)
            {
                case UnionType.Boolean:
                    return sizeof(bool);
                case UnionType.Enum:
                case UnionType.Int32:
                case UnionType.UInt32:
                    return sizeof(int);
                case UnionType.Single:
                    return sizeof(float);
                case UnionType.Vector2:
                    return sizeof(float) * 2;
                case UnionType.Vector3:
                    return sizeof(float) * 3;
                case UnionType.Vector4:
                case UnionType.Quaternion:
                case UnionType.Color:
                    return sizeof(float) * 4;
                case UnionType.Object:
                case UnionType.String:
                case UnionType.Array:
                    return 8;
                default:
                    Log.Instance?.Assert(false, $"Unknown value type {m_Type}");
                    break;
            }
            return 0;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.WriteInt32(m_Version, "Union.Version");
            serializer.WriteEnum(m_Type, "Type");
            switch (m_Type)
            {
#if PLATFORM_UNITY
                case UnionType.Color:
                    serializer.WriteColor(GetColor(), "Color Value");
                    break;
                case UnionType.Quaternion:
                    serializer.WriteQuaternion(GetQuaternion(), "Quaternion Value");
                    break;
                case UnionType.Vector2:
                    serializer.WriteVector2(GetVector2(), "Vector2 Value");
                    break;
                case UnionType.Vector3:
                    serializer.WriteVector3(GetVector3(), "Vector3 Value");
                    break;
                case UnionType.Vector4:
                    serializer.WriteVector4(GetVector4(), "Vector4 Value");
                    break;
#endif
                case UnionType.Boolean:
                    serializer.WriteBoolean(GetBoolean(), "Boolean Value");
                    break;
                case UnionType.Single:
                    serializer.WriteSingle(GetSingle(), "Single Value");
                    break;
                case UnionType.String:
                    serializer.WriteString(GetString(), "String Value");
                    break;
                case UnionType.UInt32:
                    serializer.WriteUInt32(GetUInt32(), "UInt32 Value");
                    break;
                case UnionType.Int32:
                    serializer.WriteInt32(GetInt32(), "Int32 Value");
                    break;
                default:
                    Log.Instance?.Error($"can't serialize type {m_Type}");
                    break;
            }
        }

        public void Deserialize(IDeserializer deserializer)
        {
            deserializer.ReadInt32("Union.Version");
            m_Type = deserializer.ReadEnum<UnionType>("Type");
            switch (m_Type)
            {
#if PLATFORM_UNITY
                case UnionType.Color:
                    SetColor(deserializer.ReadColor("Color Value"));
                    break;
                case UnionType.Quaternion:
                    SetQuaternion(deserializer.ReadQuaternion("Quaternion Value"));
                    break;
                case UnionType.Vector2:
                    SetVector2(deserializer.ReadVector2("Vector2 Value"));
                    break;
                case UnionType.Vector3:
                    SetVector3(deserializer.ReadVector3("Vector3 Value"));
                    break;
                case UnionType.Vector4:
                    SetVector4(deserializer.ReadVector4("Vector4 Value"));
                    break;
#endif
                case UnionType.Boolean:
                    SetBoolean(deserializer.ReadBoolean("Boolean Value"));
                    break;
                case UnionType.Single:
                    SetSingle(deserializer.ReadSingle("Single Value"));
                    break;
                case UnionType.String:
                    SetString(deserializer.ReadString("String Value"));
                    break;
                case UnionType.UInt32:
                    SetUInt32(deserializer.ReadUInt32("UInt32 Value"));
                    break;
                case UnionType.Int32:
                    SetInt32(deserializer.ReadInt32("Int32 Value"));
                    break;
                default:
                    Log.Instance?.Error($"can't deserialize type {m_Type}");
                    break;
            }
        }

        public static bool operator !=(Union a, Union b)
        {
            return !(a == b);
        }

        public static bool operator ==(Union a, Union b)
        {
            if ((a is null && b is not null) ||
                (a is not null && b is null)
                )
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a.m_Type != b.m_Type)
            {
                return false;
            }

            switch (a.m_Type)
            {
#if PLATFORM_UNITY
                case UnionType.Color:
                    return a.GetColor() == b.GetColor();
                case UnionType.Quaternion:
                    return a.GetQuaternion() == b.GetQuaternion();
                case UnionType.Vector2:
                    return a.GetVector2() == b.GetVector2();
                case UnionType.Vector3:
                    return a.GetVector3() == b.GetVector3();
                case UnionType.Vector4:
                    return a.GetVector4() == b.GetVector4();
#endif
                case UnionType.Enum:
                    return a.m_Value.Equals(b.m_Value);
                case UnionType.Array:
                    return false;
                case UnionType.Object:
                    return a.GetObject() == b.GetObject();
                case UnionType.Boolean:
                    return a.GetBoolean() == b.GetBoolean();
                case UnionType.Single:
                    return a.GetSingle() == b.GetSingle();
                case UnionType.String:
                    return a.GetString() == b.GetString();
                case UnionType.UInt32:
                    return a.GetUInt32() == b.GetUInt32();
                case UnionType.Int32:
                    return a.GetInt32() == b.GetInt32();
                default:
                    Log.Instance?.Assert(false, $"unkpnwn type {a.m_Type}");
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return m_Value.GetHashCode() ^ m_Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as Union);
        }

        private object m_Value;
        private UnionType m_Type = UnionType.Undefined;
        private const int m_Version = 1;
    }
}

//XDay