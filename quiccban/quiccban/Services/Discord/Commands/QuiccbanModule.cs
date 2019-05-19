using Discord;
using Discord.Rest;
using Qmmands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Services.Discord.Commands
{
    public abstract class QuiccbanModule<T> : ModuleBase<T> where T : QuiccbanContext
    {
        public Task<RestUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Context.Channel.SendMessageAsync(text, isTTS, embed, options);

        public Task<RestUserMessage> SendEmbedAsync(Embed embed, RequestOptions options = null)
            => Context.Channel.SendMessageAsync(null, false, embed, options);

        public Task<RestUserMessage> SendEmbedAsync(EmbedBuilder embedBuilder, RequestOptions options = null)
            => Context.Channel.SendMessageAsync(null, false, embedBuilder.Build(), options);

        public Task<RestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Context.Channel.SendFileAsync(filePath, text, isTTS, embed, options);

        public Task<RestUserMessage> SendFileAsync(Stream stream, string fileName, string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Context.Channel.SendFileAsync(stream, fileName, text, isTTS, embed, options);

    }
}
