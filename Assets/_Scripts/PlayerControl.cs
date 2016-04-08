using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class PlayerControl : MonoBehaviour{
	string champName;

	// Initializing of the necessary Textfields
	[SerializeField]
	Text
		Header;
	[SerializeField]
	Text
		LastChamp;
	[SerializeField]
	Text
		Role;
	[SerializeField]
	Text
		Residence;
	[SerializeField]
	Text
		KDA;
	[SerializeField]
	Text
		CS;
	[SerializeField]
	Text
		Gold;
	[SerializeField]
	Text
		SummonerName;
	[SerializeField]
	Text
		Team;

	const string _filename = "LeagueDB.s3db";
	private IDbConnection _dbcon;
	private IDbCommand _dbcmd;
	private IDataReader _dbr;
	
	// Function for starting the database connection
	public void StartConnection (string filename, string query){
		
		/* Need to check if the File already exists on the device
		 * If it doesn't, it needs to be created from the database file saved in StreamingAssets */
		string filepath = Application.persistentDataPath + "/" + filename;
		if (!File.Exists (filepath)) {
			WWW loadDB = new WWW ("jar:file://" + Application.dataPath + "!/assets/" + filename);
			
			while (!loadDB.isDone) {
			}
			
			File.WriteAllBytes (filepath, loadDB.bytes);
		}
		
		// After checking if the file exists, create a new sqlite connection AND open database
		_dbcon = new SqliteConnection ("URI=file:" + filepath);
		_dbcon.Open ();
		
		// Start Reader AND pass query to reader
		_dbcmd = _dbcon.CreateCommand ();
		_dbcmd.CommandText = query;
		_dbr = _dbcmd.ExecuteReader ();
	}
	
	public void CloseConnection (){
		_dbr.Close (); 
		_dbcmd.Dispose ();
		_dbcon.Close ();
	}
	
	void Start (){
		// First retrieve the Name of the summoner from PlayerPrefs
		string summonerName = PlayerPrefs.GetString ("Summoner");

		// Query uses Name of the Summoner to return the Full Name, Role, Residence, # of kills, deaths, assists, the total CS, Gold and the last champion that was played
		string query = "select P.SummonerName, P.Role, P.Residence, P.FirstName, P.LastName, tot.Kills, tot.Deaths,  " +
			"tot.Assists, tot.CS, tot.Gold, lc.ChampionName as LastChamp " +
			"from Players P," +
			"(Select PS.PlayerID, SUM(PS.Kills) as Kills, Sum(PS.Deaths) as Deaths, SUM(PS.Assists) as " +
			"Assists, SUM(PS.CreepScore) as CS, SUM(PS.Gold) as Gold " +
			"From PlayerStats PS " +
			"Where not PS.ChampionID = 0 " +
			"Group by PS.PlayerID) as  tot, " +
			"(select PS.PlayerID, C.ChampionName " +
			"from PlayerStats PS, Champions C, (Select PS.PlayerID, Max(M.MatchID) as MatchID " +
			"From PlayerStats PS, Match M " +
			"Where not PS.ChampionID = 0 " +
			"and PS.MatchID = M.MatchID " +
			"group by PS.PlayerID) latest " +
			"where PS.MatchID = latest.MatchID " +
			"and PS.PlayerID = latest.PlayerID " +
			"and PS.ChampionID = C.ChampID) as lc " +
			"Where P.PlayerID = tot.PlayerID " +
			"and P.PlayerID = lc.PlayerID " +
			"and P.SummonerName = \"" + summonerName + "\";";

		StartConnection (_filename, query);

		// Retrieve Data from DB
		for (int i = 0; _dbr.Read(); i++) {
			SummonerName.text = _dbr.GetString (0);
			Role.text = _dbr.GetString (1);
			Residence.text = _dbr.GetString (2);
			Header.text = _dbr.GetString (3) + " " + _dbr.GetString (4);
			KDA.text = _dbr.GetString (5) + "/" + _dbr.GetString (6) + "/" + _dbr.GetString (7);
			CS.text = _dbr.GetString (8);
			Gold.text = _dbr.GetString (9) + "k";
			champName = _dbr.GetString (10);
			LastChamp.text = champName;
		}
		CloseConnection ();
	}

	// Button function for clicking on a champion, loads the correct scene and saves the last played champion to use it in a query in that scene
	public void clickOnChampion (){
		PlayerPrefs.SetString ("Champion", champName);
		Application.LoadLevel ("championPool");
	}
}
