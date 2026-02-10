using Unity.Netcode;

namespace HnSF.sessionhandling.handlers.NGO
{
    public struct NetworkStringArray : INetworkSerializable
    {
        public string[] Array;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var length = 0;
            if (!serializer.IsReader)
                length = Array.Length;

            serializer.SerializeValue(ref length);

            if (serializer.IsReader)
                Array = new string[length];

            for (var n = 0; n < length; ++n)
                serializer.SerializeValue(ref Array[n]);
        }
    }
}