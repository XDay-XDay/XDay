

namespace XDay
{
    //no encryption
    public class EncryptionStageForward : IEncryptionStage
    {
        public int Decrypt(ByteStream input, ByteStream output)
        {
            input.WriteTo(output, 0);
            output.Position = 0;
            return (int)input.Length;
        }

        public int Encrypt(ByteStream input, ByteStream output)
        {
            input.WriteTo(output, 0);
            output.Position = 0;
            return (int)input.Length;
        }
    }
}
