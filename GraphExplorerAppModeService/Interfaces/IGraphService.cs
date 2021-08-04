using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorerAppModeService.Interfaces
{
    /// <summary>
    /// This class is for functions that call Microsoft Graph to check if the client owns the Teams resource they are trying to call against.
    /// </summary>
    public interface IGraphService
    {
        /// <summary>
        /// ErrorMessage will contain the error message from the Graph Client call.
        /// </summary>
        string ErrorMessage { get; set; }

        Task<bool> VerifyOwnership(GraphServiceClient graphClient, string query, string clientId);
    }
}
