using System.Buffers.Binary;
using System.Text;

namespace RetroDNS.Attributes;

public class Question
{
    public string Domain { get; set; } = default!;
    public ushort QuestionType { get; set; } = 1;
    public ushort QuestionClass { get; set; } = 1;

    public byte[] Build()
    {
        using var stream = new MemoryStream();

        Span<byte> shortSpan = stackalloc byte[2];
        var urlChunks = Domain.Split('.');

        for (var i = 0; i < urlChunks.Length; i++)
        {
            var chunk = urlChunks[i];

            if (i < urlChunks.Length - 1)
            {
                chunk = $"{chunk}.";
            }

            stream.WriteByte((byte)(chunk.Length + 1));
            stream.Write(shortSpan);

            stream.Write(Encoding.ASCII.GetBytes(chunk));
        }
        stream.WriteByte(0);// Add null byte

        BinaryPrimitives.WriteUInt16BigEndian(shortSpan, QuestionType);
        stream.Write(shortSpan);

        BinaryPrimitives.WriteUInt16BigEndian(shortSpan, QuestionClass);
        stream.Write(shortSpan);

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
