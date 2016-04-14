using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class addMatch : MonoBehaviour
{
	[SerializeField]
	InputField TeamName;

	[SerializeField]
	InputField Length;

	[SerializeField]
	InputField Date;

	[SerializeField]
	InputField HighlightLink;

	[SerializeField]
	InputField MatchLink;

	[SerializeField]
	InputField Kills;

	[SerializeField]
	InputField Deaths;

	[SerializeField]
	InputField Assists;

	[SerializeField]
	InputField Gold;

	[SerializeField]
	InputField Dragons;

	[SerializeField]
	InputField Barons;

	[SerializeField]
	InputField Towers;

	[SerializeField]
	Toggle Won;

	[SerializeField]
	InputField Ban1;

	[SerializeField]
	InputField Ban2;

	[SerializeField]
	InputField Ban3;

	[SerializeField]
	Button AddStats;

	[SerializeField]
	Button AddPlayer;

	[SerializeField]
	InputField SummonerName;

	[SerializeField]
	InputField PKills;

	[SerializeField]
	InputField PDeaths;

	[SerializeField]
	InputField PAssists;

	[SerializeField]
	InputField PGold;

	[SerializeField]
	InputField Creeps;

	[SerializeField]
	InputField ChampionName;

	[SerializeField]
	Animator PlayerSlide;

	[SerializeField]
	Button CancelButton;

	const string _filename = "LeagueDB.s3db";
	private IDbConnection _dbcon;
	private IDbCommand _dbcmd;
	private IDataReader _dbr;

	int MatchHolder;
	bool team1finished = false;
	bool wonMatch = false;
	bool teamStats = true;
	int playerCounter = 1;
	string teamNameHolder = "";


	public void StartConnection (string filename, string query)
	{
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


		if (teamStats) {
			// Start Reader AND pass query to reader
			_dbcmd = _dbcon.CreateCommand ();
			_dbcmd.CommandText = query;
			_dbr = _dbcmd.ExecuteReader ();

			for (int i = 0; _dbr.Read (); i++) {
				MatchHolder = _dbr.GetInt32 (0);
			}

			Debug.Log (MatchHolder);
		} else {
			_dbcmd = _dbcon.CreateCommand ();
			_dbcmd.CommandText = query;
			_dbcmd.ExecuteNonQuery ();
		}
	}

	public void CloseConnection ()
	{
		//_dbr.Close (); 
		_dbcmd.Dispose ();
		_dbcon.Close ();
	}


	public void StartConnectionWithReadQuery (string filename, string query)
	{
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

	public void CloseConnectionWithReader ()
	{
		_dbr.Close (); 
		_dbcmd.Dispose ();
		_dbcon.Close ();
	}


	public void createMatch ()
	{
		string query;


		int matchTime = Convert.ToInt32 (Length.text);
		Debug.Log (matchTime);
		query = @"delete from sqlite_sequence where name='Match';
			delete from sqlite_sequence where name='TeamStats';
			INSERT INTO Match (MatchLength,DateOfMatch,HighlightLink, MatchLink)  
			VALUES (" + matchTime + ",'" + Date.text + "','" + HighlightLink.text + "','" + MatchLink.text + "');" +
			"Select last_insert_rowid()";

		StartConnection (_filename, query);
		CloseConnection ();
	}

	public void enterTeamStats ()
	{
		string query;
		teamStats = true;

		if (Won.isOn)
			wonMatch = true;
		else
			wonMatch = false;

		if (!team1finished) {
			query = @"INSERT INTO TeamStats(MatchID, TeamID,Kills,Deaths,Assists,Gold,Dragons,Barons,Towers,Won,Ban1,Ban2,Ban3)" +
			" SELECT " + MatchHolder + ", (Select ID FROM Teams WHERE TeamName = '" + TeamName.text + "')," + Kills.text + "," + Deaths.text + ","
			+ Assists.text + "," + Gold.text + "," + Dragons.text + "," + Barons.text + "," + Towers.text + ",'" + wonMatch.ToString () + "',(SELECT ChampID\n" +
			"FROM Champions\n Where ChampionName = '" + Ban1.text + "'),(SELECT ChampID\nFROM Champions\n" +
			" Where ChampionName = '" + Ban2.text + "'),(SELECT ChampID\nFROM Champions\n Where ChampionName = '" + Ban3.text + "');";

			StartConnection (_filename, query);
			team1finished = true;

			teamNameHolder = TeamName.text;
			Debug.Log (teamNameHolder);
			TeamName.text = "";
			Kills.text = "";
			Deaths.text = "";
			Assists.text = "";
			Gold.text = "";
			Dragons.text = "";
			Barons.text = "";
			Towers.text = "";
			wonMatch = !wonMatch;
			Ban1.text = "";
			Ban2.text = "";
			Ban3.text = "";
			AddStats.gameObject.GetComponentInChildren<Text> ().text = "Add Blue Team";

		} else {
			Debug.Log (teamNameHolder);
			query = @"INSERT INTO PlayedBy(MatchID, RedTeamID,BlueTeamID)" + 
				" SELECT " + MatchHolder + ", (Select ID FROM Teams WHERE TeamName = '" + teamNameHolder + "'), " +
				"(Select ID FROM Teams WHERE TeamName = '" + TeamName.text+ "');\n" +
				"INSERT INTO TeamStats(MatchID, TeamID,Kills,Deaths,Assists,Gold,Dragons,Barons,Towers,Won,Ban1,Ban2,Ban3)" +
				" SELECT " + MatchHolder + ", (Select ID FROM Teams WHERE TeamName = '" + TeamName.text + "')," + Kills.text + "," + Deaths.text + ","
				+ Assists.text + "," + Gold.text + "," + Dragons.text + "," + Barons.text + "," + Towers.text + ",'" + wonMatch.ToString () + "',(SELECT ChampID\n" +
				"FROM Champions\n Where ChampionName = '" + Ban1.text + "'),(SELECT ChampID\nFROM Champions\n" +
				" Where ChampionName = '" + Ban2.text + "'),(SELECT ChampID\nFROM Champions\n Where ChampionName = '" + Ban3.text + "');";

			StartConnection (_filename, query);
			team1finished = false;
			CloseConnection ();
			PlayerSlide.SetTrigger ("Open");
		}

	}

	void Start ()
	{
		
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void enterPlayerStats(){
		teamStats = false;
		string query;
		query = @"INSERT INTO PlayerStats(PlayerID,MatchID,Kills,Deaths,Assists,CreepScore,ChampionID, Gold)   
		SELECT (Select PlayerID FROM Players WHERE SummonerName = '"+ SummonerName.text + "'),"+ MatchHolder+"," +
			PKills.text + ","+ PDeaths.text + ","+ PAssists.text + "," + Creeps.text +", (SELECT ChampID\nFROM Champions\n" +
			"Where ChampionName = '" + ChampionName.text +"'),"+ PGold.text + ";";
		StartConnection (_filename, query);
		CloseConnection ();
		playerCounter++;
		AddPlayer.gameObject.GetComponentInChildren<Text> ().text = "Add Player " + playerCounter;

		if (playerCounter > 9)
			CancelButton.gameObject.SetActive (true);
	}
}
