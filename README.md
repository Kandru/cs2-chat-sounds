# CounterstrikeSharp - Chat Sounds

[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-chat-sounds?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-chat-sounds/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-map-modifier](https://img.shields.io/github/issues/Kandru/cs2-chat-sounds)](https://github.com/Kandru/cs2-chat-sounds/issues)
[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=C2AVYKGVP9TRG)

This will play the given chat sounds whenever a player types something into the chat and the word or one of the words he typed in matches the given chat sound name. More funny then simply using a command because player will trigger this when typing something random.

## Installation

1. Download and extract the latest release from the [GitHub releases page](https://github.com/Kandru/cs2-chat-sounds/releases/).
2. Move the "ChatSounds" folder to the `/addons/counterstrikesharp/plugins/` directory.
3. Restart the server.

Updating is even easier: simply overwrite all plugin files and they will be reloaded automatically. To automate updates please use our [CS2 Update Manager](https://github.com/Kandru/cs2-update-manager/).


## Configuration

This plugin automatically creates a readable JSON configuration file. This configuration file can be found in `/addons/counterstrikesharp/configs/plugins/ChatSounds/ChatSounds.json`.

```json
{
  "enabled": true,
  "debug": true,
  "cooldown_player": 0,
  "cooldown_global": 5,
  "play_on_player": true,
  "play_on_bots": false,
  "play_on_all_players": false,
  "sounds": {
    "de": {
      "1hp": {
        "path": "Saysound.1hp",
        "length": 0
      },
      "abgefahren": {
        "path": "Saysound.Abgefahren",
        "length": 0
      },
      "amerika": {
        "path": "Saysound.America",
        "length": 0
      }
    },
    "en": {
      "1hp": {
        "path": "Saysound.1hp",
        "length": 0
      },
      "america": {
        "path": "Saysound.America",
        "length": 0
      }
  },
  "muted": [
    "[U:1:33290010]"
  ],
  "ConfigVersion": 1
}
```

Sounds need to be categorized per language. If the player language is not found (and also not the server language) the first found language of the *sounds* dictionary will be used. This dictionary automatically sorts by alphabetically order to allow easier changes to the sound list. This will happen automatically each time the plugin gets loaded.

Sounds need to be inside an workshop addon sound definition file. Otherwise only default sounds from the cs2 internal sound definition files are possible to use.

## Commands

### chatsounds (Server Console Only)

Ability to run sub-commands:

#### reload

Reloads the configuration.

#### disable

Disables the sounds instantly and remembers this state.

#### enable

Enables the sounds instantly and remembers this state.

## Compile Yourself

Clone the project:

```bash
git clone https://github.com/Kandru/cs2-chat-sounds.git
```

Go to the project directory

```bash
  cd cs2-chat-sounds
```

Install dependencies

```bash
  dotnet restore
```

Build debug files (to use on a development game server)

```bash
  dotnet build
```

Build release files (to use on a production game server)

```bash
  dotnet publish
```

## FAQ

TODO

## License

Released under [GPLv3](/LICENSE) by [@Kandru](https://github.com/Kandru).

## Authors

- [@derkalle4](https://www.github.com/derkalle4)
- [@jmgraeffe](https://www.github.com/jmgraeffe)
