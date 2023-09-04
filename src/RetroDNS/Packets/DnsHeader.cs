using System.Runtime.InteropServices;
using RetroDNS.Attributes;

namespace RetroDNS.Packets;

[StructLayout(LayoutKind.Sequential, Size = 12)]
public struct DnsHeader
{
    [Endianness(EndiannessAttribute.EndiannessType.Big)]
    public ushort TransactionId;

    [Endianness(EndiannessAttribute.EndiannessType.Big)]
    public ushort Parameters;

    [Endianness(EndiannessAttribute.EndiannessType.Big)]
    public ushort QuestionCount;

    [Endianness(EndiannessAttribute.EndiannessType.Big)]
    public ushort AnswerCount;

    [Endianness(EndiannessAttribute.EndiannessType.Big)]
    public ushort NameserverCount;

    [Endianness(EndiannessAttribute.EndiannessType.Big)]
    public ushort AdditionalCount;
}