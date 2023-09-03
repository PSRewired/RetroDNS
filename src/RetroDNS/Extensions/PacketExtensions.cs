using System.Runtime.InteropServices;
using RetroDNS.Attributes;

namespace RetroDNS.Extensions;

public static class PacketExtensions
{
    public static T Deserialize<T>(this byte[] rawData) where T : struct
    {
        T result;

        RespectEndianness(typeof(T), rawData);

        GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

        try
        {
            IntPtr rawDataPtr = handle.AddrOfPinnedObject();
            result = (T)Marshal.PtrToStructure(rawDataPtr, typeof(T))!;
        }
        finally
        {
            handle.Free();
        }

        return result;
    }

    public static byte[] Serialize<T>(this T data) where T : struct
    {
        byte[] rawData = new byte[Marshal.SizeOf(data)];
        GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        try
        {
            IntPtr rawDataPtr = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(data, rawDataPtr, false);
        }
        finally
        {
            handle.Free();
        }

        RespectEndianness(typeof(T), rawData);

        return rawData;
    }

    private static void RespectEndianness(Type type, byte[] data)
    {
        var fields = type.GetFields().Where(f => f.IsDefined(typeof(EndiannessAttribute), false))
            .Select(f => new
            {
                Field = f,
                Attribute = (EndiannessAttribute)f.GetCustomAttributes(typeof(EndiannessAttribute), false)[0],
                Offset = Marshal.OffsetOf(type, f.Name).ToInt32()
            }).ToList();

        foreach (var field in fields)
        {
            if ((field.Attribute.Endianness == EndiannessAttribute.EndiannessType.Big && BitConverter.IsLittleEndian) ||
                (field.Attribute.Endianness == EndiannessAttribute.EndiannessType.Little &&
                 !BitConverter.IsLittleEndian))
            {
                Array.Reverse(data, field.Offset, Marshal.SizeOf(field.Field.FieldType));
            }
        }
    }
}