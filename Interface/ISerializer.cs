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

using System.Collections.Generic;
using System.IO;
#if PLATFORM_UNITY
using UnityEngine;
#endif

namespace XDay
{
    public interface IObjectIDConverter
    {
        int Convert(int id);
    }

    public class PassID : IObjectIDConverter
    {
        public int Convert(int id)
        {
            return id;
        }
    }

    public interface ISerializer
    {
        static ISerializer CreateBinary(Stream outputStream = null)
        {
            return new BinarySerializer(outputStream);
        }

        static ISerializer CreateFile(ISerializer serializer, string filePath)
        {
            return new FileSerializer(serializer, filePath);
        }

        byte[] Data { get; }
        string Text { get; }

        void Uninit();

        void WriteInt32(int value, string label);
        void WriteUInt32(uint value, string label);
        void WriteInt64(long value, string label);
        void WriteUInt64(ulong value, string label);
        void WriteInt32Array(int[] value, string label);
        void WriteUInt32Array(uint[] value, string label);
        void WriteUInt16Array(ushort[] value, string label);
        void WriteByteArray(byte[] value, string label);
        void WriteSingleArray(float[] value, string label);
        void WriteStringArray(string[] value, string label);
        void WriteInt32List(List<int> value, string label);
        void WriteStringList(List<string> value, string label);
        void WriteSingleList(List<float> value, string label);
        void WriteSingle(float value, string label);
        void WriteBoolean(bool value, string label);
        void WriteBooleanArray(bool[] value, string label);
        void WriteString(string value, string label);
        void WriteByte(byte value, string label);

#if PLATFORM_UNITY
        void WriteVector2(Vector2 value, string label);
        void WriteVector3(Vector3 value, string label);
        void WriteVector2Int(Vector2Int value, string label);
        void WriteVector3Int(Vector3Int value, string label);
        void WriteVector4(Vector4 value, string label);
        void WriteQuaternion(Quaternion value, string label);
        void WriteBounds(Bounds bounds, string label);
        void WriteRect(Rect rect, string label);
        void WriteColor(Color color, string label);
        void WriteColor32(Color32 color, string label);
        void WriteColorArray(Color[] value, string label);
        void WriteColor32Array(Color32[] value, string label);
        void WriteVector2Array(Vector2[] value, string label);
        void WriteVector3Array(Vector3[] value, string label);
        void WriteVector4Array(Vector4[] value, string label);
        void WriteVector2List(List<Vector2> value, string label);
        void WriteVector3List(List<Vector3> value, string label);
        void WriteVector4List(List<Vector4> value, string label);
        void WriteVector2IntArray(Vector2Int[] value, string label);
        void WriteVector3IntArray(Vector3Int[] value, string label);
        void WriteVector2IntList(List<Vector2Int> value, string label);
        void WriteVector3IntList(List<Vector3Int> value, string label);
#endif

        void WriteEnum<T>(T value, string label) where T : System.Enum;
        void WriteList<T>(List<T> values, string label, System.Action<T, int> writeListElement);
        void WriteArray<T>(T[] values, string label, System.Action<T, int> writeArrayElement);
        void WriteStructure(string label, System.Action writeFunc);
        void WriteObjectID(int id, string label, IObjectIDConverter converter);
        void WriteSerializable(ISerializable serializable, string label, IObjectIDConverter converter, bool gameData);
    }

    public interface IDeserializer
    {
        static IDeserializer CreateBinary(Stream stream, ISerializableFactory creator)
        {
            return new BinaryDeserializer(stream, creator);
        }

        void Uninit();
        int ReadInt32(string label, int missingValue = default);
        uint ReadUInt32(string label, uint missingValue = default);
        long ReadInt64(string label, long missingValue = default);
        ulong ReadUInt64(string label, ulong missingValue = default);
        float ReadSingle(string label, float missingValue = default);
        bool ReadBoolean(string label, bool missingValue = default);
        bool[] ReadBooleanArray(string label);
        byte ReadByte(string label, byte missingValue = default);
        string ReadString(string label, string missingValue = default);

#if PLATFORM_UNITY
        Vector2 ReadVector2(string label, Vector2 missingValue = default);
        Vector3 ReadVector3(string label, Vector3 missingValue = default);
        Vector4 ReadVector4(string label, Vector4 missingValue = default);
        Vector2Int ReadVector2Int(string label, Vector2Int missingValue = default);
        Vector3Int ReadVector3Int(string label, Vector3Int missingValue = default);
        Quaternion ReadQuaternion(string label, Quaternion missingValue = default);
        Bounds ReadBounds(string label, Bounds missingValue = default);
        Rect ReadRect(string label, Rect missingValue = default);
        Color ReadColor(string label, Color missingValue = default);
        Color32 ReadColor32(string label, Color32 missingValue = default);
        Color[] ReadColorArray(string label);
        Color32[] ReadColor32Array(string label);
        Vector2[] ReadVector2Array(string label);
        Vector3[] ReadVector3Array(string label);
        Vector4[] ReadVector4Array(string label);
        List<Vector2> ReadVector2List(string label);
        List<Vector3> ReadVector3List(string label);
        List<Vector4> ReadVector4List(string label);
        Vector2Int[] ReadVector2IntArray(string label);
        Vector3Int[] ReadVector3IntArray(string label);
        List<Vector2Int> ReadVector2IntList(string label);
        List<Vector3Int> ReadVector3IntList(string label);
#endif

        T ReadEnum<T>(string label, T missingValue = default) where T : System.Enum;
        int[] ReadInt32Array(string label);
        uint[] ReadUInt32Array(string label);
        ushort[] ReadUInt16Array(string label);
        byte[] ReadByteArray(string label);
        float[] ReadSingleArray(string label);
        string[] ReadStringArray(string label);
        List<int> ReadInt32List(string label);
        List<string> ReadStringList(string label);
        List<float> ReadSingleList(string label);
        List<T> ReadList<T>(string label, System.Func<int, T> readListElement);
        T[] ReadArray<T>(string label, System.Func<int, T> readArrayElement);
        void ReadStructure(string label, System.Action readFunc);
        T ReadSerializable<T>(string label, bool gameData) where T : class, ISerializable;
    }

    public interface ISerializable
    {
        string TypeName { get; }
        string GameTypeName => TypeName;

        void EditorSerialize(ISerializer serializer, string label, IObjectIDConverter converter)
        {
            throw new System.NotImplementedException($"EditorSerialize {GetType().Name}");
        }
        void EditorDeserialize(IDeserializer deserializer, string label)
        {
            throw new System.NotImplementedException($"EditorDeserialize {GetType().Name}");
        }

        void GameSerialize(ISerializer serializer, string label, IObjectIDConverter converter)
        {
            throw new System.NotImplementedException($"GameSerialize {GetType().Name}");
        }
        void GameDeserialize(IDeserializer deserializer, string label)
        {
            throw new System.NotImplementedException($"GameDeserialize {GetType().Name}");
        }
    }

    public interface ISerializableFactory
    {
        static ISerializableFactory Create()
        {
            return new SerializableFactory();
        }

        ISerializable CreateObject(string typeName);
    }
}