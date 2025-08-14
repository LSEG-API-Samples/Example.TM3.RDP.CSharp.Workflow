using System;

namespace TM3Console
{
    /// <summary>
    /// RDP Access Token information class
    /// </summary>
    class Token
    {
        public Token() { }

        /// <summary>
        /// Access Token: The token used to invoke REST data API calls as described above. The application must keep this credential for further RDP APIs requests.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Refresh token to be used for obtaining an updated access token before expiration. The application must keep this credential for access token renewal.
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Access token validity time in seconds.
        /// </summary>
        public int Expires_in { get; set; }
    }
}
