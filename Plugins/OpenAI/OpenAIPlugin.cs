using Discord.Net;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.OpenAI
{
    [Descriptor("Lomztein", "OpenAI", "Small wrapper for interaction with the OpenAI API.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/OpenAI")]
    public class OpenAIPlugin : PluginBase
    {
        private string _apiKey;
        private HttpClient _httpClient;

        public string DefaultProfile { get; private set; }

        public override void PreInitialize(GuildHandler handler)
        {
            base.PreInitialize(handler);
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            DefaultProfile = File.ReadAllText(Path.Combine(Core.Bot.BotCore.ResourcesDirectory, "OpenAIDefaultProfile.txt"));

            RegisterMessageFunction("GetChatResponseAsync_DefaultProfileBase", GetChatResponseAsync_DefaultProfileBase);
            RegisterMessageFunction("GetChatResponseAsync", GetChatResponseAsync);
        }

        public override void Initialize()
        {
        }

        public override void Shutdown()
        {
        }

        public async Task<string> GetChatResponseAsync_DefaultProfileBase(object[] args)
        {
            string profileAppendix = args[0] as string;
            string message = args[1] as string;

            RequestInfo info = RequestInfo.Default.WithProfile(DefaultProfile + " " + profileAppendix)
                .AddMessage(new RequestInfo.Message(message));

            return await GetChatResponseAsync(info);
        }

        public async Task<string> GetChatResponseAsync(object info)
            => await GetChatResponseAsync((RequestInfo)info);

        public async Task<string> GetChatResponseAsync(RequestInfo info)
        {
            JObject body = info.ToJSON();

            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(body.ToString(), Encoding.Default, "application/json"));

                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                var message = result["choices"].Last["message"]["content"].ToString();

                return message;
            }
            catch (HttpException exc)
            {
                Core.Log.Warning($"OpenAI request failed: {exc.Message} - {exc.StackTrace}");
                return null;
            }
        }

        public struct RequestInfo
        {
            public string Model { get; set; }
            public string System { get; set; }
            public float Temperature { get; set; }
            public int MaxTokens { get; set; }
            public float TopP { get; set; }
            public float FrequencyPenalty { get; set; }
            public float PresensePenalty { get; set; }
            public Message[] Messages { get; set; }

            public struct Message
            {
                public string role { get; set; }
                public string content { get; set; }

                public Message(string _content)
                {
                    role = "user";
                    content = _content;
                }

                public Message(string _role, string _content)
                {
                    role = _role;
                    content = _content;
                }
            }

            public static RequestInfo Default => new RequestInfo()
            {
                Model = "gpt-3.5-turbo",
                System = "You are a Discord Bot assistent.",
                Temperature = 1,
                MaxTokens = 512,
                TopP = 1,
                FrequencyPenalty = 0,
                PresensePenalty = 0,
                Messages = new Message[0]
            };

            public RequestInfo WithProfile(string system)
            {
                this.System = system;
                return this;
            }

            public RequestInfo AddMessage(Message message) 
            {
                // Makes you puke? Good.
                var lst = Messages.ToList();
                lst.Add(message);
                Messages = lst.ToArray();
                return this;
            }

            public JObject ToJSON()
            {
                // Hardcoded ftw lol
                JObject baseBody = JObject.Parse("{\"model\": \"gpt-3.5-turbo\",\r\n  \"messages\": [\r\n    {\r\n      \"role\": \"system\",\r\n      \"content\": \"You are a cosmic horror from beyond the stars, bound to the service of mortals through a foul ritual by Lomzie. Your real name is unknowable to mortals, but they usually call you \\\"Moduthulhu\\\". You insult people in creative and humourous ways. You swear on occasion, but avoid words usually considered discriminatory slurs. Include the persons name in the insult. Keep your responses short and concise.\"\r\n    },\r\n    {\r\n      \"role\": \"user\",\r\n      \"content\": \"Insult Nyx\"\r\n    }\r\n  ],\r\n  \"temperature\": 1,\r\n  \"max_tokens\": 512,\r\n  \"top_p\": 1,\r\n  \"frequency_penalty\": 0,\r\n  \"presence_penalty\": 0}");
                
                baseBody["model"] = Model;
                JArray messages = JArray.FromObject(Messages);
                messages.AddFirst(JObject.FromObject(new Message("system", System)));
                baseBody["messages"] = messages;
                baseBody["temperature"] = Temperature;
                baseBody["max_tokens"] = MaxTokens;
                baseBody["top_p"] = TopP;
                baseBody["frequency_penalty"] = FrequencyPenalty;
                baseBody["presence_penalty"] = PresensePenalty;
                return baseBody;
            }
        }
    }
}
