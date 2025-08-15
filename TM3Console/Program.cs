using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotNetEnv;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;

namespace TM3Console
{
    class Program
    {
        const string bucketname = "bulk-Custom";
        static string package_id = string.Empty;   

        static string baseUrl = "https://api.refinitiv.com:443";

        static Token token = new Token();
        static string fileSet = string.Empty;
        static string modifiedSinceTime = "2025-08-12T12:00:00Z";
        static string nextLink = string.Empty;
        static string actualURL = string.Empty;
        static async Task Main(string[] args)
        {
            DotNetEnv.Env.Load();

            string machineid = Environment.GetEnvironmentVariable("MACHINE_ID") ?? "<MACHINE_ID>";
            string password = Environment.GetEnvironmentVariable("PASSWORD") ?? "<PASSWORD>";
            string appkey = Environment.GetEnvironmentVariable("APP_KEY") ?? "<APP_KEY>";
            package_id = Environment.GetEnvironmentVariable("PACKAGE_ID") ?? "<PACKAGE_ID>";

            token = await Login(machineid, password, appkey);

            if (!string.IsNullOrEmpty(token.AccessToken))
            {
                Console.WriteLine("Login succeed");
                fileSet = await QueryFileSet(bucketname, package_id, token.AccessToken);

                if (!string.IsNullOrEmpty(nextLink))
                {
                    //Do Paging
                    Console.WriteLine("Please note that the FileSets can be more than 100 records. The API returns data maximum 100 records per one query.");
                    Console.WriteLine("If the there are more than 100 records, the API returns the ```@nextLink``` node which contains the URL for requesting the next page of query");
                    Console.WriteLine($"@nextLink = {nextLink}\n");
                    await QueryFileSetPaging(token.AccessToken, nextLink);
                }

                Console.WriteLine("The ```modifiedSince``` parameter can help an application to limit the returned File-Set only for the File-Set that has been modified after a specified time. ");
                Console.WriteLine("It is recommended to call the endpoint with ```pageSize=100``` and ```modifiedSince``` parameters as follows:\n");
                await QueryFileSetModifiedSince(bucketname, package_id, token.AccessToken, modifiedSinceTime);

                if (!string.IsNullOrEmpty(fileSet))
                {
                    Console.WriteLine("### Step 3: Get the AWS S3 file URL using FileSet");
                    actualURL = await QueryActualFileURL(fileSet, token.AccessToken);
                }
                
            }

            Console.ReadLine();
        }
        /// <summary>
        /// Send HTTP Post Authentication Request Message to RDP Authentication Service (/auth/oauth2/ endpoint)
        /// </summary>
        /// <param name="machineid">RDP Machine ID/Username</param>
        /// <param name="password">RDP Password</param>
        /// <param name="appkey">RDP App-Key</param>
        /// <returns>RDP Token information</returns>
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
                Console.WriteLine($"Request {auth_url} HTTP error: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request{auth_url}  error: {e.Message}");
            }
            return new Token(); // Return empty token on failure
        }

        /// <summary>
        /// Send HTTP GET Request to RDP CFS API Service (/file-store/ endpoint) with pageSize=100
        /// </summary>
        /// <param name="bucketname">bucketname (bulk-Custom for TM3)</param>
        /// <param name="package_id">Package ID (contact your Account Manager)</param>
        /// <param name="access_token">Access Token (from RDP Authentication)</param>
        /// <returns>FileSet information</returns>
        private static async Task<string> QueryFileSet(string bucketname, string package_id, string access_token)
        {
            string queryParams = $"?bucket={bucketname}&packageId={package_id}&pageSize=100";

            string fileset_url = $"{baseUrl}/file-store/v1/file-sets{queryParams}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, fileset_url);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

                Console.WriteLine("Requesting FileSet using pageSize = 100");
                HttpResponseMessage response = await httpClient.GetAsync(fileset_url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(responseBody);
                    if (json.ContainsKey("@nextLink"))
                    {
                        nextLink = json["@nextLink"];
                    }
                    
                    JArray fileSetsArray = (JArray)json.value;
                    
                    if (fileSetsArray != null || !fileSetsArray.Any()) //file Return data is not empty
                    {
                        Console.WriteLine($"FileSets data (first 100 records) are {fileSetsArray}\n\n");
                        Console.WriteLine("The File ID is in the files array. Select the one that you need.");
                        Console.WriteLine("I am demonstrating with the first entry.");
                        string FileSet = (string)fileSetsArray[0]["files"][0];
                        Console.WriteLine($"The FileSet is {FileSet}\n\n");
                        return FileSet;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {(int)response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request {fileset_url} HTTP error: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request {fileset_url} error: {e.Message}");
            }

            return ""; //in case error or empty data
        }

        /// <summary>
        /// Send HTTP GET Request to RDP CFS API Service (/file-store/ endpoint) with pageSize=100 with Paging feature
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="nextLinkURL"></param>
        /// <returns></returns>
        private static async Task QueryFileSetPaging(string access_token, string nextLinkURL)
        {
            string fileset_url = $"{baseUrl}{nextLinkURL}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, fileset_url);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

                Console.WriteLine("Requesting FileSet next paging");
                HttpResponseMessage response = await httpClient.GetAsync(fileset_url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(responseBody);
                    if (json.ContainsKey("@nextLink"))
                    {
                        nextLink = json["@nextLink"];
                    }
                    JArray fileSetsArray = (JArray)json.value;

                    if (fileSetsArray != null || !fileSetsArray.Any()) //file Return data is not empty
                    {
                        Console.WriteLine($"FileSets data (next 100 records) are {fileSetsArray}\n\n");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {(int)response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request {fileset_url} HTTP error: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request {fileset_url} error: {e.Message}");
            }
        }

        /// <summary>
        /// Send HTTP GET Request to RDP CFS API Service (/file-store/ endpoint) with pageSize=100&modifiedSince=DATE_TIME_IN_GMT0
        /// </summary>
        /// <param name="bucketname">bucketname (bulk-Custom for TM3)</param>
        /// <param name="package_id">Package ID (contact your Account Manager)</param>
        /// <param name="access_token">Access Token (from RDP Authentication)</param>
        /// <param name="modifiedSince">Return all file-sets that have a modified date after the specified Datetime.</param>
        /// <returns></returns>
        private static async Task QueryFileSetModifiedSince(string bucketname, string package_id, string access_token, string modifiedSince)
        {
            string queryParams = $"?bucket={bucketname}&packageId={package_id}&pageSize=100&modifiedSince={Uri.EscapeDataString(modifiedSince)}";

            string fileset_url = $"{baseUrl}/file-store/v1/file-sets{queryParams}";

            try
            {
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
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request {fileset_url} HTTP error: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request {fileset_url} error: {e.Message}");
            }

            
        }

        private static async Task<string> QueryActualFileURL(string fileSet, string access_token)
        {
            string file_url = $"{baseUrl}/file-store/v1/files/{fileSet}/stream?doNotRedirect=true";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, file_url);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

                Console.WriteLine("Requesting actual file URL using FileSet");
                HttpResponseMessage response = await httpClient.GetAsync(file_url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(responseBody);
                    Console.WriteLine($"{responseBody}\n\n");
                    if (json.ContainsKey("url"))
                    {
                        Console.WriteLine("The Actual File URL is in the ```url``` attribute of the response message.");
                        Console.WriteLine($"The Actual File URL = {(string)json.url}\n\n");
                        return (string)json.url;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {(int)response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request {file_url} HTTP error: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request {file_url} error: {e.Message}");
            }

            return string.Empty; //fail case
        }
    }
}
