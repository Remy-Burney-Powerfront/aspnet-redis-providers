using System.Runtime.InteropServices;

namespace Microsoft.Web.Redis
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IRedisASPSessionStateClient : IRedisASPSession
    {
    }
}