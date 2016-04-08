
using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class MatchStatsControll : MonoBehaviour
{
	// Keep track of which team was selected (aka lineNumber)
	string lineNumber;
	string Team1;
	string Team2;

	/* A bunch of gameobjects are needed for displaying the data
	 * Array of TextFields for the Names of both teams
	 * single TextFields for Dates, Time of the match in minutes, Total Kills/Deaths/Assist, Gold, Barons, Bans, Towers AND dragons
	 * Since there are different stats, some string arrays are needed to store data to allow user to switch between data
	 * */
	[SerializeField]
	Text[]
		TeamNames;
	[SerializeField]
	Text
		Date;
	[SerializeField]
	Text
		_matchLength;
	[SerializeField]
	Text
		_KDA;
	[SerializeField]
	Text
		_Gold;
	[SerializeField]
	Text
		_Barons;
	[SerializeField]
	Text
		_Bans;
	[SerializeField]
	Text
		_Towers;
	[SerializeField]
	Text
		_Dragons;

	string[] Towers = new string[2];
	string[] Barons = new string[2];
	string[] Bans = new string[2];
	string[] Dragons = new string[2];
	string[] Gold = new string[2];
	string[] KDA = new string[2];
	// Link to the match AND to some highlights, which will open in Youtube App (or Browser if no youtube app installed)
	string matchLink;
	string highlightLink;

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
	
	void Start ()
	{
		// Need the team that was used to access this match (saved in PlayerPrefs)
		Team1 = PlayerPrefs.GetString ("TeamName");

		// Need the line of the team for OFFSET to determine the row
		lineNumber = PlayerPrefs.GetString ("LineNumber");

		// The first team is just going to be the one saved in PlayerPrefs
		TeamNames [0].text = Team1;

		// Query takes the TeamName and the line number to determine which match it has to return, then it returns all of the stats saved in the TeamStats table
		string query = "SELECT M.DateOfMatch, (S.TeamName) AS Opponent, PI.Won, Round(M.MatchLength/60, 2) " +
			"AS MatchLength, M.MatchLink, M.HighlightLink, (PI.Kills) AS ThisTeamKills, " +
			"(PI.Deaths) AS ThisTeamDeaths , (PI.Assists) AS ThisTeamAssists, (PI.Gold) AS ThisTeamGold, " +
			"(PI.Dragons) as ThisTeamDragons, (PI.Barons) as ThisTeamBarons, " +
			"(PI.Towers) as ThisTeamTowers, (PI.Ban1) as ThisTeamBan1, " +
			"(PI.Ban2) as ThisTeamBan2, (PI.Ban3) as ThisTeamBan3, " +
			"(T.Kills) as ThatTeamKills, (T.Deaths) as ThatTeamDeaths , " +
			"(T.Assists) as ThatTeamAssists, (T.Gold) as ThatTeamGold, " +
			"(T.Dragons) as ThatTeamDragons, (T.Barons) as ThatTeamBarons, (T.Towers) as ThatTeamTowers, " +
			"(C1.ChampionName) as ThatTeamBan1, (C2.ChampionName) as ThatTeamBan2, " +
			"(C3.ChampionName) as ThatTeamBan3 " +
			"FROM Teams S, Match M, TeamStats T, Champions C1, Champions C2, Champions C3, (" +
			"SELECT T.MatchID, Teams.ID, T.Won, T.Kills, T.Deaths, T.Assists, T.Gold, T.Dragons, " +
			"T.Barons, T.Towers,(C1.ChampionName) as Ban1, (C2.ChampionName) as Ban2, " +
			"(C3.ChampionName) as Ban3 " +
			"FROM TeamStats T, Teams, Champions C1, Champions C2, Champions C3 " +
			"WHERE Teams.Abbreviation = '" + Team1 + "' AND T.Ban1 = C1.ChampID AND T.Ban2 = C2.ChampID " +
			"AND T.Ban3 = C3.ChampID AND Teams.ID = T.TeamID) as PI " +
			"WHERE M.MatchID = PI.MatchID AND T.MatchID = PI.MatchID AND not T.TeamID = PI.ID " +
			"AND T.TeamID = S.ID AND T.Ban1 = C1.ChampID AND T.Ban2 = C2.ChampID AND T.Ban3 = C3.ChampID " +
			"ORDER BY 1 " +
			"LIMIT 1 " +
			"OFFSET " + lineNumber + ";";

		StartConnection (_filename, query);

		/* Retrieve all the data from the DB
		 * Date and the name of the second team is going to be assigned to the textfields
		 * Outcome is used to check which team won (Winner will get the blue color assigned to the textfield)
		 * For anything that is different for both teams (KDA, Gold etc) the data is going to be put in the arrays so that the user can switch between the options
		 * */
		for (int i = 0; _dbr.Read(); i++) {
			Date.text = _dbr.GetString (0);
			Team2 = _dbr.GetString (1);
			TeamNames [1].text = Team2;

			string outcome = _dbr.GetString (2);
			if (outcome == "TRUE") {
				TeamNames [0].color = Color.blue;
				TeamNames [1].color = Color.red;
			} else {
				TeamNames [1].color = Color.blue;
				TeamNames [0].color = Color.red;
			}

			_matchLength.text = _dbr.GetString (3) + "min";
			matchLink = _dbr.GetString (4);
			highlightLink = _dbr.GetString (5);
			KDA [0] = _dbr.GetString (6) + " / " + _dbr.GetString (7) + " / " + _dbr.GetString (8);
			Gold [0] = _dbr.GetString (9)+ "k";
			Dragons [0] = _dbr.GetString (10);
			Barons [0] = _dbr.GetString (11);
			Towers [0] = _dbr.GetString (12);
			Bans [0] = _dbr.GetString (13) + ", " + _dbr.GetString (14) + ", " + _dbr.GetString (15);

			KDA [1] = _dbr.GetString (16) + " / " + _dbr.GetString (17) + " / " + _dbr.GetString (18);
			Gold [1] = _dbr.GetString (19) + "k";
			Dragons [1] = _dbr.GetString (20);
			Barons [1] = _dbr.GetString (21);
			Towers [1] = _dbr.GetString (22);
			Bans [1] = _dbr.GetString (23) + ", " + _dbr.GetString (24) + ", " + _dbr.GetString (25);

		
		}
		// This is going to be the default stats (Team 1 stats)
		_Dragons.text = Dragons [0];
		_Barons.text = Barons [0];
		_Towers.text = Towers [0];
		_KDA.text = KDA [0];
		_Bans.text = Bans [0];
		_Gold.text = Gold [0];

		CloseConnection ();

	}

	// Button to switch between the Stats of the first team and the second team
	public void selectTeam (int teamNumber){
		_Dragons.text = Dragons [teamNumber];
		_Barons.text = Barons [teamNumber];
		_Towers.text = Towers [teamNumber];
		_KDA.text = KDA [teamNumber];
		_Bans.text = Bans [teamNumber];
		_Gold.text = Gold [teamNumber];
	}

	// Function to open the link (link retrieved from DB)
	public void openLink (int linktype){
		if (linktype == 1)
			Application.OpenURL (matchLink);
		else 
			Application.OpenURL (highlightLink);
	}
}
