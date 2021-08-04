using GraphExplorerAppModeService.Interfaces;
using Microsoft.Graph;
using System.Net.Http.Headers;

namespace GraphExplorerAppModeService.Services
{
    public class GraphAppAuthProvider : IGraphAppAuthProvider
    {

        public GraphServiceClient GetAuthenticatedGraphClient(string accessToken) =>
            new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    // Append the access token to the request
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }));
    }

}
