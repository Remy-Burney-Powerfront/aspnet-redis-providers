using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ASPTypeLibrary;

namespace Microsoft.Web.Redis
{
    public class RedisASPSessionProvider : RedisASPSessionProviderDispatch, IRedisASPSessionProvider
    {



        // List of all the classic asp sessions loaded in the cache on the webserver
        // Caching on the webserver means we have to lock the user to a specific webserver so it's using the same cached data
        // TODO : handle refresh of the cache when session in REDIS is updated from a standalone aspx script
        private static ConcurrentDictionary<String, RedisASPSessionStateClient> _currentSessions = new ConcurrentDictionary<string, RedisASPSessionStateClient>();

        public RedisASPSessionProvider()
        {
            
        }

        public string GetCurrentSessionId()
        {
            var _request = (IRequest)ContextUtil.GetNamedProperty("Request");
            IReadCookie cookie = (IReadCookie)_request.Cookies["ASP.NET_SessionId"];
            var _sessionId = String.Empty;
            if (cookie != null && cookie[Missing.Value] != null)
            {
                _sessionId = cookie[Missing.Value];
            }
            return _sessionId;
        }
        public RedisASPSessionStateClient GetCurrentSession()
        {
            //LogEnter();
            var sessionId = GetCurrentSessionId();

            //Log.Debug($"GetCurrentSession:GetCurrentSessionId:{sessionId}");

            // When the sessionId is there for the client, we get the session.
            if (!String.IsNullOrWhiteSpace(sessionId))
            {
                return _currentSessions.GetOrAdd(sessionId, (id) => new RedisASPSessionStateClient());
            }

            // otherwize we create a new session
            var newSession = new RedisASPSessionStateClient();
            newSession.Init();
            //Log.Debug($"GetCurrentSession:New Session Created:{newSession.SessionID}");
            if (!_currentSessions.TryAdd(newSession.SessionID, newSession))
            {
                //Log.Debug($"GetCurrentSession:Error Add New Session:{newSession.SessionID}");
                throw new InvalidOperationException($"Cannot add session {newSession.SessionID}");
            }

            //Log.Debug($"GetCurrentSession:newSession.Init():{newSession.SessionID}");
            return newSession;
        }

        public object this[string key]
        {
            get
            {
                var currentSession = GetCurrentSession();
                if (currentSession == null) throw new NullReferenceException("Current Client Session is null");
                return currentSession[key];
            }
            set
            {
                var currentSession = GetCurrentSession();
                if (currentSession == null) throw new NullReferenceException("Current Client Session is null");
                currentSession[key] = value;
            }
        }

        public string SessionID { get { return GetCurrentSessionId(); } }
        public int Timeout
        {
            get
            {
                return GetCurrentSession().Timeout;
            }
            set
            {
                GetCurrentSession().Timeout = value;
            }
        }

        public void Abandon()
        {
            GetCurrentSession().Abandon();
        }

        public int CodePage
        {
            get
            {
                return GetCurrentSession().CodePage;
            }
            set
            {
                GetCurrentSession().CodePage = value;
            }
        }

        public int LCID
        {
            get
            {
                return GetCurrentSession().LCID;
            }
            set
            {
                GetCurrentSession().LCID = value;
            }
        }

        public RedisASPSessionVariantDictionary Contents
        {
            get
            {
                return GetCurrentSession().Contents;
            }
        }

    }

}
