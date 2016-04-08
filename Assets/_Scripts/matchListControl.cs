using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class matchListControl : MonoBehaviour{
	[SerializeField]
	GameObject[]
		matchContainers;
	[SerializeField]
	Text[]
		Team1Name;
	[SerializeField]
	Text[]
		Team2Names;
	[SerializeField]
	Text[]
		Date;

	const string _filename = "LeagueDB.s3db";
	private IDbConnection _dbcon;
	private IDbCommand _dbcmd;
	private IDataReader _dbr;
	
	// Function for starting the database connection
	public void StartConnection (string filename, string query){
		
		/* Need to check if the File already exists on the device
		 * If it doesn't, it needs to be created from the database file saved in StreamingAssets */
		string filepath = Application.persistentDataPath + "/" + filename;
		Debug.Log(filepath);
		if (!File.Exists (filepath)) {
			WWW loadDB = new WWW ("jar:file://" + Application.dataPath + "!/assets/" + filename);
			
			while (!loadDB.isDone) {
			}
			
			File.WriteAllBytes (filepath, loadDB.bytes);
		}
		
		// After checking if the file exists, create a new sqlite connection and open database
		_dbcon = new SqliteConnection ("URI=file:" + filepath);
		_dbcon.Open ();
		
		// Start Reader and pass query to reader
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

		// Query to return the Date of match, abbreviation of Blue and Red Team and which team won the match
		string query = "SELECT M.DateOfMatch, UPPER(S1.Abbreviation) AS BlueTeam, T1.won AS BlueTeamWon, " +
			"UPPER(S2.Abbreviation) AS RedTeam, T2.won AS RedTeamWon " +
			"FROM Match M, TeamStats T1, TeamStats T2, PlayedBY P, Teams S1, Teams S2 " +
			"WHERE M.MatchID = T1.MatchID " +
			"AND M.MatchID = T2.MatchID " +
			"AND M.MatchID = P.MatchID " +
			"AND T1.TeamID = P.BlueTeamID " +
			"AND T2.TeamID = P.RedTeamID " +
			"AND T1.TeamID = S1.ID " +
			"AND T2.TeamID = S2.ID " +
			"ORDER BY M.DateOfMatch " +
			"LIMIT 15;";

		StartConnection (_filename, query);
		
		for (int i = 0; _dbr.Read(); i++) {
			// Activate the necessary gameobjects
			matchContainers [i].gameObject.SetActive (true);

			// Retrieve Date of the match and the Team that was on the blue side
			Date [i].text = _dbr.GetString (0);
			Team1Name [i].text = _dbr.GetString (1);

			// Retrieve the outcome in form of a string
			string outcome1 = _dbr.GetString (2);

			// if the blue team won, the color of the textfield is set to blue, otherwise red
			if (outcome1 == "TRUE")
				Team1Name [i].color = Color.blue;
			else
				Team1Name [i].color = Color.red;

			// Repeat the same thing with the team on the red side
			Team2Names [i].text = _dbr.GetString (3);
			string outcome2 = _dbr.GetString (4);
			
			if (outcome2 == "TRUE")
				Team2Names [i].color = Color.blue;
			else
				Team2Names [i].color = Color.red;

		}
		CloseConnection ();
	}
}
