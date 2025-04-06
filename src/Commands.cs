using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Menu;

namespace ChatSounds
{
    public partial class ChatSounds
    {
        [ConsoleCommand("s", "ChatSounds")]
        [ConsoleCommand("sounds", "ChatSounds")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY, minArgs: 0, usage: "")]
        public void CommandSounds(CCSPlayerController player, CommandInfo command)
        {
            // close any active menu
            MenuManager.CloseActiveMenu(player);
            // create menu to choose sound
            var menu = new ChatMenu(Localizer["menu.title"]);
            // check if player is muted
            if (Config.Muted.Contains(player.NetworkIDString))
            {
                menu.AddMenuOption(Localizer["menu.unmute"], (_, _) => ToggleMute(player));
            }
            // check if player is on cooldown
            else if (_globalCooldown >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                || (_playerCooldowns.ContainsKey(player)
                && _playerCooldowns[player] >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
            {
                menu.AddMenuOption(Localizer["menu.cooldown"].Value
                    .Replace("{seconds}", Math.Max(
                        _globalCooldown - DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        _playerCooldowns.ContainsKey(player)
                            ? _playerCooldowns[player] - DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                            : 0
                    ).ToString()), (_, _) => { }, true);
            }
            else
            {
                // add option to mute
                menu.AddMenuOption(Localizer["menu.mute"], (_, _) => ToggleMute(player));
                // show player where sound will be played
                if (Config.PlayOnPlayer)
                {
                    menu.AddMenuOption(Localizer["menu.soundsplayedon.player"], (_, _) => { }, true);
                }
                else if (Config.PlayOnAllPlayers)
                {
                    menu.AddMenuOption(Localizer["menu.soundsplayedon.allplayers"], (_, _) => { }, true);
                }
                else
                {
                    menu.AddMenuOption(Localizer["menu.soundsplayedon.server"], (_, _) => { }, true);
                }
                // determine player language or fallback
                string lang = Config.Sounds.ContainsKey(playerLanguageManager.GetLanguage(new SteamID(player.SteamID)).TwoLetterISOLanguageName)
                    ? playerLanguageManager.GetLanguage(new SteamID(player.SteamID)).TwoLetterISOLanguageName
                    : Config.Sounds.ContainsKey(CoreConfig.ServerLanguage)
                        ? CoreConfig.ServerLanguage
                        : Config.Sounds.Keys.First();
                // add sounds to menu
                foreach (var (soundName, soundData) in Config.Sounds[lang])
                {
                    menu.AddMenuOption(soundName, (_, _) => CheckPlaySound(player, soundData.Path));
                }
            }
            // open menu
            MenuManager.OpenChatMenu(player, menu);
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
