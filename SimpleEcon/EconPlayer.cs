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

        public TSPlayer player { get; set; }

        public float balance { get; set; }

        public EconPlayer(string playerName, TSPlayer player)
        {
            this.name = playerName;
            this.player = player;

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

        public static void UpdatePlayerBalance(string name, float am)
        {
            var p = SimpleEcon.econPlayers.Find(p => p.name == name);
            p.balance = am;
            SimpleEcon.dbManager.SavePlayer(p);
            return;
        }

  
    }
}
