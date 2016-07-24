using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace Microsoft.Web.Redis
{
    public class RedisASPSessionStateProvider : RedisSessionStateProvider
    {
        
        internal string _sessionId;
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            _sessionId = id;
            return base.GetItem(context, id, out locked, out lockAge, out lockId, out actions);
        }
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            _sessionId = id;
            return base.GetItemExclusive(context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Update the session timeout
        /// </summary>
        /// <param name="id">session id</param>
        /// <param name="timeout">session timeout in seconds</param>
        public void ResetItemTimeout(string id, int timeout)
        {
            try
            {
                if (LastException == null)
                {
                    LogUtility.LogInfo("ResetItemTimeout => Session Id: {0}, Session provider object: {1}.", id, this.GetHashCode());
                    GetAccessToStore(id);
                    if (timeout > 0)
                    {
                        cache.UpdateExpiryTime(timeout);
                    }
                    else
                    {
                        cache.UpdateExpiryTime((int) configuration.SessionTimeout.TotalSeconds);
                    }
                    cache = null;
                }
            }
            catch (Exception e)
            {
                LogUtility.LogError("ResetItemTimeout => {0}", e.ToString());
                LastException = e;
                if (configuration.ThrowOnError)
                {
                    throw;
                }
            }
        }

        //public SessionStateStoreData CreateNewDataStore(String sessionId)
        //{

        //    if (!string.IsNullOrWhiteSpace(sessionId))
        //    {
        //        CreateUninitializedItem(null, sessionId,
        //            (int)configuration.SessionTimeout.TotalMinutes);

        //        // Get write lock and session from cache
        //        bool locked;
        //        TimeSpan lockAge;
        //        object lockId;
        //        SessionStateActions actions;

        //        return GetItem(null, sessionId, out locked, out lockAge, out lockId,
        //            out actions);
        //    }

        //    LastException = new NullReferenceException("SessionId cannot be null");
        //    if (configuration.ThrowOnError)
        //    {
        //        throw LastException;
        //    }
        //    return null;
        //}

    }
}
