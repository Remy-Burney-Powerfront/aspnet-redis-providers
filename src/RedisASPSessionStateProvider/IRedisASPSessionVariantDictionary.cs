using System.Collections;
using System.Runtime.InteropServices;

namespace Microsoft.Web.Redis
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IRedisASPSessionVariantDictionary : IEnumerable
    {
        [DispId(0)]
        object this[string key]
        {
            [DispId(0)]
            get;
            [DispId(0)]
            set;
        }

        long Count { get; }
        void Remove(string key);
        void RemoveAll();
        bool ContainsKey(string key);
        string[] Keys { get; }
        string ToString();
    }
}