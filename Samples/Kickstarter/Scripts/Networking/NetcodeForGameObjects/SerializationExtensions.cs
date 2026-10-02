using System;
using Unity.Netcode;

namespace HnSF
{
    public static class SerializationExtensions
    {
        public static void ReadValueSafe(this FastBufferReader reader, out Guid guid)
        {
            reader.ReadValueSafe(out byte[] val);
            guid = new Guid(val);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in Guid guid)
        {
            writer.WriteValueSafe(guid.ToByteArray());
        }
    }
}