using System;

namespace TM3Console
{
    class Token
    {
        public Token() { }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public int Expires_in { get; set; }
    }
}
