using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace SimpleEcon
{
    [ApiVersion(2, 1)]
    public class SimpleEcon : TerrariaPlugin
    {
        public override string Name => "Simple Economy";
        public override Version Version => new Version(1, 1, 0);
        public override string Author => "Average";
        public override string Description => "A simple, light-weight economy TShock V5 plugin that can also be utilized by other plugins. No bullshit dependecies!";
        public static List<EconPlayer> econPlayers = new List<EconPlayer>();
        private IDbConnection _db;
        public static Database dbManager;
        public static Config config = new Config();
        public SimpleEcon(Main game) : base(game)
        {

        }
        public override void Initialize()
        {
            switch (TShock.Config.Settings.StorageType.ToLower())
            {
                case "sqlite":
                    _db = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "SimpleEcon.sqlite")));
                    break;
                case "mysql":
                    try
                    {
                        var host = TShock.Config.Settings.MySqlHost.Split(':');
                        _db = new MySqlConnection
                        {
                            ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.Settings.MySqlDbName,
                            TShock.Config.Settings.MySqlUsername,
                            TShock.Config.Settings.MySqlPassword
                            )
                        };
                    }
                    catch (MySqlException ex)
                    {
                        TShock.Log.Error(ex.ToString());
                        throw new Exception("MySQL not setup correctly.");
                    }
                    break;
                default:
                    throw new Exception("Invalid storage type.");
            }
            dbManager = new Database(_db);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PlayerJoin;
            TShockAPI.GetDataHandlers.KillMe += PlayerDead;
            ServerApi.Hooks.ServerLeave.Register(this, PlayerLeave);
            ServerApi.Hooks.GameInitialize.Register(this, Loaded);
            ServerApi.Hooks.NetSendData.Register(this, OnNpcStrike);
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += Reloaded;
        }

        private void Loaded(EventArgs args)
        {
            config = Config.Read();
            Commands.ChatCommands.Add(new Command("se.user", Balance, "balance", "bal", "eco"));
            Commands.ChatCommands.Add(new Command("se.user", BalTop, "baltop", "ecotop", "top"));
            Commands.ChatCommands.Add(new Command("se.user", PayUser, "pay", "transfer"));
            Commands.ChatCommands.Add(new Command("se.admin", GiveBal, "givebal", "gbal"));
            Commands.ChatCommands.Add(new Command("se.admin", TakeBal, "takebal", "tbal"));
            Commands.ChatCommands.Add(new Command("se.admin", ResetBal, "resetbal", "rbal"));
            Commands.ChatCommands.Add(new Command("se.admin", SetBal, "setbal", "sbal"));

            rewardsManager();
        }

        public void PlayerDead(object sender, GetDataHandlers.KillMeEventArgs args)
        {
            if (args.Player.IsLoggedIn && config.DropOnDeath > 0)
            {
             
                EconPlayer p = PlayerManager.GetPlayer(args.Player.Name);
                var toLose = (float)(p.balance * config.DropOnDeath);
                p.balance -= toLose;
                if (config.announceMobDrops)
                {
                    args.Player.SendMessage($"You lost {toLose} {((toLose == 0) ? config.currencyNameSingular : config.currencyNamePlural) } from dying!", Color.Orange);
                    return;
                }
            }
            else
            {
                return;
            }
        }

        public async void rewardsManager()
        {
            if (config.giveRewardsForPlaytime == true)
            {
                await Task.Delay(config.rewardtimer * 60 * 1000);
                foreach (EconPlayer p in econPlayers)
                {
                    p.balance++;
                }
                dbManager.SaveAllPlayers();
                rewardsManager();
            }
        }
        public void OnNpcStrike(SendDataEventArgs args)
        {
                if (args.MsgId != PacketTypes.NpcStrike) { 
                return;
                }

                if (config.enableMobDrops == false)
            {
                return;
            }
            var npc = Main.npc[args.number];

            if (args.ignoreClient == -1)
            {
                return;
            }

            var player = TSPlayer.FindByNameOrID(args.ignoreClient.ToString())[0];
            Color color;

            if (!(npc.life <= 0))
            {
                return;
            }

                color = Color.Gold;

            if (npc.type != NPCID.TargetDummy && !npc.SpawnedFromStatue)
            {
        
                int totalGiven = 1;


                if (config.excludedMobs.Count > 0)
                {
                    foreach(var mob in config.excludedMobs)
                    {
                        if(npc.netID == mob)
                        {
                            return;
                        }
                    }
                }

                if (npc.netID == NPCID.EyeofCthulhu)
                {
                    totalGiven = 100;
                    color = Color.IndianRed;
                }

                if (npc.netID == NPCID.EaterofWorldsBody)
                {
                    totalGiven = 150;
                    color = Color.MediumPurple;
                }


                if (npc.netID == NPCID.SkeletronHead)
                {
                    totalGiven = 150;
                    color = Color.Gray;
                }


                if (npc.netID == NPCID.Skeleton)
                {
                    totalGiven = 3;
                    color = Color.Gray;
                }

                if (npc.netID == NPCID.Pinky)
                {
                    totalGiven = 1000;
                    color = Color.Pink;
                }

                if (npc.netID == NPCID.DemonEye)
                {
                    totalGiven = 2;
                    color = Color.DarkRed;
                }

                if (npc.netID == NPCID.Zombie)
                {
                    totalGiven = 2;
                    color = Color.DarkGreen;
                }

                if (npc.netID == NPCID.BlueSlime)
                {
                    totalGiven = 1;
                    color = Color.Blue;
                }

                if (npc.netID == NPCID.GreenSlime)
                {
                    totalGiven = 1;
                    color = Color.Green;
                }

                if (npc.netID == NPCID.RedSlime)
                {
                    totalGiven = 1;
                    color = Color.Red;
                }

                PlayerManager.GetPlayer(player.Name).balance += totalGiven;

                if(config.announceMobDrops == false)
                {
                    return;
                }


                if (totalGiven == 1)
                {
                    player.SendMessage("+ " + totalGiven + " " + config.currencyNameSingular + " from killing " + npc.FullName, color);
                }
                else
                {
                    player.SendMessage("+ " + totalGiven + " " + config.currencyNamePlural + " from killing " + npc.FullName, color);
                }
            }
        }
      


        private void Balance(CommandArgs args)
        {
            EconPlayer player = PlayerManager.GetPlayer(args.Player.Name);
            float balance = dbManager.getUserBalance(player.accountName);

            args.Player.SendMessage($"You currently have {balance} {(balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}", Color.LightGoldenrodYellow);
            return;
        }

        private void GiveBal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Please enter a player name! /gbal <user> <amount>");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage($"Please enter a quantity to send! /gbal {args.Parameters[0]} <amount>");
                return;
            }

            TSPlayer player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];
            if (player == null)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            float amount = float.Parse(args.Parameters[1]);
            PlayerManager.GetPlayer(player.Name).balance += amount;
            dbManager.SaveAllPlayers();
            player.SendSuccessMessage($"The moderator {args.Player.Name} has manipulated your currency and given you {amount} {(amount == 1 ? config.currencyNameSingular : config.currencyNamePlural)}! Your new balance is: {PlayerManager.GetPlayer(player.Name).balance} {(PlayerManager.GetPlayer(player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}");
            return;

        }
        private void SetBal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Please enter a player name! /sbal <user> <amount>");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage($"Please enter a quantity! /setbal {args.Parameters[0]} <amount>");
                return;
            }

            TSPlayer player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];
            if (player == null)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            float amount = float.Parse(args.Parameters[1]);
            PlayerManager.GetPlayer(player.Name).balance = amount;
            dbManager.SaveAllPlayers();
            player.SendSuccessMessage($"The moderator {args.Player.Name} has set your balance! Your new balance is: {PlayerManager.GetPlayer(player.Name).balance} {(PlayerManager.GetPlayer(player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}");
            return;

        }
        private void ResetBal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Please enter a player name! /resetbal <user>");
                return;
            }


            TSPlayer player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];
            if (player == null)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            float amount = float.Parse(args.Parameters[1]);
            PlayerManager.GetPlayer(player.Name).balance = 0;
            dbManager.SaveAllPlayers();
            player.SendErrorMessage($"The moderator {args.Player.Name} has reset your balance! Your new balance is: {PlayerManager.GetPlayer(player.Name).balance} {(PlayerManager.GetPlayer(player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}");
            return;

        }

        private void TakeBal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Please enter a player name! /tbal <user> <amount>");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage($"Please enter a quantity to take away from the player! /tbal {args.Parameters[0]} <amount>");
                return;
            }

            TSPlayer player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];
            if (player == null)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            float amount = float.Parse(args.Parameters[1]);
            PlayerManager.GetPlayer(player.Name).balance -= amount;
            dbManager.SaveAllPlayers();
            player.SendErrorMessage($"The moderator {args.Player.Name} has manipulated your currency and removed {amount} {(amount == 1 ? config.currencyNameSingular : config.currencyNamePlural)} from your account! Your new balance is: {PlayerManager.GetPlayer(player.Name).balance} {(PlayerManager.GetPlayer(player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}");
            return;

        }

        private void PayUser(CommandArgs args)
        {
            if(args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Please enter a player name! /pay <user> <amount>");
                return;
            }
            if(args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage($"Please enter a quantity to send! /pay {args.Parameters[0]} <amount>");
                return;
            }

            TSPlayer player = TSPlayer.FindByNameOrID(args.Parameters[0])[0];
            if (player == null)
            {
                args.Player.SendErrorMessage("Invalid player!");
                return;
            }
            float amount = float.Parse(args.Parameters[1]);
            if(amount < 1)
            {
                args.Player.SendErrorMessage("Please send a valid amount!");
                return;
            }

            if (amount <= PlayerManager.GetPlayer(args.Player.Name).balance)
            {
                PlayerManager.GetPlayer(args.Player.Name).balance -= amount;
                PlayerManager.GetPlayer(player.Name).balance += amount;
                dbManager.SaveAllPlayers();
                args.Player.SendSuccessMessage($"You successfully sent {amount} {(amount == 1 ? config.currencyNameSingular : config.currencyNamePlural)} to {player.Name}! Your remaining balance is: {PlayerManager.GetPlayer(args.Player.Name).balance} {(PlayerManager.GetPlayer(args.Player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNameSingular)}");
                player.SendSuccessMessage($"You have been sent {amount} {(amount == 1 ? config.currencyNameSingular : config.currencyNamePlural)} by {args.Player.Name}! Your new balance is: {PlayerManager.GetPlayer(player.Name).balance} {(PlayerManager.GetPlayer(player.Name).balance == 1 ? config.currencyNameSingular : config.currencyNamePlural)}");
                return;
            }
            else
            {
                args.Player.SendErrorMessage("You do not have enough money to make this transaction!");
                return;
            }
            return;
        }

        private void BalTop(CommandArgs args)
        {
            args.Player.SendMessage($"Top Users by Balance (/baltop)", Color.LightGoldenrodYellow);

            var balTop = dbManager.RetrieveBalTop();
            foreach (Tuple<string, float> p in balTop)
            {
                args.Player.SendMessage($"{balTop.IndexOf(p)+1}. {p.Item1} - {p.Item2}", Color.LightGreen);
            }

            return;
        }

        private void Reloaded(ReloadEventArgs e)
        {
            dbManager.SaveAllPlayers();
            config = Config.Read();
            rewardsManager();
        }


        private void PlayerJoin(PlayerPostLoginEventArgs args)
        {
            if(args.Player == null)
            {
                return;
            }
            if(args.Player.IsLoggedIn == false)
            {
                return;
            }

            TSPlayer player = args.Player;

            if (dbManager.userExists(player.Account.Name) == false)
            {
                EconPlayer p = new EconPlayer(player.Name, player);
                econPlayers.Add(p);
                dbManager.InsertPlayer(p);
                return;
            }

            var bal = dbManager.getUserBalance(player.Account.Name);
            EconPlayer o = new EconPlayer(player.Name, player, bal);
            econPlayers.Add(o);

            return;
        }


        public void UpdatePlayer(EconPlayer p)
        {
            p.balance++;
            dbManager.SavePlayer(p);
        }

        private void PlayerLeave(LeaveEventArgs args)
        {
            if(TShock.Players[args.Who] == null)
            {
                return;
            }

            TSPlayer player = TShock.Players[args.Who];

            if (PlayerManager.GetPlayer(player.Name) == null) {
                return;
            }


            dbManager.SavePlayer(PlayerManager.GetPlayer(player.Name));
            econPlayers.Remove(new EconPlayer(player.Name, player));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbManager.SaveAllPlayers();
                PlayerHooks.PlayerPostLogin -= PlayerJoin;
                ServerApi.Hooks.ServerLeave.Deregister(this, PlayerLeave);
                ServerApi.Hooks.GameInitialize.Deregister(this, Loaded);
                ServerApi.Hooks.NetSendData.Deregister(this, OnNpcStrike);
                TShockAPI.Hooks.GeneralHooks.ReloadEvent -= Reloaded;
            }
            base.Dispose(disposing);
        }
    }
}
