﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.BL;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono
{
    public class BobDono
    {
        private DiscordClient _client;
        private IBotBackbone _botBackbone;

        public static async Task Main(string[] args)
        {

            var prog = new BobDono();
            await prog.RunBotAsync();
        }

        private async Task RunBotAsync()
        {
            _client = new DiscordClient(new DiscordConfiguration
            {
                Token = Secrets.BotKey,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            });
            ResourceLocator.RegisterDependencies(_client);
            await _client.ConnectAsync();

            _client.MessageCreated += ClientOnMessageCreated;

            await Task.Delay(1000);
            _botBackbone = ResourceLocator.BotBackbone;
            _botBackbone.Initialize();
            await Task.Delay(-1);
        }
#pragma warning disable 4014
        private async Task ClientOnMessageCreated(MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Author.IsBot)
                return;

            foreach (var handlerEntry in _botBackbone.Handlers)
            {
                if (!handlerEntry.AreTypesEqual(typeof(MessageCreateEventArgs)))
                    continue;                
                try
                {
                    if (handlerEntry.Attribute.ParentModuleAttribute.IsChannelContextual)
                    {
                        foreach (var context in handlerEntry.Attribute.ParentModuleAttribute.Contexts)
                        {
                            if (handlerEntry.Predicates.All(predicate =>
                                predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs, context)))
                            {
                                if (handlerEntry.Attribute.Awaitable)
                                    await handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs,context);
                                else

                                    handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs,context);
                            }
                        
                        }
                    }
                    else
                    {
                        if (handlerEntry.Predicates.All(predicate =>
                            predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs)))
                        {
                            if (handlerEntry.Attribute.Awaitable)
                                await handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs);
                            else
                                handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs);
                        }
                    }

                }
                catch (Exception e)
                {
                    await messageCreateEventArgs.Channel.SendMessageAsync(ResourceLocator.ExceptionHandler.Handle(e));
                }
            }
        }
#pragma warning restore 4014
    }
}
