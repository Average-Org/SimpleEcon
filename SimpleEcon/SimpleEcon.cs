﻿using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace SimpleEcon
{
    [ApiVersion(2, 1)]
    public class SimpleEcon : TerrariaPlugin
    {
        public override string Name => "Simple Economy";
        public override Version Version => new Version(1, 0, 0);
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
            ServerApi.Hooks.NetGreetPlayer.Register(this, PlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, PlayerLeave);
            ServerApi.Hooks.GameInitialize.Register(this, Loaded);
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += Reloaded;
        }

        private void Loaded(EventArgs args)
        {
            config = Config.Read();
            Commands.ChatCommands.Add(new Command("se.user", Balance, "balance", "bal", "eco"));
            Commands.ChatCommands.Add(new Command("se.user", BalTop, "baltop", "ecotop", "top"));
            Commands.ChatCommands.Add(new Command("se.user", PayUser, "pay", "transfer"));
            Commands.ChatCommands.Add(new Command("se.admin", GiveBal, "givebal", "gbal"));

            rewardsManager();
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

        private void Balance(CommandArgs args)
        {
            EconPlayer player = PlayerManager.GetPlayer(args.Player.Name);
            float balance = dbManager.getUserBalance(player);

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
            
            if(amount <= PlayerManager.GetPlayer(args.Player.Name).balance)
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


        private void PlayerJoin(GreetPlayerEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];

            EconPlayer p = new EconPlayer(player.Name, player);
            econPlayers.Add(p);
            InitPlayerEcon(p);


        }

        public void InitPlayerEcon(EconPlayer p)
        {
            if (dbManager.userExists(p) == false)
            {
                dbManager.InsertPlayer(p);
                return;
            }

            p.balance = dbManager.getUserBalance(p);            
        }

        public void UpdatePlayer(EconPlayer p)
        {
            p.balance++;
            dbManager.SavePlayer(p);
        }

        private void PlayerLeave(LeaveEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];

            dbManager.SavePlayer(PlayerManager.GetPlayer(player.Name));
            econPlayers.Remove(new EconPlayer(player.Name, player));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
            base.Dispose(disposing);
        }
    }
}
