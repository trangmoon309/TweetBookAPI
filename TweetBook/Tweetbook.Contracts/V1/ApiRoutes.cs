    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Version = "v1";
        public const string Root = "api";
        public const string Base = Root + "/" + Version;
       public static class Posts
       {
            public const string GetAll = Base + "/posts";
            public const string GetOne = Base + "/posts/{id}";
            public const string Create = Base + "/posts";
            public const string Update = Base + "/posts/{id}";
            public const string Delete = Base + "/posts/{id}";

       }

        public static class Identity
        {
            public const string Login = Base + "/identity/login";
            public const string Register = Base + "/identity/register";
            public const string Refresh = Base + "/identity/refresh";
            public const string Logout = Base + "/logout";
        }

        public static class Tags
        {
            public const string GetAll = Base + "/tags";
            public const string Delete = Base + "/tags/{tagName}";
            public const string Create = Base + "/tags";

        }
    }
}
