﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BobDono.Attributes;
using BobDono.Interfaces;
using BobDono.Utils;
using DSharpPlus.EventArgs;

namespace BobDono
{
    public static class CommandPredicates
    {
        private static HashSet<ulong> _authorizedUsers = new HashSet<ulong>
        {
            74458088760934400,
        };


        public static AuthorizedFilter Authorize { get; } = new AuthorizedFilter();
        public static ChannelFilter Channel { get;  } = new ChannelFilter();
        public static RegexFilter Regex { get;  } = new RegexFilter();


        public abstract class ArgumentPredicate<TArg> : ICommandPredicate where TArg : class
        {
            protected abstract bool MeetsCriteria(CommandHandlerAttribute attr,TArg arg);

            public bool MeetsCriteria(CommandHandlerAttribute attr,params object[] args)
            {
                return MeetsCriteria(attr,args.First() as TArg);
            }
        }

        public class AuthorizedFilter : ArgumentPredicate<MessageCreateEventArgs>
        {
            protected override bool MeetsCriteria(CommandHandlerAttribute attr, MessageCreateEventArgs arg)
            {
                return _authorizedUsers.Contains(arg.Author.Id);
            }
        }

        public class ChannelFilter : ArgumentPredicate<MessageCreateEventArgs>
        {
            protected override bool MeetsCriteria(CommandHandlerAttribute attr, MessageCreateEventArgs arg)
            {
                return arg.Channel.Id.Equals(attr.LimitToChannel);
            }
        }

        public class RegexFilter : ArgumentPredicate<MessageCreateEventArgs>
        {
            protected override bool MeetsCriteria(CommandHandlerAttribute attr, MessageCreateEventArgs arg)
            {
                return System.Text.RegularExpressions.Regex.IsMatch(arg.Message.Content, attr.Regex);
            }
        }
    }
}
