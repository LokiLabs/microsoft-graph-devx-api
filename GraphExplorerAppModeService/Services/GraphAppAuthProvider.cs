using GraphExplorerAppModeService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


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
