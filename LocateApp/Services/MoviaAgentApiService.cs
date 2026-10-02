using LocateApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LocateApp.DataTransferObjects;

namespace LocateApp.Services
{
    public static class MoviaAgentApiService
    {
        public async static Task<Token> LoginToMoviaAsync()
        {
            var baseUrl = $"{ConfigurationManager.AppSettings["MoviaApiBaseUrl"]}";
            var loginUrl = $"{ConfigurationManager.AppSettings["MoviaApiLoginUrl"]}";
            var userName = $"{ConfigurationManager.AppSettings["MoviaUserName"]}";
            var password = $"{ConfigurationManager.AppSettings["MoviaUserPassword"]}";

            var url = $"{baseUrl}{loginUrl}";
            var loginRequest = new LoginSendRequest { UserName = userName, Password = password };

            var response = await CallApiService.PostAsync(loginRequest, url);

            string result = response.Content.ReadAsStringAsync().Result;

            var objDefinition = new { Success = -1, Message = "", Token = "", Content = new List<GetMoviaIdentityRequest>()};

            var responseObject = JsonConvert.DeserializeAnonymousType(result, objDefinition);

            var token = JsonConvert.DeserializeObject<Token>(responseObject.Token);

            return token;
        }

        public static Agent GetAgentInfoFromMoviaAsync(string matricule, Token token)
        {
            var nip = $"{matricule}00";
            var baseUrl = $"{ConfigurationManager.AppSettings["MoviaApiBaseUrl"]}";
            var agentUrl = $"{ConfigurationManager.AppSettings["MoviaApiPatientUrl"]}";

            var url = $"{baseUrl}{agentUrl}{nip}";

            var response =  CallApiService.GetAsync(url, token);

            string result = response.Content.ReadAsStringAsync().Result;

            var definition = new { Success = -1, Message = "", Agent = new Agent() };

            var responseObject = JsonConvert.DeserializeAnonymousType(result, definition);


            return responseObject.Agent;
        }
    }
}