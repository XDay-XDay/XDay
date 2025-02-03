


namespace XDay
{
    //no compression
    public class CompressionStageForward : ICompressionStage
    {
        public void Compress(ByteStream input, ByteStream output)
        {
            input.WriteTo(output, 0);
            output.Position = 0;
        }

        public void Decompress(ByteStream input, ByteStream output)
        {
            input.WriteTo(output, 0);
            output.Position = 0;
        }
    }
}


//XDay