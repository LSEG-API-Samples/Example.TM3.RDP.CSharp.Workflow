using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TM3Console
{
    class Program
    {
        const string bucketname = "bulk-Custom";
        const string package_id = "packageId";

        static string baseUrl = "https://api.refinitiv.com:443";

        static Token token = new Token();
        static async Task Main(string[] args)
        {

            string machineid = "RDP MACHINEID";
            string password = "RDP PASSWORD";
            string appkey = "RDP APP_KEY";

            token = await Login(machineid, password, appkey);

            if (!string.IsNullOrEmpty(token.AccessToken))
            {
                Console.WriteLine("Login succeed\n\n");
                await QueryFileSet(bucketname,package_id,token.AccessToken);
                await QueryFileSetModifiedSince(bucketname, package_id, token.AccessToken);
            }

            Console.ReadLine();
        }

        private static async Task<Token> Login(string machineid, string password, string appkey)
        {
            string auth_url = $"{baseUrl}/auth/oauth2/v1/token";
            try
            {
                var payload = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "username", machineid },
                    { "password", password },
                    { "client_id", appkey },
                    { "grant_type", "password" },
                    { "takeExclusiveSignOnControl", "true" },
                    { "scope", "trapi" }
                });

                payload.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var httpclient = new HttpClient();
                HttpResponseMessage response = await httpclient.PostAsync(auth_url, payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Status Code: {(int)response.StatusCode}");
                    Console.WriteLine($"Status Text: {response.ReasonPhrase}");
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject( responseBody );
                    return new Token
                    {
                        AccessToken = json.access_token,
                        RefreshToken = json.refresh_token,
                        Expires_in = (int)json.expires_in
                    };
                }
                else
                {
                    Console.WriteLine($"Status Code: {(int)response.StatusCode}");
                    Console.WriteLine($"Status Text: {response.ReasonPhrase}");
                    var error_resp = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Authen RDP Fail with {error_resp} error");
                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request HTTP error: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
            return new Token(); // Return empty token on failure
        }

        private static async Task QueryFileSet(string bucketname, string package_id, string access_token)
        {
            string queryParams = $"?bucket={bucketname}&packageId={package_id}&pageSize=100";

            string fileset_url = $"{baseUrl}/file-store/v1/file-sets{queryParams}";

            var request = new HttpRequestMessage(HttpMethod.Get, fileset_url);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

            Console.WriteLine("Requesting FileSet using pageSize = 100");
            HttpResponseMessage response = await httpClient.GetAsync(fileset_url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{responseBody}\n\n");
            }
            else
            {
                Console.WriteLine($"Error: {(int)response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        private static async Task QueryFileSetModifiedSince(string bucketname, string package_id, string access_token)
        {
            string queryParams = $"?bucket={bucketname}&packageId={package_id}&pageSize=100&modifiedSince={Uri.EscapeDataString("2025-08-12T12:00:00Z")}";

            string fileset_url = $"{baseUrl}/file-store/v1/file-sets{queryParams}";

            var request = new HttpRequestMessage(HttpMethod.Get, fileset_url);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

            Console.WriteLine("Requesting FileSet using pageSize = 100 and modifiedSince = 2025-08-12T12:00:00Z");
            HttpResponseMessage response = await httpClient.GetAsync(fileset_url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{responseBody}\n\n");
            }
            else
            {
                Console.WriteLine($"Error: {(int)response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }
}
