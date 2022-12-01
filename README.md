# Average's Simple Economy
Support me & this plugin's (along with several others) development on Ko.Fi: [Here!](https://ko-fi.com/averageterraria)

A super simple and lightweight tShock V5 plugin. This serves as a framework(ish) where other plugin developers can utilize this plugin to integrate a form of economy. There are also a few commands that this plugin implements. Everything you will need to know will be listed here! If I have missed anything, contact me on Discord: Average#1305

### Notes
- SQLite is currently the only usable DB type. If your TShock server utilizies MySQL, you are currently out of luck (this will be changed soon, however)
- Balances are stored in `tshock/SimpleEcon.sqlite`

## Config Explained
(tshock/SimpleEcon.json)

```json
{
  "currencyNameSingular": "dollar",
  "currencyNamePlural": "dollars",
  "excludedMobs": [
    211,
    210
  ],
  "enableMobDrops": true,
  "announceMobDrops": true,
  "giveRewardsForPlaytime": false,
  "rewardtimer": 5
  "DropOnDeath": 0.25
}

```
All of this is extremely simple, and intuitive. `currencyNameSingular` and `currencyNamePlural` dictate what the currency will be called within this plugin, and others!

`giveRewardsForPlaytime` will give the player a singular currency each time the `rewardTimer` goes off. To explain it better, if enabled, every <x> minutes (defined by `rewardTimer`), the player is given one currency!
 
 `excludedMobs` is a list of NPC ids that are excluded from dropping economy! Find the ID on the [Terraria Wiki!](https://terraria.fandom.com/wiki)
 
 `enableMobDrops` will drop economy when a mob is killed!
 
 `announceMobDrops` toggles whether or not it should send "+1 economy" to the player when they kill a mob or not!
 
 `DropOnDeath`, this is a value range of 0-1 (0.05, 0.1, 0.5, etc) and this value will be taken as a percentage. This means each time a player dies they lose that percentage. 0.5 = 50%, 0.05 = 5%! If the value is 0, players will not lose money on death.
 
## Commands List 

| Command        |Description           |Usage  |Permission    |
| ------------- |:-------------:| :-----:| :-----------:|
| /bal    |Shows the player's balance | /bal (Aliases: /eco, /balance) | se.user |
| /baltop    |Shows the top balances of the server | /baltop (Aliases: /ecotop, /top) | se.user |
| /transfer    |Takes money out of the user's account and sends it to another | /transfer `player` `quantity` (Alias: /pay) | se.user |
| /givebal    |**ADMIN COMMAND**: allows a user to add to the user's balance | /givebal `player` `quantity` (Alias: /gbal) | se.admin |
| /setbal    |**ADMIN COMMAND**: allows a user to set the user's balance to a value | /setbal `player` `quantity` (Alias: /sbal) | se.admin |
| /resetbal    |**ADMIN COMMAND**: allows a user to reset the user's balance to zero | /resetbal `player` (Alias: /rbal) | se.admin |
| /takebal    |**ADMIN COMMAND**: allows a user to take from the user's balance | /takebal `player` (Alias: /tbal) | se.admin |

## Plugin Dev Implementations
Simply add this plugin as a dependency for yours and you'll be able to use the following:

### Retrieving a user's balance
```c#

//Retrieve a user balance:
SimpleEcon.PlayerManager.GetPlayer(playerName).balance;

//Example - will check if user has certain amount
var p = SimpleEcon.PlayerManager.GetPlayer(playerName).balance;

if(SimpleEcon.PlayerManager.GetPlayer(playerName).balance >= 50) {
    // has enough
  }else{
    // not enough
  }

```

### Updating a user's balance
```c#

//Updating a user balance:
SimpleEcon.PlayerManager.UpdatePlayerBalance(playerName, amount);

//Example - will update a user balance to 5000
var pm = SimpleEcon.PlayerManager;

pm.UpdatePlayerBalance("John Doe", 5000);
//negative values ARE possible!

```

### Getting currency name
```c#

//Getting the singular currency name
SimpleEcon.SimpleEcon.config.currencyNameSingular;

//Getting the plural currency name
SimpleEcon.SimpleEcon.config.currencyNamePlural;

//Example - send message to user on money update 
float amount = 100;

player.SendSuccessMessage($"The moderator {moderator} has manipulated your currency and given you {amount} {(amount == 1 ? config.currencyNameSingular : config.currencyNamePlural)}! Your new balance is: {PlayerManager.GetPlayer(player.Name).balance} {(PlayerManager.GetPlayer(player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}");

```

### Some dev notes:
- Currencies are stored as a float, so decimal values are possible.
- There are many other methods that have been implemented for your use! Feel free to ask me any questions about implementations :)
