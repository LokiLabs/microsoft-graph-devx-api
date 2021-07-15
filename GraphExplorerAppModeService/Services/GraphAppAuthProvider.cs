using GraphExplorerAppModeService.Interfaces;
using Microsoft.Identity.Client;
using System.Threading.Tasks;


namespace GraphExplorerAppModeService.Services
{
    public class GraphAppAuthProvider : IGraphAppAuthProvider
    {
        private readonly IConfidentialClientApplication _app;
        private readonly string[] _scopes;
        public GraphAppAuthProvider(string clientId, string clientSecret, string[] scopes, string uri)
        {
            _scopes = scopes;
            _app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(uri)
                    .Build();
        }
        public async Task<string> retrieveToken()
        {
            var authenticationResult = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
            return authenticationResult.AccessToken;
        }
    }
}

