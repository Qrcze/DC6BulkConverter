namespace DC6BulkConverter
{
    public readonly struct DC6Header
    {
        public uint Version { get; }      // 06 00 00 00
        public uint Unknown1 { get; }     // 01 00 00 00
        public uint Unknown2 { get; }     // 00 00 00 00
        public uint Termination { get; }  // EE EE EE EE or CD CD CD CD
        public uint Directions { get; }   // xx 00 00 00
        public uint FramesPerDir { get; } // xx 00 00 00

        public DC6Header(ByteReader reader)
        {
            Version = reader.ReadUInt32();
            Unknown1 = reader.ReadUInt32();
            Unknown2 = reader.ReadUInt32();
            Termination = reader.ReadUInt32();
            Directions = reader.ReadUInt32();
            FramesPerDir = reader.ReadUInt32();
        }
    }
}