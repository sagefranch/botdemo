using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Net;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

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
            for (int i = 0; i < checkedPhrase.Count(); i++)
            {
                Models.SpellCheck suggestedCorrection = checkedPhrase.ElementAt(i);
                suggestions += suggestedCorrection.suggestion;
                SearchResults.Add(new Models.SpellCol
                {
                    spellcol = suggestedCorrection
                });
            }
            var message = "";
            if (suggestions == "" || suggestions == null)
            {
                //if there are no suggestions, text is error-free
                //handle next step
                message = "You're so eloquent!";

            }
            else
            {
                message = suggestions;
            }
            await context.PostAsync($"{message}");
            context.Wait(MessageReceivedAsync);
        }

        public ObservableCollection<Models.SpellCol> SearchResults
        {
            get;
            set;
        } = new ObservableCollection<Models.SpellCol>();

        async Task<IEnumerable<Models.SpellCheck>> spellcheck(IMessageActivity received)
        {
            List<Models.SpellCheck> spellCheckRequest = new List<Models.SpellCheck>();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<YOUR API SUBSCRIPTION KEY HERE>");
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
                    suggestion = "Spelling Suggestion : " + data.flaggedTokens[i].suggestions[0].suggestion
                });
            }
            return spellCheckRequest;
        }
    }

}