﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Text.Json.Serialization;

namespace ChatSounds
{
    public class Sounds
    {
        // sound path (either sounds/ path or soundevents name)
        [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
        // sound length in seconds
        [JsonPropertyName("length")] public float Length { get; set; } = 0;
    }

    public class PluginConfig : BasePluginConfig
    {
        // disabled
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // debug prints
        [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
        // cooldown for a player after playing a sound
        [JsonPropertyName("cooldown_player")] public int PlayerCooldown { get; set; } = 60;
        // global cooldown for all players after playing a sound
        [JsonPropertyName("cooldown_global")] public int GlobalCooldown { get; set; } = 5;
        // info messages
        [JsonPropertyName("sounds")] public Dictionary<string, Sounds> Sounds { get; set; } = [];
        // muted players
        [JsonPropertyName("muted")] public List<string> Muted { get; set; } = [];
    }

    public partial class ChatSounds : BasePlugin, IPluginConfig<PluginConfig>
    {
        public required PluginConfig Config { get; set; }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            // sort sounds by key
            Config.Sounds = Config.Sounds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            // update config and write new values from plugin to config file if changed after update
            Config.Update();
            Console.WriteLine(Localizer["core.config"]);
        }
    }
}
