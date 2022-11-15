using System;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using TShockAPI.DB;
using SimpleEcon;
using System.Collections.Generic;

namespace SimpleEcon
{
    public class Database
    {
        private readonly IDbConnection _db;

        public Database(IDbConnection db)
        {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            var table = new SqlTable("SimpleEcon",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },
                new SqlColumn("Name", MySqlDbType.VarChar, 50) { Unique = true },
                new SqlColumn("Balance", MySqlDbType.Float)
                );
            sqlCreator.EnsureTableStructure(table);
        }

        public bool InsertPlayer(EconPlayer player)
        {
            return _db.Query("INSERT INTO SimpleEcon (Name, Balance)" + "VALUES (@0, @1)", player.name, 0) != 0;
        }

        public bool DeletePlayer(string playerName)
        {
            return _db.Query("DELETE FROM SimpleEcon WHERE Name = @0", playerName) != 0;
        }

        public bool SavePlayer(EconPlayer p)
        {
            EconPlayer player = PlayerManager.GetPlayer(p.name);

            return _db.Query("UPDATE SimpleEcon SET Balance = @0 WHERE Name = @1",
                player.balance, player.name) != 0;
        }

        public void SaveAllPlayers()
        {
            foreach (var player in SimpleEcon.econPlayers){
                SavePlayer(PlayerManager.GetPlayer(player.name));

            }
        }

        public bool userExists(EconPlayer player)
        {
            using (var reader = _db.QueryReader("SELECT * FROM SimpleEcon WHERE Name = @0", player.name))
            {
                while (reader.Read())
                {
                    var name = reader.Get<string>("Name");
                    var bal = reader.Get<float>("Balance");

                    return true;
                }
                Console.WriteLine("User did not exist! Creating economy for " + player.name);
                return false;
            }
        }
        public List<Tuple<string, float>> RetrieveBalTop()
        {
            SaveAllPlayers();
            List<Tuple<string, float>> p = new List<Tuple<string, float>>();

            using (var reader = _db.QueryReader("SELECT * FROM SimpleEcon ORDER BY balance DESC"))
            {
                while (reader.Read() && p.Count != 10)
                {
                    var name = reader.Get<string>("Name");
                    var bal = reader.Get<float>("Balance");

                    p.Add(new Tuple<string, float>(name, bal));
                }
                return p;
            }
        }

        public float getUserBalance(EconPlayer player)
        {
            SaveAllPlayers();
            List<Tuple<string, float>> p = new List<Tuple<string, float>>();

            using (var reader = _db.QueryReader("SELECT * FROM SimpleEcon WHERE Name = @0", player.name))
            {
                while (reader.Read() && p.Count != 10)
                {
                    var name = reader.Get<string>("Name");
                    var bal = reader.Get<float>("Balance");

                    return bal;

                }
                return 0;
            }
        }
    }
}