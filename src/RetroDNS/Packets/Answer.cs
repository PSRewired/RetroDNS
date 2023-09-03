using System.Buffers.Binary;
using System.Net;
using System.Text;
using RetroDNS.Attributes;
using RetroDNS.Extensions;
using Serilog;

namespace RetroDNS.Packets;

public class Answer
{
    public required string Name { get; set; }
    public required IPAddress Domain { get; set; }
    public ushort QuestionType { get; set; } = 1;
    public ushort QuestionClass { get; set; } = 1;
    public uint Ttl { get; set; } = 1;

    public byte[] Build()
    {
        using var stream = new MemoryStream();

        Span<byte> shortSpan = stackalloc byte[2];
        Span<byte> intSpan = stackalloc byte[4];
        var urlChunks = Name.Split('.');

        for (var i = 0; i < urlChunks.Length; i++)
        {
            var chunk = urlChunks[i];

            stream.WriteByte((byte)chunk.Length);
            stream.Write(Encoding.ASCII.GetBytes(chunk));

            /*
            if (i < urlChunks.Length - 1)
            {
                stream.WriteByte((byte)'.');
            }
            */
        }
        stream.WriteByte(0);// Add null byte

        Log.Debug("Answer domain:\n{Domain}", stream.ToArray().ToHexDump());

        BinaryPrimitives.WriteUInt16BigEndian(shortSpan, QuestionType);
        stream.Write(shortSpan);

        BinaryPrimitives.WriteUInt16BigEndian(shortSpan, QuestionClass);
        stream.Write(shortSpan);

        BinaryPrimitives.WriteUInt32BigEndian(intSpan, Ttl);
        stream.Write(intSpan);

        var addressBytes = Domain.GetAddressBytes();
        BinaryPrimitives.WriteUInt16BigEndian(shortSpan, (ushort)addressBytes.Length);
        stream.Write(shortSpan);
        stream.Write(addressBytes);

        return stream.ToArray();
    }

    public static Question FromBytes(byte[] buffer)
    {
        using var reader = new BinaryReader(new MemoryStream(buffer));
        var sb = new StringBuilder();

        var currentOffset = 0;

        do
        {
            var size = reader.ReadByte();

            // Break out if we hit the null byte
            if (size < 1)
            {
                // Remove the final '.' from the url
                sb.Remove(sb.Length - 1, 1);
                break;
            }

            sb.Append(Encoding.ASCII.GetString(reader.ReadBytes(size)));
            sb.Append('.');
            currentOffset += size + 1;
        } while (true);


        return new Question
        {
            Domain = sb.ToString(),
            QuestionType = BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(sizeof(ushort))),
            QuestionClass = BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(sizeof(ushort))),
        };
    }
}
