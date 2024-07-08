using fbognini.Notifications.Sinks.MTarget.Sdk.Models;
using fbognini.Sdk;
using fbognini.Sdk.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace fbognini.Notifications.Sinks.MTarget.Sdk
{

    public interface IMTargetService
    {
        void ChangeSettings(MTargetSettings settings);
        Task<SendMessagesResponse> SendMessages(List<string> phoneNumbers, string message);
    }

    public class MTargetUtils
    {
        internal static JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    internal class MTargetService : BaseApiService, IMTargetService
    {

        private MTargetSettings settings;

        public MTargetService(HttpClient client, IOptions<MTargetSettings> options)
            : base(client, options: MTargetUtils.JsonSerializerOptions)
        {
            settings = options.Value;
            ChangeSettings(settings);
        }

        public void ChangeSettings(MTargetSettings settings)
        {
            if (settings is null)
                return;

            client.BaseAddress = new Uri(settings.GetUrl());
            this.settings = settings;
        }

        public async Task<SendMessagesResponse> SendMessages(List<string> phoneNumbers, string message)
        {
            var collection = new List<KeyValuePair<string, string>>
            {
                new("username", settings.Username),
                new("password", settings.Password),
                new("sender", settings.Sender),
                new("msisdn", string.Join(',', phoneNumbers)),
                new("msg", message)
            };
            var content = new FormUrlEncodedContent(collection);
            return await PostApi<SendMessagesResponse>("messages", content);
        }
    }
}
