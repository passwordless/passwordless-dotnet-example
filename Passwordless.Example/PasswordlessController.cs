using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Example.Models;

namespace Passwordless.Example;

[ApiController]
public class PasswordlessController : Controller
{
    private const string API_SECRET = "<YOUR_API_SECRET>"; // Replace with your API secret
    private readonly HttpClient _httpClient;

    public PasswordlessController()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://v4.passwordless.dev/");
        _httpClient.DefaultRequestHeaders.Add("ApiSecret", API_SECRET);

        if (API_SECRET == "<YOUR_API_SECRET>") throw new InvalidOperationException("Please set your API SECRET");
    }


    /// <summary>
    ///     Register - Get token from the passwordless API
    ///     The passwordless client side code needs a Token to register a key to a username.
    ///     The token is used by the Passwordless API to verify that this action is allowed for this user.
    ///     Your server can create this token by calling the Passwordless API with the ApiSecret.
    ///     This allows you to control the process, perhaps you only want to allow new users to register or only allow already
    ///     signed in users to add a Key to their own account.
    ///     Please see: https://docs.passwordless.dev/guide/api.html#register-token
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    [HttpGet("/create-token")]
    public async Task<IActionResult> GetRegisterToken(string alias)
    {
        var userId = Guid.NewGuid().ToString();

        var payload = new
        {
            userId,
            username = alias,
            Aliases = new[] { alias }
        };

        var request = await _httpClient.PostAsJsonAsync("register/token", payload);

        if (request.IsSuccessStatusCode)
        {
            var token = await request.Content.ReadFromJsonAsync<TokenResponse>();
            return Ok(token);
        }

        // Handle or log any API error
        var error = await request.Content.ReadFromJsonAsync<ProblemDetails>();
        return new JsonResult(error)
        {
            StatusCode = (int)request.StatusCode
        };
    }


    /// <summary>
    ///     Sign in - Verify the sign in
    ///     The passwordless API handles all the cryptography and WebAuthn details so that you don't need to.
    ///     In order for you to verify that the sign in was successful and retrieve details such as the username, you need to
    ///     verify the token that the passwordless client side code returned to you.
    ///     This is as easy as POST'ing it to together with your ApiSecret.
    ///     Please see: https://docs.passwordless.dev/guide/api.html#signin-verify
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/verify-signin")]
    public async Task<IActionResult> VerifySignInToken(string token)
    {
        var payload = new
        {
            token
        };

        var request = await _httpClient.PostAsJsonAsync("signin/verify",payload);

        if (request.IsSuccessStatusCode)
        {
            var signin = await request.Content.ReadFromJsonAsync<SigninResponse>();
            if (signin.Success)
            {
                // Set cookie etc.
            }

            return Ok(signin);
        }

        // Handle or log any API error
        var error = await request.Content.ReadFromJsonAsync<ProblemDetails>();
        return new JsonResult(error)
        {
            StatusCode = (int)request.StatusCode
        };
    }
}