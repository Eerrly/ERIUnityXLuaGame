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

    public static byte[] Serialize(object obj)
    {
        byte[] data = null;
        try
        {
            stream.Reset();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            data = stream.ToArray();
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
        finally
        {
            stream.Close();
        }
        return data;
    }

    public static object Deserialize(byte[] data)
    {
        object obj = null;
        try
        {
            stream.Reset();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            obj = formatter.Deserialize(stream);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
        finally
        {
            stream.Close();
        }
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
