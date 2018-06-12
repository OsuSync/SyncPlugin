using Sync.Source;
using System;
using System.Reflection;
using System.Linq;
using Sync.MessageFilter;
using Sync.Command;
using Sync.Tools;
using Sync.Plugins;
using System.Diagnostics;
using System.Globalization;
using DefaultPlugin.Sources.BiliBili;
using DefaultPlugin.Clients;
using static Sync.Tools.IO;
using static DefaultPlugin.DefaultPlugin;
using static DefaultPlugin.Language;

namespace DefaultPlugin.Commands
{
    class BaseCommand
    {
        public BaseCommand(CommandManager manager)
        {
            manager.Dispatch.bind("setbili", setBilibili, LANG_COMMANDS_BILIBILI);
            manager.Dispatch.bind("setosubot", setosubot, LANG_COMMANDS_SET_OSU_BOT);
        }

        private bool setBilibili(Arguments arg)
        {
            if (arg.Count == 0) return false;
            BiliBili.RoomID = arg[0];
            return true;
        }

        private bool setosubot(Arguments arg)
        {

            if (arg.Count == 0)
            {
                CurrentIO.WriteColor(string.Format(LANG_COMMANDS_BOTIRC_CURRENT, DirectOSUIRCBot.IRCBotName), ConsoleColor.Cyan);
                CurrentIO.WriteColor(string.Format(LANG_COMMANDS_IRC_CURRENT, DirectOSUIRCBot.IRCNick), ConsoleColor.Cyan);
                return true;
            }
            if (arg.Count < 3) return false;

            DirectOSUIRCBot.IRCBotName = arg[0];
            DirectOSUIRCBot.IRCBotPasswd = arg[1];
            DirectOSUIRCBot.IRCNick = arg[2];

            return true;
        }
    }
}
