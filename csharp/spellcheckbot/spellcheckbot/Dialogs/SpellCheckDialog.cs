using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace seesharpbot.Dialogs
{
    [Serializable]
    public class SpellCheckDialog : IDialog<object>
    {
        public async System.Threading.Tasks.Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var received = await argument;
            var checkedPhrase = await spellcheck(received);
            var suggestions = "";
            ObservableCollection<Models.SpellCol> SearchResults = new ObservableCollection<Models.SpellCol>();

            for (int i = 0; i < checkedPhrase.Count(); i++)
            {
                Models.SpellCheck suggestedCorrection = checkedPhrase.ElementAt(i);
                suggestions += suggestedCorrection.suggestion;
                SearchResults.Add(new Models.SpellCol
                {
                    spellcol = suggestedCorrection
                });
            }

            if (suggestions == "" || suggestions == null)
            {
                //if there are no suggestions, text is error-free
                //handle next step

            }
            else
            {
                PromptDialog.Confirm(
                      context,
                      AfterCheckAsync,
                      ($"Did you mean {suggestions}?"),
                      "Didn't get that!",
                      promptStyle: PromptStyle.None);
            }
        }

        public async Task AfterCheckAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
               await context.PostAsync("Okay! Logging that.");
            }
            else
            {
                await context.PostAsync("I'm sorry, I don't understand. Please enter your query again.");
                
            }

            context.Wait(MessageReceivedAsync);
        }

        async Task<IEnumerable<Models.SpellCheck>> spellcheck(IMessageActivity received)
        {
            List<Models.SpellCheck> spellCheckRequest = new List<Models.SpellCheck>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "YOUR BING API KEY HERE");
            string text = received.Text;
            string mode = "proof";
            string mkt = "en-us";
            var SpellEndPoint = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?";
            var result = await client.GetAsync(string.Format("{0}text={1}&mode={2}&mkt={3}", SpellEndPoint, text, mode, mkt));
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(json);
            for (int i = 0; i < data.flaggedTokens.Count; i++)
            {
                spellCheckRequest.Add(new Models.SpellCheck
                {
                    offset = "Offset : " + data.flaggedTokens[i].offset,
                    token = "Wrong Word : " + data.flaggedTokens[i].token,
                    suggestion = data.flaggedTokens[i].suggestions[0].suggestion
                });
            }
            return spellCheckRequest;
        }

    }
}