using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;

namespace ChatSounds
{
    public partial class ChatSounds
    {
        [ConsoleCommand("sounds", "ChatSounds")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 1, usage: "<command>")]
        public void CommandSounds(CCSPlayerController player, CommandInfo command)
        {
            if (Config.Muted.Contains(player.NetworkIDString))
            {
                command.ReplyToCommand(Localizer["command.sounds.enabled"]);
            }
            else
            {
                command.ReplyToCommand(Localizer["command.sounds.disabled"]);
                Config.Muted.Add(player.NetworkIDString);
                Config.Update();
            }
        }

        [ConsoleCommand("chatsounds", "ChatSounds admin commands")]
        [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY, minArgs: 1, usage: "<command>")]
        public void CommandChatsounds(CCSPlayerController player, CommandInfo command)
        {
            string subCommand = command.GetArg(1);
            switch (subCommand.ToLower())
            {
                case "reload":
                    Config.Reload();
                    command.ReplyToCommand(Localizer["admin.reload"]);
                    break;
                case "disable":
                    Config.Enabled = false;
                    Config.Update();
                    command.ReplyToCommand(Localizer["admin.disable"]);
                    break;
                case "enable":
                    Config.Enabled = true;
                    Config.Update();
                    command.ReplyToCommand(Localizer["admin.enable"]);
                    break;
                default:
                    command.ReplyToCommand(Localizer["admin.unknown_command"].Value
                        .Replace("{command}", subCommand));
                    break;
            }
        }
    }
}
