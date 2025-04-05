using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace ChatSounds
{
    public partial class ChatSounds : BasePlugin
    {
        public override string ModuleName => "CS2 ChatSounds";
        public override string ModuleAuthor => "Kalle <kalle@kandru.de>";

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
                || player.IsBot
                // skip if player is muted
                || Config.Muted.Contains(player.NetworkIDString)
                // skip if global cooldown is active
                || _globalCooldown >= DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                // skip if player cooldown is active
                || (_playerCooldowns.ContainsKey(player)
                    && _playerCooldowns[player] >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())) return HookResult.Continue;
            // find sound to play
            foreach (var sound in Config.Sounds)
                if (@event.Text.ToLower().Contains(sound.Key, StringComparison.CurrentCultureIgnoreCase))
                {
                    PlaySound(player, sound.Value.Path);
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
                        && !p.IsBot
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