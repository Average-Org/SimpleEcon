using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace SimpleEcon
{
    public class Config
    {
		public string currencyNameSingular { get; set; } = "dollar";
		public string currencyNamePlural { get; set; } = "dollars";


		public bool giveRewardsForPlaytime { get; set; } = false;
		public int rewardtimer { get; set; } = 5;

		public void Write()
		{
			string path = Path.Combine(TShock.SavePath, "SimpleEcon.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
		public static Config Read()
		{
			string filepath = Path.Combine(TShock.SavePath, "SimpleEcon.json");
			try
			{
				Config config = new Config();

				if (!File.Exists(filepath))
				{
					File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));
				}
				config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filepath));


				return config;
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.ToString());
				return new Config();
			}
		}

	}
}
