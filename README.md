# CS2 Rock The Vote
Players can type rtv to request the map to be changed, once a number of votes is reached (by default 60% of players in the server) the map will end in the end of the round and trigger a next map vote (that you need to configure yourself using CS2 built in nextmap vote system)

# Limitations
 - I haven't tested this with a server with more than 1 player, so this is a WIP version, I will address all feedback and issues that appear.
 - This is intended to be used alongside the built in map vote system in CS2 so you need to configure end of map vote in CS2 yourself. 
 - For now only English and Brazilian Portuguese are supported languages, adding translations require recompiling the plugin for now, this will likely change in the near future, feel fre to open a PR adding a new language, I will be glad to review and recompile the plugin myself.
  
# Requirements
[Latest release of Counter Strike Sharp](https://github.com/roflmuffin/CounterStrikeSharp)

# Instalation
- Download the latest release from https://github.com/abnerfs/cs2-rockthevote/releases
- Extract the .zip file into `addons/counterstrikesharp/plugins`
- Enjoy

# Config
- A config file will be created in `addons/counterstrikesharp/configs/plugins/RockTheVote` the first time you load the plugin.
- Changes in the config file will require you to reload the plugin or restart the server (change the map won't work).

```json
{
  "MinPlayers": 0, // Number of players required to enable the command
  "VotePercentage": 0.6, // Percentage of votes required to change the map
  "Language": "en", // The language, for now only en and pt are valid values
  "Version": 1 // Don't change this
}
```
