
using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class TeamController : MonoBehaviour{

	/* A whole bunch of Textfields, Gameobjects and Images are needed
	 * HeaderImage and HeaderImage 2 represent the Team Logo
	 * The image Collection array is used to store all  of the possible logos
	 * Textfield Arrays for the Summoner and Player Names and Roles for the roster
	 * Containers are used for all gameobjects needed for the Roster, matchContainer fulfill same functionality for the match history
	 * match Animator is an object used to animate a slide over for the match history
	 * */
	[SerializeField]
	Image
		HeaderImage;
	[SerializeField]
	Image
		HeaderImage2;
	[SerializeField]
	Sprite[]
		imageCollection;
	[SerializeField]
	Text[]
		SummonerNames;
	[SerializeField]
	Text[]
		PlayerNames;
	[SerializeField]
	Text[]
		Roles;
	[SerializeField]
	GameObject[]
		Containers;
	[SerializeField]
	Animator
		matchAnimator;
	[SerializeField]
	GameObject[]
		matchContainers;
	[SerializeField]
	Text[]
		Dates;
	[SerializeField]
	Text[]
		Opponent;
	[SerializeField]
	Text[]
		Outcome;

	// Initialize activeTeam Name and constant strings for loss and win
	string activeTeam = "";
	const string loss = "Loss";
	const string win = "Win";

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

		// Retrieve the active Team from the PlayerPrefs (saved in previous scene)
		activeTeam = PlayerPrefs.GetString ("TeamName");

		// Activate the logo, based on the TeamID, set the right sprite for both HeaderImages
		int activeLogo = Convert.ToInt32 (PlayerPrefs.GetString ("TeamID"));
		HeaderImage.sprite = imageCollection [activeLogo - 1];
		HeaderImage2.sprite = imageCollection [activeLogo - 1];

		// Simple query to return the Players of a Team based on the TeamName
		string query = "SELECT P.SummonerName, P.FirstName, P.LastName, P.Role " +
			"FROM Players P, Teams T " +
			"WHERE T.Abbreviation = '" + activeTeam + "' AND T.ID = P.TeamID;";
		
		StartConnection (_filename, query);

		// Retrieve the data from DB, activate the needed number of containers (teams have different amount of members)
		for (int i = 0; _dbr.Read(); i++) {
			Containers [i].gameObject.SetActive (true);
			SummonerNames [i].text = _dbr.GetString (0);
			PlayerNames [i].text = _dbr.GetString (1) + " " + _dbr.GetString (2);
			Roles [i].text = _dbr.GetString (3);

		}
		CloseConnection ();
	}

	// This is going to open up the screen for the match history
	public void button_Click (){

		// Query to return the information about all of the matches played by the currently active team
		string query = "SELECT M.DateOfMatch, S.TeamName, PI.won " +
			"FROM Teams S, Match M, TeamStats T, (" +
			"SELECT T.MatchID, Teams.ID, T.won " +
			"FROM TeamStats T, Teams " +
			"WHERE Teams.Abbreviation = '" + activeTeam + 
			"' and Teams.ID = T.TeamID) as PI " +
			"WHERE M.MatchID = PI.MatchID AND T.MatchID = PI.MatchID " +
			"AND NOT T.TeamID = PI.ID AND T.TeamID = S.ID " +
			"Order By M.DateOfMatch ";

		StartConnection (_filename, query);

		// Retrieve the data from DB
		for (int j = 0; _dbr.Read(); j++) {
			// Similarly to before - activate containers based on amount of data
			matchContainers [j].gameObject.SetActive (true);

			Dates [j].text = _dbr.GetString (0);
			Opponent [j].text = _dbr.GetString (1);

			// Determine which team has won the match
			string outcome = _dbr.GetString (2);

			if (outcome == "TRUE" || outcome == "True") {
				Outcome [j].color = Color.blue;
				Outcome [j].text = win;
			} else {
				Outcome [j].color = Color.red;
				Outcome [j].text = loss;
			}
		}

		// AFTER retrieving the data, show the slide
		matchAnimator.SetTrigger ("MatchHistory");
		CloseConnection ();
		
	}

	// Go back to the Roster Slide
	public void button_CancelClick (){
		matchAnimator.SetTrigger ("Dismiss");
	}

	// Button function to see a specific match (pass in number of the match to determine the row)
	public void seeMatch (string linenumber){
		PlayerPrefs.SetString ("LineNumber", linenumber);
		Debug.Log (linenumber);
		Application.LoadLevel ("matchSchedule");
	}

	// Button function to see more information about a specific player (use the number in the order to save which player was clicked on)
	public void seePlayer (int playerLine){
		PlayerPrefs.SetString ("Summoner", SummonerNames [playerLine].text);
		Application.LoadLevel ("Players");
	}
}
