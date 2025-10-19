using System.Buffers.Binary;

namespace DC6BulkConverter
{
    public class ByteReader
    {
        public int Position { get; set; }
        public int Length => _data.Length;
        private byte[] _data;

        public ByteReader(string filePath)
        {
            _data = File.ReadAllBytes(filePath);
        }

        public uint ReadUInt32()
        {
            uint result = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan().Slice(Position, 4));
            Position += 4;
            return result;
        }

        public int ReadInt32()
        {
            int result = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan().Slice(Position, 4));
            Position += 4;
            return result;
        }

        public uint[] ReadUInt32(uint bytes)
        {
            uint[] data = new uint[bytes / 4];
            for (int i = 0; i < data.Length; i++)
                data[i] = ReadUInt32();
            return data;
        }

        public byte ReadByte()
        {
            byte data = _data[Position];
            Position++;
            return data;
        }

        internal byte[] ReadBytes(int length)
        {
            byte[] data = _data[Position..(Position + length)];
            Position += length;
            return data;
        }
    }
}