using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;

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
            string text = @event.Text.ToLower();
            foreach (var sound in Config.Sounds)
            {
                if (text.Contains(sound.Key, StringComparison.CurrentCultureIgnoreCase))
                {
                    // get world entity
                    CWorld? worldEnt = Utilities.FindAllEntitiesByDesignerName<CWorld>("worldent").FirstOrDefault();
                    if (worldEnt == null
                        || !worldEnt.IsValid) return HookResult.Continue;
                    // play sound
                    worldEnt.EmitSound(sound.Value.Path);
                    // update cooldowns
                    _globalCooldown = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Config.GlobalCooldown;
                    if (_playerCooldowns.ContainsKey(player))
                        _playerCooldowns[player] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Config.PlayerCooldown;
                    else
                        _playerCooldowns.Add(player, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Config.PlayerCooldown);
                    // only play first sound found
                    break;
                }
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
    }
}