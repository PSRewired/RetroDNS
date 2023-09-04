namespace RetroDNS.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class EndiannessAttribute : Attribute
{
    public enum EndiannessType
    {
        Big,
        Little,
    }
    
    public EndiannessType Endianness { get; }


    public EndiannessAttribute(EndiannessType endianness)
    {
        Endianness = endianness;
    }
}