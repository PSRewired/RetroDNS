using System.Buffers.Binary;
using System.Text;

namespace RetroDNS.Attributes;

public class Question
{
    public string Domain { get; set; } = default!;
    public ushort QuestionType { get; set; }
    public ushort QuestionClass { get; set; }
    
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