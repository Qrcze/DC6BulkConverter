using System.Diagnostics;

namespace DC6BulkConverter
{
    public readonly struct DC6Image
    {
        public DC6Header Header { get; }
        public int[] Pointers { get; }
        public DC6FrameHeader[] Frames { get; }

        public DC6Image(DC6Header header)
        {
            Header = header;
            uint framesCount = header.Directions * header.FramesPerDir;
            Pointers = new int[framesCount];
            Frames = new DC6FrameHeader[framesCount];
        }

        public static DC6Image Read(string filePath)
        {
            var reader = new ByteReader(filePath);

            var header = new DC6Header(reader);
            var img = new DC6Image(header);

            for (int i = 0; i < img.Frames.Length; i++)
            {
                img.Pointers[i] = reader.ReadInt32();
            }

            for (int i = 0; i < img.Frames.Length; i++)
            {
                var frameHeader = new DC6FrameHeader(reader);
                img.Frames[i] = frameHeader;

                //extra 3-4 bytes that i have no idea how to actually determine
                int nextPointer = (i + 1 >= img.Pointers.Length) ? reader.Length : img.Pointers[i + 1];
                int terminationBytesLen = nextPointer - reader.Position;
                Debug.Assert(terminationBytesLen == 3); // || terminationBytesLen == 4 (never seen it, so leaving this asset in case i find one)
                reader.Position += terminationBytesLen;
            }

            Debug.Assert(reader.Position == reader.Length);

            return img;
        }
    }
}