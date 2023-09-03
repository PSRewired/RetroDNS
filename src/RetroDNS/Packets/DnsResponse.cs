using RetroDNS.Attributes;
using RetroDNS.Extensions;

namespace RetroDNS.Packets;

public class DnsResponse
{
    public DnsHeader Header { get; set; }
    public Question Question { get; set; }
    public Answer Answer { get; set; }

    public byte[] Build()
    {
        using var stream = new MemoryStream();

        var header = Header with { QuestionCount = 0, AnswerCount = 1, AdditionalCount = 0, NameserverCount = 0, Parameters = 0x8580};

        stream.Write(header.Serialize());
        //stream.Write(Question.Build());
        stream.Write(Answer.Build());

        return stream.ToArray();
    }
}
