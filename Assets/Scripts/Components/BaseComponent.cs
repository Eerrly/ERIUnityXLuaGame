
using System.Text;
using System.Collections;
using System.Reflection;

/// <summary>
/// 组件基类
/// </summary>
public class BaseComponent
{
    public override string ToString() {
        PropertyInfo[] props = GetType().GetProperties();
        StringBuilder sb = new StringBuilder();
        foreach (PropertyInfo info in props)
        {
            object value = info.GetValue(this);
            if (value != null)
            {
                if (value.GetType().IsArray)
                {
                    sb.AppendFormat("\t\t{0}:\n", info.Name);
                    foreach (var a in (IEnumerable)value)
                    {
                        sb.AppendFormat("\t\t\t{0}\n", a);
                    }
                }
                else
                {
                    sb.AppendFormat("\t\t{0}:{1}\n", info.Name, value.ToString());
                }
            }
        }
        sb.Append("\n");
        return sb.ToString();
    }
}
