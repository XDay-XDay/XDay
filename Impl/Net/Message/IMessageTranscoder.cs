

namespace XDay
{
    public interface IMessagePipeline
    {
        byte[] Encode(object msg);
        object Decode(byte[] data, int offset, int count);
    }

    public interface IMessageTranscoder
    {
        byte[] Encode(object msg);
        void Input(byte[] data, int count);
        object Decode();
    }
}
