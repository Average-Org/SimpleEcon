using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace SimpleEcon
{
    public class EconPlayer
    {
        public string name { get; set; }

        public string accountName {  get; set; }

        public TSPlayer player { get; set; }

        public float balance { get; set; }

        public float RetrieveBalance()
        {
            return this.balance;
        }

        public EconPlayer(string playerName, TSPlayer player)
        {
            this.name = playerName;
            this.player = player;
            this.accountName = player.Account.Name;
        }

        public EconPlayer(string playerName, TSPlayer player, float bal)
        {
            this.name = playerName;
            this.player = player;
            this.balance = bal;
            this.accountName = player.Account.Name;
        }

    }

    public static class PlayerManager
    {
        public static EconPlayer GetPlayer(int playerId)
        {
            if(playerId == null)
            {
                return null;
            }

            if (TShock.Players[playerId] == null)
            {
                return null;
            }

            var name = TShock.Players[playerId].Name;


            return SimpleEcon.econPlayers.Find(p => p.name == name);
        }
        public static EconPlayer GetPlayer(string name)
        {
             return SimpleEcon.econPlayers.Find(p => p.name == name);
        }

        public static EconPlayer GetPlayerFromAccount(string name)
        {
            return SimpleEcon.econPlayers.Find(p => p.accountName == name);
        }


        public static void UpdatePlayerBalance(string name, float am)
        {
            var p = SimpleEcon.econPlayers.Find(p => p.name == name);
            p.balance = am;
            SimpleEcon.dbManager.SavePlayer(p);
            return;
        }


        public static void UpdatePlayerBalance(EconPlayer ply, float am)
        {
            var p = SimpleEcon.econPlayers.Find(p => p.name == ply.name);
            p.balance = am;
            SimpleEcon.dbManager.SavePlayer(p);
            return;
        }

        public static void SubtractPlayerBalance(EconPlayer ply, float toRemove)
        {
            var p = SimpleEcon.econPlayers.Find(p => p.name == ply.name);
            p.balance -= toRemove;
            if(p.balance <= 0)
            {
                p.balance = 0;
            }
            SimpleEcon.dbManager.SavePlayer(p);
            return;
        }

        public static void AddPlayerBalance(EconPlayer ply, float toAdd)
        {
            var p = SimpleEcon.econPlayers.Find(p => p.name == ply.name);
            p.balance += toAdd;
            if (p.balance <= 0)
            {
                p.balance = 0;
            }
            SimpleEcon.dbManager.SavePlayer(p);
            return;
        }

        public static void ResetPlayerBalance(EconPlayer ply)
        {
            var p = SimpleEcon.econPlayers.Find(p => p.name == ply.name);
            p.balance = 0;
            SimpleEcon.dbManager.SavePlayer(p);
            return;
        }

    }
}
