using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Graph;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Primitives;
using GraphExplorerAppModeService.Services;
using GraphExplorerAppModeService.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace GraphWebApi.Controllers
{
    [ApiController]
    public class GraphExplorerAppModeController : ControllerBase
    {
        private readonly IGraphAppAuthProvider _graphAuthClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _config;
        private readonly IGraphService _graphService;

        public GraphExplorerAppModeController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, IGraphAppAuthProvider graphServiceClient, IGraphService graphService)
        {
            this._graphAuthClient = graphServiceClient;
            this._tokenAcquisition = tokenAcquisition;
            this._config = configuration;
            this._graphService = graphService;
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> GetAsync(string all, [FromHeader] string Authorization)
        {
            return await this.ProcessRequestAsync("GET", all, null, Authorization).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPost]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> PostAsync(string all, [FromBody] object body, [FromHeader] string Authorization)
        {
            return await ProcessRequestAsync("POST", all, body, Authorization).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(string all, [FromHeader] string Authorization)
        {
            return await ProcessRequestAsync("DELETE", all, null, Authorization).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPut]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> PutAsync(string all, [FromBody] object body, [FromHeader] string Authorization)
        {
            return await ProcessRequestAsync("PUT", all, body, Authorization).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPatch]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> PatchAsync(string all, [FromBody] object body, [FromHeader] string Authorization)
        {
            return await ProcessRequestAsync("PATCH", all, body, Authorization).ConfigureAwait(false);
        }

        private async Task<IActionResult> ProcessRequestAsync(string method, string all, object content, string Authorization)
        {
            // decode JWT Auth token
            string userToken = Authorization.Split(" ")[1];
            JwtSecurityToken authToken = new JwtSecurityTokenHandler().ReadJwtToken(userToken);

            string tenantId = "8d87ccfa-0037-44f0-ac71-31c71ac81a2b";
            string clientId = "8d87ccfa-0037-44f0-ac71-31c71ac81a2b";

            string errorContentType = "application/json";
            try
            {
                // Authentication provider using a generated application context token
                GraphServiceClient graphServiceClient = _graphAuthClient.GetAuthenticatedGraphClient(GetTokenAsync(tenantId).Result.ToString());

                // Processing the graph request
                GraphResponse processedGraphRequest = await ProcessGraphRequest(method, all, content, graphServiceClient);
                
                // Authentication provider using user's delegated token
                GraphServiceClient userGraphServiceClient = _graphAuthClient.GetAuthenticatedGraphClient(userToken);

                // Check if user owns the resource in question 
                bool userOwnership = await _graphService.VerifyOwnership(userGraphServiceClient, all, clientId);

                if (userOwnership) {
                    return new HttpResponseMessageResult(ReturnHttpResponseMessage(HttpStatusCode.OK, processedGraphRequest.contentType, new ByteArrayContent(processedGraphRequest.contentByteArray)));
                } else
                {
                    return new HttpResponseMessageResult(ReturnHttpResponseMessage(HttpStatusCode.Forbidden, errorContentType, new StringContent("")));
                }
            }
            catch (ServiceException ex)
            {
                return new HttpResponseMessageResult(ReturnHttpResponseMessage(ex.StatusCode, errorContentType, new StringContent(ex.Error.ToString())));
            }

        }

        struct GraphResponse
        {
            public string contentType;
            public byte[] contentByteArray;
        }

        private async Task<GraphResponse> ProcessGraphRequest(string method, string all, object content, GraphServiceClient graphClient)
        {
            var url = $"{GetBaseUrlWithoutVersion(graphClient)}/{all}{HttpContext.Request.QueryString.ToUriComponent()}";

            var request = new BaseRequest(url, graphClient, null)
            {
                Method = method,
                ContentType = HttpContext.Request.ContentType,
            };

            var neededHeaders = Request.Headers.Where(h => h.Key.ToLower() == "if-match" || h.Key.ToLower() == "consistencylevel").ToList();
            if (neededHeaders.Count() > 0)
            {
                foreach (var header in neededHeaders)
                {
                    request.Headers.Add(new HeaderOption(header.Key, string.Join(",", header.Value)));
                }
            }

            var contentType = "application/json";
           
            using (var response = await request.SendRequestAsync(content?.ToString(), CancellationToken.None, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
            {
                response.Content.Headers.TryGetValues("content-type", out var contentTypes);

                contentType = contentTypes?.FirstOrDefault() ?? contentType;

                var byteArrayContent = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                Console.WriteLine(byteArrayContent);

                return new GraphResponse
                {
                    contentByteArray = byteArrayContent,
                    contentType = contentType
                };
            }
        }

        // Acquire the application context access token.
        private async Task<string> GetTokenAsync(string tenantId)
        {
            string scopes = "https://graph.microsoft.com/.default";

            return await _tokenAcquisition.GetAccessTokenForAppAsync(scopes, tenantId, null);
        }

        private static HttpResponseMessage ReturnHttpResponseMessage(HttpStatusCode httpStatusCode, string contentType, HttpContent httpContent)
        {
            var httpResponseMessage = new HttpResponseMessage(httpStatusCode)
            {
                Content = httpContent
            };

            try
            {
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            catch
            {
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpResponseMessage;
        }

        private string GetBaseUrlWithoutVersion(GraphServiceClient graphClient)
        {
            var baseUrl = graphClient.BaseUrl;
            var index = baseUrl.LastIndexOf('/');
            return baseUrl.Substring(0, index);
        }

        public class HttpResponseMessageResult : IActionResult
        {
            private readonly HttpResponseMessage _responseMessage;

            public HttpResponseMessageResult(HttpResponseMessage responseMessage)
            {
                _responseMessage = responseMessage; // could add throw if null
            }

            public async Task ExecuteResultAsync(ActionContext context)
            {
                context.HttpContext.Response.StatusCode = (int)_responseMessage.StatusCode;

                foreach (var header in _responseMessage.Headers)
                {
                    context.HttpContext.Response.Headers.TryAdd(header.Key, new StringValues(header.Value.ToArray()));
                }

                context.HttpContext.Response.ContentType = _responseMessage.Content.Headers.ContentType.ToString();

                using (var stream = await _responseMessage.Content.ReadAsStreamAsync())
                {
                    await stream.CopyToAsync(context.HttpContext.Response.Body);
                    await context.HttpContext.Response.Body.FlushAsync();
                }
            }
        }


    }
}
