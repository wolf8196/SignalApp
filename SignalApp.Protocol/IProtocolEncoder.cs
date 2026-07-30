using System.Buffers;

namespace SignalApp.Protocol
{
    public interface IProtocolEncoder
    {
        ProtocolRecord DecodeHeader(ReadOnlySequence<byte> buffer);

        ProtocolRecord DecodeData(ReadOnlySequence<byte> buffer);

        byte[] Encode(ProtocolRecord record);
    }
}