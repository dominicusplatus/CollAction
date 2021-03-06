﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;

namespace CollAction.Helpers
{
    public class MailChimpManager
    {
        private class MailChimpErrorResponse
        {
            public string type { get; set; }
            public string title { get; set; }
            public int status { get; set; }
            public string detail { get; set; }
            public string instance { get; set; }
            public string[] errors { get; set; }

            public override string ToString()
            {
                StringBuilder errorResponse = new StringBuilder();
                errorResponse.AppendFormat("ERROR RESPONSE...\r", type);
                errorResponse.AppendFormat("type: {0}\r", type);
                errorResponse.AppendFormat("title: {0}\r", title);
                errorResponse.AppendFormat("status: {0}\r", status);
                errorResponse.AppendFormat("detail: {0}\r", detail);
                errorResponse.AppendFormat("instance: {0}\r", instance);
                errorResponse.AppendFormat("errors: {0}\r", string.Join(", ", errors ?? new string[0]));
                errorResponse.AppendFormat("\r");
                return errorResponse.ToString();
            }
        }

        public enum SubscriptionStatus { NotFound, Pending, Subscribed, Unknown };

        public struct ListMemberInfo { public string status; }

        private readonly string _apiKey;
        private readonly string _dataCenter;

        private Uri RootUri { get { return new Uri(String.Format("https://{0}.api.mailchimp.com/3.0", _dataCenter)); } }

        public MailChimpManager(string apiKey)
        {
            _apiKey = apiKey;
            _dataCenter = apiKey.Split('-')[1];
        }

        public async Task<SubscriptionStatus> GetListMemberStatusAsync(string listId, string email)
        {
            using (var client = GetClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(GetListMemberUri(listId, email) + "?fields=status"))
                {

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return SubscriptionStatus.NotFound;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        string errorResponse = await GetMailChimpErrorResponse(response);
                        throw new Exception("Failed to get MailChimp list member status. Status code: " + response.StatusCode + " -> \r" + errorResponse);
                    }

                    ListMemberInfo info = JsonConvert.DeserializeObject<ListMemberInfo>(await response.Content.ReadAsStringAsync());
                    switch (info.status)
                    {
                        case "pending": return SubscriptionStatus.Pending;
                        case "subscribed": return SubscriptionStatus.Subscribed;
                        default: return SubscriptionStatus.Unknown;
                    }
                }
            }
        }

        public async Task AddOrUpdateListMemberAsync(string listId, string email, bool usePendingStatusIfNew = true)
        {
            using (var client = GetClient())
            {
                StringContent content = new StringContent(GetAddOrUpdateListMemberParametersJSON(email, usePendingStatusIfNew), Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = await client.PutAsync(GetListMemberUri(listId, email), content))
                {

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        string errorResponse = await GetMailChimpErrorResponse(response);
                        string httpRequestHeader = client.DefaultRequestHeaders.ToString();
                        string httpRequestBaseAddress = client.BaseAddress.ToString();
                        string diagnostics = string.Format("{0}httpRequestHeader: {1}\rhttpRequestBaseAddress:{2}", errorResponse, httpRequestHeader, httpRequestBaseAddress);
                        throw new Exception("Failed to add or update MailChimp list member. Status code: " + response.StatusCode + " -> \r" + diagnostics);
                    }
                }
            }
        }

        public async Task DeleteListMemberAsync(string listId, string email)
        {
            using (var client = GetClient())
            {
                using (HttpResponseMessage response = await client.DeleteAsync(GetListMemberUri(listId, email)))
                {

                    if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.NotFound)
                    {
                        string errorResponse = await GetMailChimpErrorResponse(response);
                        throw new Exception("Failed to delete MailChimp list member. Status code: " + response.StatusCode + " -> \r" + errorResponse);
                    }
                }
            }
        }

        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _apiKey);
            client.BaseAddress = RootUri;
            return client;
        }

        private string GetListMemberUri(string listId, string email)
        {
            return String.Format("{0}/lists/{1}/members/{2}", RootUri, listId, GetMemberHash(email));
        }

        private string GetAddOrUpdateListMemberParametersJSON(string email, bool usePendingStatusIfNew)
        {
            var subscribeParameters = new {
                email_address = email,
                status_if_new = usePendingStatusIfNew ? "pending" : "subscribed"
            };
            return JsonConvert.SerializeObject(subscribeParameters);
        }

        private string GetMemberHash(string email)
        {
            string input = email.ToLower();
            using (MD5 cryptoService = MD5.Create())
            {
                byte[] data = cryptoService.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task<string> GetMailChimpErrorResponse(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<MailChimpErrorResponse>(await response.Content.ReadAsStringAsync()).ToString();
        }
    }
}
