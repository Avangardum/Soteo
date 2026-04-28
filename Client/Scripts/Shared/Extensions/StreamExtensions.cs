namespace Soteo.Shared.Extensions;

public static class StreamExtensions
{
    extension (Stream self)
    {
        public int Read(Span<byte> buffer)
        {
            int bytesRead = 0;
            while (true)
            {
                int value = self.ReadByte();
                if (value == -1) return bytesRead;
                buffer[bytesRead] = (byte)value;
                bytesRead++;
            }
        }
        
        public void ReadExactly(Span<byte> buffer)
        {
            int bytesRead = self.Read(buffer);
            if (bytesRead != buffer.Length)
                throw new EndOfStreamException();
        }
        
        public byte ReadExactlyByte()
        {
            int value = self.ReadByte();
            if (value == -1)
                throw new EndOfStreamException();
            return (byte)value;
        }
        
        public void Write(ReadOnlySpan<byte> buffer)
        {
            foreach (byte b in buffer)
                self.WriteByte(b);
        }
    }
}