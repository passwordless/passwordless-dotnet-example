using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace passwordless_dotnet_example
{
    [ApiController]
    public class PasswordlessController : Controller
    {
        private HttpClient _httpClient;
        private readonly static string API_SECRET = "YOUR_API_SECRET"; // Replace with your API secret

        public PasswordlessController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.passwordless.dev/");
            _httpClient.DefaultRequestHeaders.Add("ApiSecret", API_SECRET);

            if(API_SECRET == "YOUR_API_SECRET") { throw new Exception("Please set your API SECRET"); }

        }


        /// <summary>
        /// Register - Get token from the passwordless API
        ///
        /// The passwordless client side code needs a Token to register a key to a username.
        /// The token is used by the Passwordless API to verify that this action is allowed for this user.
        /// Your server can create this token by calling the Passwordless API with the ApiSecret.
        /// This allows you to control the process, perhaps you only want to allow new users to register or only allow already signed in users to add a Key to their own account.
        /// 
        /// Request body looks like:
        ///  { username: 'anders', displayName:'Anders Åberg'}
        ///  Response body looks like:
        ///  "abcdefghiojklmnopq..."
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("/create-token")]
        public async Task<ActionResult<string>> GetRegisterToken(string username)
        {
            
            var json = JsonSerializer.Serialize(new
            {
                username
            });

            var request = await _httpClient.PostAsync("register/token", new StringContent(json, Encoding.UTF8, "application/json"));
            request.EnsureSuccessStatusCode();
            var token = await request.Content.ReadAsStringAsync();

            return token;
        }

        /// <summary>
        /// Sign in - Verify the sign in
        /// 
        /// The passwordless API handles all the cryptography and WebAuthn details so that you don't need to.
        /// In order for you to verify that the sign in was successful and retrieve details such as the username, you need to verify the token that the passwordless client side code returned to you.
        /// This is as easy as POST'ing it to together with your ApiSecret.
        /// 
        /// Request body looks like:
        /// { token: "xxxyyyzz..." }
        /// Response body looks like:
        /// {
        /// "success":true,
        /// "username":"anders@user.com",
        /// "timestamp":"2020-08-21T16:42:48.5061807Z",
        /// "rpid":"example.com",
        /// "origin":"https://example.com"}
        ///
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/verify-signin")]
        public async Task<IActionResult> VerifySignInToken(string token)
        {

            // check if the fido2 authentication was valid and for what user
            var json = JsonSerializer.Serialize(new
            {
                token
            });

            var request = await _httpClient.PostAsync("signin/verify", new StringContent(json, Encoding.UTF8, "application/json"));
            request.EnsureSuccessStatusCode();
            var response = await request.Content.ReadAsStringAsync();
            var signin = JsonSerializer.Deserialize<SignInDto>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (signin.Success == true)
            {
                // Sign in the user, set cookies etc
            }

            return Json(signin);
        }

        public class SignInDto
        {
            public SignInDto()
            {

            }
            public bool Success { get; set; }

            public string Username { get; set; }
            public DateTime Timestamp { get; set; }
            public string RPID { get; set; }
            public string Origin { get; set; }
            public string Device { get; set; }
            public string Country { get; set; }
            public string Nickname { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}

