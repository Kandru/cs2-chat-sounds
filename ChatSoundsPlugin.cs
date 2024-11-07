using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.VisualBasic.CompilerServices;

namespace ChatSoundsPlugin;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("sounds")]
    public Dictionary<string, string> Sounds { get; set; } = new()
    {
        {"defuse", "sounds/vo/announcer/cs2_classic/bombdef"},
        {"plant", "sounds/vo/announcer/cs2_classic/bombpl"}
    };
    
    [JsonPropertyName("global_cooldown")]
    public int GlobalCooldown { get; set; } = 1;
    
    [JsonPropertyName("player_cooldown")]
    public int PlayerCooldown { get; set; } = 5;
}

public class ChatSoundsPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "Chat Sounds Plugin";
    public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it>";
    public override string ModuleVersion => "1.0.0";

    private DateTime _lastSound;
    private readonly Dictionary<SteamID, DateTime> _lastSoundByPlayer = new();
    private ChatMenu _menu = null!;
    
    public PluginConfig Config { get; set; } = null!;
    
    public void OnConfigParsed(PluginConfig? config)
    {
        if (config == null) return;
        Config = config;
        
        _menu = new ChatMenu(Localizer["chatMenuTitle"]);
        foreach (var sound in config.Sounds)
        {
            _menu.AddMenuOption(sound.Key, (player, option) =>
            {
                PlaySound(option.Text, player);
            });
        }
    }
    
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerChat>((@event, _) =>
        {
            var sender = Utilities.GetPlayerFromUserid(@event.Userid);
            if (sender == null || !Config.Sounds.ContainsKey(@event.Text)) return HookResult.Continue;
            
            PlaySound(@event.Text, sender);
 
            return HookResult.Continue;
        });
        
        RegisterListener<Listeners.OnMapStart>(_ =>
        {
            _lastSoundByPlayer.Clear();
        });
        
        AddCommand("css_sounds", "Play a funny or not-so-funny sound to all players on the server", (player, _) =>
        {
            if (player == null) return;
            _menu.Open(player);
        });
    }

    private void PlaySound(string name, CCSPlayerController sender)
    {
        if (sender.AuthorizedSteamID == null) return;
        
        // cooldown checks
        var secondsSinceLastSound = (DateTime.Now - _lastSound).TotalSeconds;
        if (secondsSinceLastSound <= Config.GlobalCooldown)
        {
            sender.PrintToChat(Localizer["globalCooldownActive"].Value.Replace("{seconds}",
                Math.Ceiling(Config.GlobalCooldown - secondsSinceLastSound).ToString(CultureInfo.CurrentCulture)));
            return;
        }
        secondsSinceLastSound = _lastSoundByPlayer.TryGetValue(sender.AuthorizedSteamID, out var value)
            ? (DateTime.Now - value).TotalSeconds
            : double.MaxValue;
        if (secondsSinceLastSound <= Config.PlayerCooldown)
        {
            sender.PrintToChat(Localizer["playerCooldownActive"].Value.Replace("{seconds}",
                Math.Ceiling(Config.PlayerCooldown - secondsSinceLastSound).ToString(CultureInfo.CurrentCulture)));
            return;
        }
                
        foreach (var player in Utilities.GetPlayers())
        {
            player.ExecuteClientCommand($"play \"{Config.Sounds[name]}\"");
        }

        _lastSound = DateTime.Now;
        _lastSoundByPlayer[sender.AuthorizedSteamID] = DateTime.Now;
    }
}