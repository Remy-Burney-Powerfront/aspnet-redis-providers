using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Web.Redis
{
    
    [ComVisible(false)]
    public class RedisASPSessionDispatch : IReflect
    {
        // Called by CLR to get DISPIDs and names for properties
        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return this.GetType().GetProperties(bindingAttr);
        }

        // Called by CLR to get DISPIDs and names for fields
        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return this.GetType().GetFields(bindingAttr);
        }

        // Called by CLR to get DISPIDs and names for methods
        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return this.GetType().GetMethods(bindingAttr);
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args,
            ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            return InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        // Called by CLR to invoke a member
        protected object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            try
            {
                //PFSession.LogEnter(name, invokeAttr, binder, target);
                /*
                System.IO.File.AppendAllText("d:\\log files\\test.log",
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + target.GetType().Name + ".InvokeMember:\r\n"
                    + "  name =  \"" + name + "\",\r\n"
                    + "  binding = \"" + invokeAttr.ToString() + "\",\r\n"
                    + "  target = \"" + target.GetType().Name + "\",\r\n"
                    + "  args = " + (args != null ? "[" + string.Join(", ", args.Select(a => a.ToString()).ToArray()) + "]" : "null") + ",\r\n\r\n");
                */
                // Test if it is the default method, return ToString
                if (name == "[DISPID=0]")
                {
                    if ((invokeAttr & BindingFlags.GetProperty) == BindingFlags.GetProperty)
                    {
                        if (args.Length == 0)
                            return this.GetType().GetMethod("ToString").Invoke(this, BindingFlags.InvokeMethod, null, new object[] { }, culture);
                        if (args.Length == 1)
                        {
                            object contents = this.GetType().GetProperty("Contents").GetValue(this);
                            return contents.GetType().GetMethod("Get").Invoke(contents, BindingFlags.InvokeMethod, null, args, culture);
                        }
                    }
                    else if ((invokeAttr & BindingFlags.PutDispProperty) == BindingFlags.PutDispProperty)
                    {
                        if (args.Length == 2)
                        {
                            object contents = this.GetType().GetProperty("Contents").GetValue(this);
                            return contents.GetType().GetMethod("Set").Invoke(contents, BindingFlags.InvokeMethod, null, args, culture);
                        }
                    }
                }
                if (name == "Contents")
                {
                    object contents = this.GetType().GetProperty("Contents").GetValue(this);
                    if ((invokeAttr & BindingFlags.GetProperty) == BindingFlags.GetProperty)
                    {
                        if (args.Length == 0)
                            return new DispatchWrapper(contents);
                        if (args.Length == 1)
                            return contents.GetType().GetMethod("Get").Invoke(contents, BindingFlags.InvokeMethod, null, args, culture);
                    }
                    else if ((invokeAttr & BindingFlags.PutDispProperty) == BindingFlags.PutDispProperty)
                    {
                        if (args.Length == 2)
                            return contents.GetType().GetMethod("Set").Invoke(contents, BindingFlags.InvokeMethod, null, args, culture);
                    }
                }

                // default InvokeMember
                return this.GetType().InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
            }
            catch (MissingMemberException ex)
            {
                //PFSession.Log.Error($"ERROR MissingMemberException: {ex.ToString()}");
                //System.IO.File.AppendAllText(@"C:\\log files\\pfsession.log",
                //    $"ERROR MissingMemberException: {ex.ToString()}\r\n");
                // Well-known HRESULT returned by IDispatch.Invoke:
                const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);
                throw new COMException(ex.Message, DISP_E_MEMBERNOTFOUND);
            }
            catch (Exception ex)
            {
                //PFSession.Log.Error($"ERROR MissingMemberException: {ex.ToString()}");
                //System.IO.File.AppendAllText(@"C:\\log files\\pfsession.log", $"ERROR MissingMemberException: {ex.ToString()}\r\n");
                const int DISP_E_EXCEPTION = unchecked((int)0x80020009);
                throw new COMException(ex.Message, DISP_E_EXCEPTION);
            }
        }

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return this.GetType().GetField(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return this.GetType().GetMember(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return this.GetType().GetMembers(bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return this.GetType().GetMethod(name, bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr,
        Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return this.GetType().GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr,
        Binder binder, Type returnType, Type[] types,
        ParameterModifier[] modifiers)
        {
            return this.GetType().GetProperty(name, bindingAttr, binder,
            returnType, types, modifiers);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return this.GetType().GetProperty(name, bindingAttr);
        }

        Type IReflect.UnderlyingSystemType
        {
            get { return this.GetType().UnderlyingSystemType; }
        }
    }
}
