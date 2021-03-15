using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Api.V1
{
    public static class ApiRoutes
    {
        public const string Slash = "/";
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + Slash + Version;
        
        public static class Clients
        {
            public const string GetAll = Base + Slash + "Clients";
        }

        public static class Auth
        {
            public const string Login = Base + Slash + "auth/login";
            public const string Refresh = Base + Slash + "auth/refresh";
            public const string Logout = Base + Slash + "auth/logout";
        }

        public static class Sessions
        {
            public const string GetLatestInProgress = Base + Slash + "Sessions/GetLatestInProgress";
            public const string Create = Base + Slash + "Sessions/Create";
            public const string Update = Base + Slash + "Sessions/Update";
        }
    }
}
