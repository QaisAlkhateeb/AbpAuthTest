using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TodoApp.Web
{
    public class StagingUaePassOptions : OAuthOptions
    {
        public string BaseUrl { get; set; } = UaePassDefaults.STAGING_ENVIROMENT_URL;

        public StagingUaePassOptions()
        {
            CallbackPath = new PathString(UaePassDefaults.CallBackUrl);

            AuthorizationEndpoint = $"{BaseUrl}{UaePassDefaults.AuthorizationEndpoint}";
            TokenEndpoint = $"{BaseUrl}{UaePassDefaults.TokenEndpoint}";
            UserInformationEndpoint = $"{BaseUrl}{UaePassDefaults.UserInformationEndpoint}";
            

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "fullnameEN");
            ClaimActions.MapJsonKey(ClaimTypes.GivenName, "firstnameEN");
            ClaimActions.MapJsonKey(ClaimTypes.Surname, "lastnameEN");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            ClaimActions.MapJsonKey(ClaimTypes.MobilePhone, "mobile");
            ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
            ClaimActions.MapJsonKey(ClaimTypes.Country, "nationalityEN");
            ClaimActions.MapJsonKey(ClaimTypes.SerialNumber, "idn");

            Scope.Add("openid");
        }
    }
    public static class UaePassDefaults
    {
        public const string AuthenticationScheme = "UaePass";
        public static string DisplayName = "UAEPASS";

        public static string AuthorizationEndpoint = "/authorize?acr_values=urn:safelayer:tws:policies:authentication:level:low";
        public static string TokenEndpoint = "/token";
        public static string UserInformationEndpoint = "/userinfo";
        public static string LogOutEndpoint = "/logout";
        public static string CallBackUrl = "/Account/UaePassCallBack";

        public const string StagingClientId = "sandbox_stage";
        public const string STAGING_ENVIROMENT_URL = "https://stg-id.uaepass.ae/idshub";

    }

    public class UaePassHandler : OAuthHandler<StagingUaePassOptions>
    {
        public UaePassHandler(IOptionsMonitor<StagingUaePassOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }
        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await Backchannel.SendAsync(request, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving Uae Pass user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted)))
            {
                var context = new OAuthCreatingTicketContext
                    (new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
                context.RunClaimActions();
                await Events.CreatingTicket(context);
                return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
            }

        }
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            return base.BuildChallengeUrl(properties, redirectUri);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            return base.HandleChallengeAsync(properties);
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return base.HandleAuthenticateAsync();
        }

        protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            return base.HandleRemoteAuthenticateAsync();
        }

        protected override Task<HandleRequestResult> HandleAccessDeniedErrorAsync(AuthenticationProperties properties)
        {
            return base.HandleAccessDeniedErrorAsync(properties);
        }

        protected override Task<object> CreateEventsAsync()
        {
            return base.CreateEventsAsync();
        }

        protected override Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            return base.ExchangeCodeAsync(context);
        }

        protected override string? ResolveTarget(string? scheme)
        {
            return base.ResolveTarget(scheme);
        }
    }

}
