using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class MemoryStreamEx
{
    public static void Reset(this System.IO.MemoryStream stream)
    {
        stream.Position = 0;
        stream.SetLength(0);
    }
}

public static class FormatUtil
{
    private static MemoryStream stream = new MemoryStream();

    public static byte[] SerializeToBinary(object obj)
    {
        stream.Reset();
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, obj);
        byte[] data = stream.ToArray();
        stream.Close();
        return data;
    }

    public static object DeserializeWithBinary(byte[] data)
    {
        stream.Reset();
        stream.Write(data, 0, data.Length);
        stream.Position = 0;
        BinaryFormatter formatter = new BinaryFormatter();
        object obj = formatter.Deserialize(stream);
        stream.Close();
        return obj;
    }

    public static void Release()
    {
        if(stream != null)
        {
            stream.Close();
            stream = null;
        }
    }

}
