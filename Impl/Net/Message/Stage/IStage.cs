

using System;

namespace XDay
{
    public interface IProtocalStage
    {
        void OnDestroy() { }
        void Encode(object protocal, ByteStream output);
        object Decode(ByteStream input);
        void RegisterProtocal(Type type);
        void RegisterProtocal<T>() where T : class, new();
    }

    public interface IProtocalStageCreator
    {
        IProtocalStage Create();
    }

    public interface ICompressionStage
    {
        void OnDestroy() { }
        void Compress(ByteStream input, ByteStream output);
        void Decompress(ByteStream input, ByteStream output);
    }

    public interface ICompressionStageCreator
    {
        ICompressionStage Create();
    }

    public interface IEncryptionStage
    {
        void OnDestroy() { }
        int Encrypt(ByteStream input, ByteStream output);
        int Decrypt(ByteStream input, ByteStream output);
    }

    public interface IEncryptionStageCreator
    {
        IEncryptionStage Create(object data);
    }
}

 