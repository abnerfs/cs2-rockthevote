# CS2 Rock The Vote
Players can type rtv to request the map to be changed, once a number of votes is reached (by default 60% of players in the server) a vote will start for the next map, this vote lasts up to 30 seconds (hardcoded for now), in the end server changes to the winner map.

# Features
- Reads from a custom maplist
- nominate command

# Limitations
 - I haven't tested this with a server with more than 1 player, so this is a WIP version, I will address all feedback and issues that appear.
 - Previous version relied on the official CS2 vote system, I pivoted this idea in favor of adding nominate, I will probably create another plugin with the original idea as soon as I figure out how to do the nominate command that way.
  
  
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
  "Version": 4,
  "RtvVotePercentage": 60,
  "RtvMinPlayers": 0,
  "DisableVotesInWarmup": false,
  "MapsToShowInVote": 5,
  "ChangeImmediatly": true,
  "MinRounds": 0
}
```

Maps that will be used in RTV are located in addons/counterstrikesharp/configs/plugins/RockTheVote/maplist.txt

# TODO
- [X] ~~Add minimum rounds to use commands.~~
- [ ] Add votemap.
- [x] ~~Translations support~~
- [ ] Add dont change option
- [x] ~~Nomination menu~~
- [ ] Add end of map vote
- [ ] Add timeleft command
- [ ] Add currentmap command
- [ ] Add nextmap command
