
using System.Text;
using System.Collections;
using System.Reflection;

public class BaseComponent
{
    public override string ToString() {
        var props = GetType().GetProperties();
        var sb = new StringBuilder();
        foreach (var info in props)
        {
            var value = info.GetValue(this);
            if (value == null) continue;
            
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
        sb.Append("\n");
        return sb.ToString();
    }
}
