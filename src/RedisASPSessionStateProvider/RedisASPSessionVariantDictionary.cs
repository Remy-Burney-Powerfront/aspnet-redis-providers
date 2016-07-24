using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Web.Redis
{
    [ComVisible(false)]
    public class RedisASPSessionVariantDictionary : RedisASPSessionDispatch, IRedisASPSessionVariantDictionary
    {
        internal string sessionId;
        internal RedisASPSessionStateProvider sessionProvider;
        internal SessionStateStoreData storeData;
        protected new object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
            object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture,
            string[] namedParameters)
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
                            return this.GetType()
                                .GetMethod("ToString")
                                .Invoke(this, BindingFlags.InvokeMethod, null, new object[] { }, culture);
                        if (args.Length == 1)
                            return this.GetType()
                                .GetMethod("Get")
                                .Invoke(this, BindingFlags.InvokeMethod, null, args, culture);
                    }
                    else if ((invokeAttr & BindingFlags.PutDispProperty) == BindingFlags.PutDispProperty)
                    {
                        if (args.Length == 2)
                            return this.GetType()
                                .GetMethod("Set")
                                .Invoke(this, BindingFlags.InvokeMethod, null, args, culture);
                    }
                }
                else if (name == "Keys" && (invokeAttr & BindingFlags.GetProperty) == BindingFlags.GetProperty &&
                         args.Length == 0)
                {
                    return ((string[])this.GetType().GetProperty("Keys").GetValue(this)).Cast<object>().ToArray();
                }

                // default InvokeMember
                return this.GetType()
                    .InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
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

        public RedisASPSessionVariantDictionary()
        {
        }
        public RedisASPSessionVariantDictionary Init(RedisASPSessionStateProvider sessionProvider, SessionStateStoreData storeData, string sessionId, int expiry)
        {
            //PFSession.Log.Debug($"PFRedisVariantDictionary:Init:{hashkey}");
            //System.IO.File.AppendAllText(@"C:\\log files\\pfsession.log", $"PFRedisVariantDictionary:Init:{hashkey}");
            this.sessionId = sessionId;
            this.sessionProvider = sessionProvider;
            this.storeData = storeData;

            return this;
        }

        public object Get(string key)
        {
            //PFSession.Log.Debug($"PFRedisVariantDictionary[{_hashkey}]:Get:{key}");
            //System.IO.File.AppendAllText(@"C:\\log files\\pfsession.log", $"PFRedisVariantDictionary[{_hashkey}]:Get:{key}");

            return storeData.Items[key];
        }

        public bool Set(string key, object value)
        {
            //PFSession.Log.Debug($"PFRedisVariantDictionary[{_hashkey}]:Set:{key}:{value}");
            //System.IO.File.AppendAllText(@"C:\\log files\\pfsession.log", $"PFRedisVariantDictionary[{_hashkey}]:Set:{key}:{value}");

            // setting data as done by any normal session operation
            storeData.Items[key] = value;

            CommitSessionStoreData();

            return true;
        }

        internal void CommitSessionStoreData()
        {

            // session update
            sessionProvider.SetAndReleaseItemExclusive(null, sessionId, storeData, false, false);

            //TODO : To Check with the 2 actions below are required : ResetItemTimeout, EndRequest. Maybe we can remove to speed up the process

            // reset sessions timeout
            sessionProvider.ResetItemTimeout(null, sessionId);

            // End request
            sessionProvider.EndRequest(null);

        }

        public IEnumerator GetEnumerator()
        {
            return Keys.GetEnumerator();
        }

        public object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        public long Count => storeData?.Items?.Count ?? 0;

        public void Remove(string key)
        {
            if (storeData == null) return;
            storeData?.Items?.Remove(key);
            CommitSessionStoreData();
        }

        public void RemoveAll()
        {
            if (storeData == null) return;
            storeData?.Items?.Clear();
            CommitSessionStoreData();
        }
        public bool ContainsKey(string key)
        {
            return storeData != null && storeData.Items.Keys.Cast<object>().Contains(key);
        }
        public string[] Keys => storeData.Items.Keys.Cast<string>().ToArray();
    }
}
