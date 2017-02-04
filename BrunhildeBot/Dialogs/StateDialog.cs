using System.Linq;
using BrunhildeBot.Models;

namespace BrunhildeBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class StateDialog : IDialog<object>
    {
        private const string HelpMessage = "\n * If you want to know which reddit I'm using for my searches type 'current reddit'. \n * Want to change the current reddit? Type 'change reddit to ...'. \n * Want to change it just for your searches? Type 'change my reddit to ....'";
        private bool userWelcomed;

        public async Task StartAsync(IDialogContext context)
        {
            string subReddit;

            if (!context.ConversationData.TryGetValue(ContextConstants.Key, out subReddit))
            {
                subReddit = "all";
                context.ConversationData.SetValue(ContextConstants.Key, subReddit);
            }

            await context.PostAsync($"Welcome to the Reddit Sentiment Search bot. I'm currently configured to search for things in {subReddit}");
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string userName;

            if (!context.UserData.TryGetValue(ContextConstants.UserNameKey, out userName))
            {
                PromptDialog.Text(context, this.ResumeAfterPrompt, "Before get started, please tell me your name?");
                return;
            }

            if (!this.userWelcomed)
            {
                this.userWelcomed = true;
                await context.PostAsync($"Welcome back {userName}! Remember the rules: {HelpMessage}");

                context.Wait(this.MessageReceivedAsync);
                return;
            }

            if (message.Text.Equals("current reddit", StringComparison.InvariantCultureIgnoreCase))
            {
                string subReddit;

                var defaultSub = context.ConversationData.Get<string>(ContextConstants.Key);

                if (context.PrivateConversationData.TryGetValue(ContextConstants.Key, out subReddit))
                {
                    await context.PostAsync($"{userName}, you have overridden the subReddit. Your searches are for things in  {subReddit}. The default conversation subReddit is {defaultSub}.");
                }
                else
                {
                    await context.PostAsync($"Hey {userName}, I'm currently configured to search for things in {subReddit}.");
                }
            }
            else if (message.Text.StartsWith("change reddit to", StringComparison.InvariantCultureIgnoreCase))
            {
                var newReddit = message.Text.Substring("change reddit to".Length).Trim();
                context.ConversationData.SetValue(ContextConstants.Key, newReddit);

                await context.PostAsync($"All set {userName}. From now on, all my searches will be for things in {newReddit}.");
            }
            else if (message.Text.StartsWith("change my reddit to", StringComparison.InvariantCultureIgnoreCase))
            {
                var newReddit = message.Text.Substring("change my reddit to".Length).Trim();
                context.PrivateConversationData.SetValue(ContextConstants.Key, newReddit);

                await context.PostAsync($"All set {userName}. I have overridden the reddit to {newReddit} just for you.");
            }
            else if (message.Text.StartsWith("find key", StringComparison.InvariantCultureIgnoreCase))
            {
                string reddit;
                var searchtext = message.Text.Substring("find key".Length).Trim();

                if (!context.PrivateConversationData.TryGetValue(ContextConstants.Key, out reddit))
                {
                    reddit = context.ConversationData.Get<string>(ContextConstants.Key);
                }

                await context.PostAsync($"{userName}, wait a few seconds. Searching for key '{searchtext}' in '{reddit}'...");

                using (var db = new Entities())
                {
                    var phrases = db.CommentKeyPhrases.Where(k => k.Phrase.Contains(searchtext)).Take(10);
                    
                    foreach (var p in phrases)
                    {
                        var comments = db.Comments.Where(c => c.RedditId == p.RedditId && ( c.SubReddit == reddit || reddit == "all" ) ).Take(10);
                        foreach (var c in comments)
                        {
                            await context.PostAsync($"\n * Sentiment : {c.Sentiment} \n * Key : {p.Phrase} \n * Comment : {c.Comment}");
                        }
                    }
                }
            }
            else if (message.Text.StartsWith("find happy", StringComparison.InvariantCultureIgnoreCase))
            {
                string reddit;
                var searchtext = message.Text.Substring("find happy".Length).Trim();

                if (!context.PrivateConversationData.TryGetValue(ContextConstants.Key, out reddit))
                {
                    reddit = context.ConversationData.Get<string>(ContextConstants.Key);
                }

                await context.PostAsync($"{userName}, wait a few seconds. Searching for happy comments in '{reddit}'...");

                using (var db = new Entities())
                {
                    var comments =
                        db.Comments.Where(c => c.Sentiment > 0.5 && (c.SubReddit == reddit || reddit == "all")).Take(10);

                    foreach (var c in comments)
                    {
                        await context.PostAsync($"\n * Sentiment : {c.Sentiment} \n * Comment : {c.Comment}");
                        
                    }
                }
            }
            else if (message.Text.StartsWith("find unhappy", StringComparison.InvariantCultureIgnoreCase))
            {
                string reddit;
                var searchtext = message.Text.Substring("find unhappy".Length).Trim();

                if (!context.PrivateConversationData.TryGetValue(ContextConstants.Key, out reddit))
                {
                    reddit = context.ConversationData.Get<string>(ContextConstants.Key);
                }

                await context.PostAsync($"{userName}, wait a few seconds. Searching for unhappy comments in '{reddit}'...");

                using (var db = new Entities())
                {
                    var comments =
                        db.Comments.Where(c => c.Sentiment < 0.5 && (c.SubReddit == reddit || reddit == "all")).Take(10);

                    foreach (var c in comments)
                    {
                        await context.PostAsync($"\n * Sentiment : {c.Sentiment} \n * Comment : {c.Comment}");

                    }
                }
            }
            else
            {
                await context.PostAsync($"I don't understand");
                await context.PostAsync($"Remember the rules: {HelpMessage}");
            }

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterPrompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var userName = await result;
                this.userWelcomed = true;

                await context.PostAsync($"Welcome {userName}! {HelpMessage}");

                context.UserData.SetValue(ContextConstants.UserNameKey, userName);
            }
            catch (TooManyAttemptsException)
            {
            }

            context.Wait(this.MessageReceivedAsync);
        }
    }
}