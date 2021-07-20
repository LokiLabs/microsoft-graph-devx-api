using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using GraphExplorerAppModeService.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Graph;
using System.Net;
using Microsoft.Identity.Client;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace GraphWebApi.Controllers
{
    [ApiController]
    public class GraphExplorerAppModeController : ControllerBase
    {
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly GraphServiceClient _graphServiceClient;
        public GraphExplorerAppModeController(ITokenAcquisition tokenAcquisition, GraphServiceClient graphServiceClient)
        {
            this.tokenAcquisition = tokenAcquisition;
            this._graphServiceClient = graphServiceClient;
        }
        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> GetAsync(string all)
        {
            return await ProcessRequestAsync("GET", all, null).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/token")]
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<string> GetTokenAsync(string all)
        {
            // Acquire the access token.
            string scopes = "https://graph.microsoft.com/.default";

            return await tokenAcquisition.GetAccessTokenForAppAsync(scopes);
        } 

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPost]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> PostAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("POST", all, body).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(string all)
        {
            try
            {
                string accessToken = GetTokenAsync("").Result.ToString();
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.DeleteAsync("https://graph.microsoft.com/v1.0/teams/8248c7dd-f773-40a5-b090-19386856ced3/channels/19:cfc8004ddb25441b8be2b7c5da02967a@thread.tacv2");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("http status code is ok");
                Console.WriteLine(response.ReasonPhrase);
                Console.WriteLine(response.Content);
                return Ok(response.ReasonPhrase);
            }
            catch (Exception exception)
            {
                Console.WriteLine("IT WENT INTO EXCEPTTTTTTTTT");
                return new JsonResult(exception) { StatusCode = 404 };
            }
            return null;
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPut]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> PutAsync(string all)
        {
            GraphServiceClient _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                    {
                        // Passing tenant ID to the sample auth provider to use as a cache key
                        string accessToken = GetTokenAsync("").Result.ToString();
                        // Append the access token to the request
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }));

            var qs = HttpContext.Request.QueryString;
            Console.WriteLine(HttpContext);

            var url = $"{GetBaseUrlWithoutVersion(_graphServiceClient)}/{all}{qs.ToUriComponent()}";

            Console.WriteLine("IS IT IN HERE");

            var request = new BaseRequest(url, _graphServiceClient, null)
            {
                Method = "DELETE",
                ContentType = HttpContext.Request.ContentType,
            };

            var contentType = "application/json";
            object content = null;
            try
            {
                using (var response = await request.SendRequestAsync(content?.ToString(), CancellationToken.None , HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    response.Content.Headers.TryGetValues("content-type", out var contentTypes);

                    contentType = contentTypes?.FirstOrDefault() ?? contentType;

                    var byteArrayContent = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    Console.WriteLine(byteArrayContent);
                    return new HttpResponseMessageResult(ReturnHttpResponseMessage(HttpStatusCode.OK, contentType, new ByteArrayContent(byteArrayContent)));
                }
            }
            catch (ServiceException ex)
            {
                return new HttpResponseMessageResult(ReturnHttpResponseMessage(ex.StatusCode, contentType, new StringContent(ex.Error.ToString())));
            }
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

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPatch]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<IActionResult> PatchAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("PATCH", all, body).ConfigureAwait(false);
        }

        private async Task<IActionResult> ProcessRequestAsync(string method, string all, object content)
        {
            return Ok();
        }
    }
}
