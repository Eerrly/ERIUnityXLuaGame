
using System.Text;
using System.Collections;
using System.Reflection;

/// <summary>
/// 组件基类
/// </summary>
public class BaseComponent
{
    public override string ToString() {

        var fileds = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        StringBuilder sb = new StringBuilder();
        foreach (var filed in fileds)
        {
            object value = filed.GetValue(this);
            if (value != null)
            {
                if (value.GetType().IsGenericType)
                {
                    sb.AppendFormat("\t\t{0}:\n", filed.Name);
                    foreach (var a in (IEnumerable)value)
                    {
                        sb.AppendFormat("\t\t\t{0}\n", a);
                    }
                }
                else
                {
                    sb.AppendFormat("\t\t{0}:{1}\n", filed.Name, value.ToString());
                }
            }
        }
        sb.Append("\n");
        return sb.ToString();
    }
}
