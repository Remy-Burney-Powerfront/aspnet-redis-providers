using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Web.Redis
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IRedisASPSession
    {
        [DispId(0)]
        object this[string key]
        {
            [DispId(0)]
            get;
            [DispId(0)]
            set;
        }

        string ToString();

        string SessionID { get; }
        int Timeout { get; set; }

        void Abandon();

        int CodePage { get; set; }
        int LCID { get; set; }
        RedisASPSessionVariantDictionary Contents { get; }
    }
}
