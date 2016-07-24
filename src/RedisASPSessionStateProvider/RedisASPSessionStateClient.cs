using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using ASPTypeLibrary;

namespace Microsoft.Web.Redis
{
    public class RedisASPSessionStateClient : RedisASPSessionStateClientDispatch, IRedisASPSessionStateClient
    {

        protected bool _isExclusive = false;
        protected IRequest _request;
        protected RedisASPSessionVariantDictionary _session;
        protected HttpContext _context;
        protected string _sessionId;
        protected int _codePage = Encoding.UTF8.CodePage;
        protected CultureInfo _cultureInfo = CultureInfo.CurrentCulture;

        protected bool _locked;
        protected TimeSpan _lockAge;
        protected object _lockId;
        protected SessionStateActions _actionFlags;
        protected bool _newItem;

        protected TimeSpan _executionTimeout = new TimeSpan(0, 1, 50);
        protected int _sessionTimeout = 20;
        protected DateTime _lastAccessDateTime { get; set; }

        private bool _disposed;

        internal RedisASPSessionStateProvider _sessionProvider;
        internal SessionStateStoreData _storeData;
        public RedisASPSessionStateClient()
        {
        }

        public object this[string key]
        {
            get
            {
                return _session[key];
            }
            set
            {
                _session[key] = value;
            }
        }

        /// <summary>
        /// Session Timeout in minutes
        /// </summary>
        public int Timeout
        {
            get { return _sessionTimeout; }
            set
            {
                _sessionTimeout = value;
                _sessionProvider.ResetItemTimeout(_sessionId, _sessionTimeout * 60);
            }
        }
        public RedisASPSessionVariantDictionary Contents
        {
            get
            {
                Init();
                return _session;
            }
        }

        public void Abandon()
        {
            _session.RemoveAll();
        }

        public int CodePage
        {
            get
            {
                return _codePage;
            }
            set
            {
                _codePage = value;
            }
        }

        public int LCID
        {
            get
            {
                return _cultureInfo.LCID;
            }
            set
            {
                _cultureInfo = CultureInfo.GetCultureInfo(value);
            }
        }

        public void Init()
        {
            if (_request == null)
            {
                //PFSession.Log.Debug($"RedisASPSessionStateClient:Init:{this.SessionID}");

                _request = (IRequest)ContextUtil.GetNamedProperty("Request");
                var _response = (Response)ContextUtil.GetNamedProperty("Response");
                //_session = (ISessionObject)ContextUtil.GetNamedProperty("PFSession");

                AspWorkerRequest wr = new AspWorkerRequest(_request);
                _context = new HttpContext(wr);

                // Try to retrieve SessionId from the Cookies. It could have been generated from ASP.NET or from Classic ASP.
                IReadCookie cookie = (IReadCookie)_request.Cookies["ASP.NET_SessionId"];
                _sessionId = String.Empty;
                if (cookie != null && cookie[Missing.Value] != null)
                {
                    //http://msdn.microsoft.com/en-us/library/ms525056(VS.90).aspx
                    _sessionId = cookie[Missing.Value];
                    //VR_ERROR with DISP_E_PARAMNOTFOUND, http://www.informit.com/articles/article.aspx?p=27219&seqNum=8
                }

                InitSession();

                _session = new RedisASPSessionVariantDictionary().Init(_sessionProvider, _storeData, _sessionId, _sessionTimeout * 60);
            }
        }

        /// <summary>
        /// Try to retrive the existing session data from Redis
        /// If the data is not found, it will create a new session with a new sessionId
        /// </summary>
        private void InitSession()
        {

            // If sessionProvider is not null that means we already initialized the session and got the dataStore
            if (_sessionProvider == null)
            {
                _sessionProvider = new RedisASPSessionStateProvider();
            }

            _storeData = null;

            // Get write lock and session from cache
            bool locked;
            TimeSpan lockAge;
            object lockId;
            SessionStateActions actions;

            if (!string.IsNullOrWhiteSpace(_sessionId))
            {
                // Try Retrieve the existing session
                _storeData = _sessionProvider.GetItem(null, _sessionId, out locked, out lockAge, out lockId,
                    out actions);
            }

            // If we cannot find an existing session we will create a new one
            if (_storeData == null)
            {
                // Generate a new session id
                SessionIDManager Manager = new SessionIDManager();
                _sessionId = Manager.CreateSessionID(_context);

                // Create a new Session in Redis 
                _sessionTimeout = (int)RedisSessionStateProvider.configuration.SessionTimeout.TotalMinutes;
                _sessionProvider.CreateUninitializedItem(null, _sessionId, _sessionTimeout);
                // Get the store Data from the new session created
                _storeData = _sessionProvider.GetItem(null, _sessionId, out locked, out lockAge, out lockId,
                    out actions);

                // If for some reason it failed to get the store data we raise an exception
                if (_storeData == null)
                {
                    throw new InvalidOperationException("Store Data cannot be created");
                }
            }

        }

        public object Get(string key)
        {
            return Contents.Get(key);
        }

        public void Set(string key, object value)
        {
            Contents.Set(key, value);
        }

        public long Count => Contents.Count;

        public string[] Keys => Contents.Keys;
        public string SessionID => _sessionId;
        public void RemoveAll()
        {
            Contents.RemoveAll();
        }

        public void Remove(string key)
        {
            Contents.Remove(key);
        }

        public bool ContainsKey(string key)
        {
            return Contents.ContainsKey(key);
        }

        public IEnumerator GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

    }
}
