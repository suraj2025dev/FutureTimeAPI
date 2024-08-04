using Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth
{
    public static class SessionManagement
    {
        private static volatile List<SessionPayload> ActiveSessionList = new List<SessionPayload>();

        static object session_lock = new object();

        public static string GenerateToken(SessionPayload payload)
        {
            lock (session_lock)
            {
                var new_valid_token = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString()).Replace("-", "");
                payload.token = new_valid_token;
                payload.last_interaction_time = DateTime.Now;
                ActiveSessionList.Add(payload);
                ClearUpExpiredSession();

                return new_valid_token;
            }
        }

        public static SessionPayload IsSessionActive(string token, Guid? session_id = null)
        {
            lock (session_lock)
            {
                //This function returns null if session is invalid.

                //If session_id then only validate token. It is called from appbase
                ClearUpExpiredSession();

                SessionPayload active_session = null;
                if (session_id != null)
                {
                    var index_of_active_session = ActiveSessionList.FindIndex(w => w.session_id == session_id && w.token == token);
                    if (index_of_active_session == -1)
                        return null;
                    ActiveSessionList[index_of_active_session].last_interaction_time = DateTime.Now;
                    active_session = ActiveSessionList[index_of_active_session];

                }
                else if (session_id == null)
                {
                    var index_of_active_session = ActiveSessionList.FindIndex(w => w.token == token);
                    if (index_of_active_session == -1)
                        return null;
                    ActiveSessionList[index_of_active_session].last_interaction_time = DateTime.Now;
                    active_session = ActiveSessionList[index_of_active_session];
                }

                return active_session;
            }
        }

        private static void ClearUpExpiredSession()
        {
            lock (session_lock)
            {
                //If Active Session List count hits 10000 mark then clear up everything.
                if (ActiveSessionList.Count() > 10000)
                {
                    ActiveSessionList = new List<SessionPayload>();
                }
                ActiveSessionList = ActiveSessionList.Where(w => w.last_interaction_time.AddMinutes(Library.Data.AppStatic.CONFIG.App.SessionTimeOut) >= DateTime.Now).ToList();
            }
        }

        public static void ClearUpSpecificSessionOfToken(string token)
        {
            lock (session_lock)
            {
                ActiveSessionList = ActiveSessionList.Where(w => w.token != token).ToList();
            }
        }

        public static void ClearUpSpecificSessionOfSessionId(Guid session_id)
        {
            lock (session_lock)
            {
                ActiveSessionList = ActiveSessionList.Where(w => w.session_id != session_id).ToList();
            }
        }

        public static void ClearUpSpecificSessionOfUser(string user_name)
        {
            lock (session_lock)
            {
                ActiveSessionList = ActiveSessionList.Where(w => w.user_email != user_name).ToList();
            }
        }
    }

    public class SessionPayload
    {
        public string user_email { get; set; }
        public int user_id { get; set; }
        public string token { get; set; }
        public Guid session_id { get; set; }
        public DateTime last_interaction_time { get; set; }//Last time this token was used. A token created before session time out is set to auto expire.
    }
}
