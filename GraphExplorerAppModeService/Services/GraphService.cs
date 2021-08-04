using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphExplorerAppModeService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace GraphExplorerAppModeService.Services
{
    public class GraphService : IGraphService
    {
        private string errorMessage;

        // Docs defined in IGraphService
        public string ErrorMessage { get; set; }

        public async Task<bool> VerifyOwnership(GraphServiceClient graphClient, string query, string clientId)
        {
            string[] queryList = query.Split("/");

            for (int i=0; i < queryList.Length; i++)
            {
                if (queryList[i] == "teams" && i + 1 < queryList.Length)
                {
                    return await VerifyTeamsOwnership(graphClient, queryList[i + 1], clientId);
                } else if (queryList[i] == "chats" && i + 1 < queryList.Length)
                {
                    return await VerifyChatOwnership(graphClient, queryList[i + 1], clientId);
                }
            }

            return false;
        }

        private async Task<bool> VerifyTeamsOwnership(GraphServiceClient graphClient, string teamId, string clientId)
        {
            try
            {
                var owners = await graphClient.Groups[teamId].Owners.Request().GetAsync();

                foreach (var owner in owners)
                {
                    if (clientId == owner.Id) return true;
                }
            }
            catch (ServiceException e)
            {
                ErrorMessage = e.Message;
            }

            return false;
        }

        private async Task<bool> VerifyChatOwnership(GraphServiceClient graphClient, string chatId, string clientId)
        {
            try
            {
                var members = await graphClient.Chats[chatId].Members.Request().GetAsync();

                foreach (AadUserConversationMember member in members)
                {
                    if (clientId == member.UserId && member.Roles.Contains("owner")) return true;
                }
            }
            catch (ServiceException e)
            {
                ErrorMessage = e.Message;
            }

            return false;
        }
    }
}
