using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using System.Globalization;

namespace ChatSounds
{
    public partial class ChatSounds : BasePlugin
    {
        public override string ModuleName => "CS2 ChatSounds";
        public override string ModuleAuthor => "Kalle <kalle@kandru.de>";

        private readonly PlayerLanguageManager playerLanguageManager = new();
        private Dictionary<CCSPlayerController, long> _playerCooldowns = [];
        private long _globalCooldown = 0L;

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerChat>(OnPlayerChatCommand);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        }

        public override void Unload(bool hotReload)
        {
            DeregisterEventHandler<EventPlayerChat>(OnPlayerChatCommand);
            DeregisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        }

        private HookResult OnPlayerChatCommand(EventPlayerChat @event, GameEventInfo info)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromUserid(@event.Userid);
            if (!Config.Enabled
                || player == null
                || !player.IsValid
                || player.IsBot) return HookResult.Continue;
            // check if player changed language
            if (@event.Text.StartsWith("!lang", StringComparison.OrdinalIgnoreCase))
            {
                // get language from command instead of player because it defaults to english all the time Oo
                string? language = @event.Text.Split(' ').Skip(1).FirstOrDefault()?.Trim();
                if (language == null
                    || !CultureInfo.GetCultures(CultureTypes.AllCultures).Any(c => c.Name.Equals(language, StringComparison.OrdinalIgnoreCase)))
                    return HookResult.Continue;
                // set language for player
                playerLanguageManager.SetLanguage(new SteamID(player.SteamID), new CultureInfo(language));
                return HookResult.Continue;
            }
            // skip if player is muted
            if (Config.Muted.Contains(player.NetworkIDString)) return HookResult.Continue;
            // find sound to play
            foreach (var kvp in Config.Sounds)
                foreach (var sound in Config.Sounds[kvp.Key])
                    if (@event.Text.ToLower().Contains(sound.Key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (@event.Text.Split(' ').Length > 1)
                        {
                            if (_globalCooldown >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                                // skip if player cooldown is active
                                || (_playerCooldowns.ContainsKey(player)
                                    && _playerCooldowns[player] >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
                                break;
                            PlaySound(player, sound.Value.Path);
                        }
                        else
                        {
                            CheckPlaySound(player, sound.Key, sound.Value.Path);
                        }
                        break;
                    }
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || !player.IsValid
                || !_playerCooldowns.ContainsKey(player)) return HookResult.Continue;
            // remove player from cooldowns if applicable
            _playerCooldowns.Remove(player);
            return HookResult.Continue;
        }

        private void CheckPlaySound(CCSPlayerController player, string command, string sound)
        {
            if (_globalCooldown >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                || (_playerCooldowns.ContainsKey(player)
                && _playerCooldowns[player] >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
            {
                Server.NextFrame(() =>
                {
                    if (player == null
                        || !player.IsValid) return;
                    player.PrintToChat(Localizer["command.sounds.cooldown"].Value
                        .Replace("{seconds}", Math.Max(
                            _globalCooldown - DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            _playerCooldowns.ContainsKey(player)
                                ? _playerCooldowns[player] - DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                                : 0
                        ).ToString()));
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    if (player == null
                        || !player.IsValid) return;
                    Server.PrintToChatAll(Localizer["command.sounds.played"].Value
                        .Replace("{player}", player.PlayerName)
                        .Replace("{sound}", command));
                });

                PlaySound(player, sound);
            }
        }

        private void PlaySound(CCSPlayerController player, string sound)
        {
            DebugPrint($"[ChatSounds] Playing sound {sound} for player {player.PlayerName}.");
            // prepare recipient filter (to avoid playing sounds for muted players)
            RecipientFilter filter = [];
            foreach (var entry in Utilities.GetPlayers().Where(
                p => p.IsValid
                    && !p.IsBot
                    && !Config.Muted.Contains(p.NetworkIDString)).ToList())
                filter.Add(entry);
            if (Config.PlayOnPlayer && player.PawnIsAlive)
            {
                player.EmitSound(sound, filter);
                DebugPrint("[ChatSounds] Playing sound on player.");
            }
            else if (Config.PlayOnAllPlayers && player.PawnIsAlive)
            {
                foreach (var entry in Utilities.GetPlayers().Where(
                    p => p.IsValid
                        && (!p.IsBot || Config.PlayOnBots)
                        && !Config.Muted.Contains(p.NetworkIDString)).ToList())
                    entry.EmitSound(sound, filter);
                DebugPrint("[ChatSounds] Playing sound on all players.");
            }
            else
            {
                // get world entity
                CWorld? worldEnt = Utilities.FindAllEntitiesByDesignerName<CWorld>("worldent").FirstOrDefault();
                if (worldEnt == null
                    || !worldEnt.IsValid)
                {
                    DebugPrint("[ChatSounds] Could not find world entity.");
                    return;
                }
                DebugPrint("[ChatSounds] Playing sound on world entity.");
                // play sound
                worldEnt.EmitSound(sound, filter);
            }
            // update cooldowns
            _globalCooldown = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Config.GlobalCooldown;
            if (_playerCooldowns.ContainsKey(player))
                _playerCooldowns[player] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Config.PlayerCooldown;
            else
                _playerCooldowns.Add(player, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Config.PlayerCooldown);
        }
    }
}