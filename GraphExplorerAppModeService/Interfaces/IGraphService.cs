using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorerAppModeService.Interfaces
{
    public interface IGraphService
    {
        string ErrorMessage { get; set; }
        Task<bool> VerifyOwnership(GraphServiceClient graphClient, string query, string clientId);
    }
}
