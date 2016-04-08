using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class MainMenuController : MonoBehaviour
{

	[SerializeField]
	GameObject[]
		Logos;
	[SerializeField]
	GameObject[]
		TeamNames;
	[SerializeField]
	GameObject[]
		Lines;

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
	// Use this for initialization
	void Start (){

		// Query to retrieve all the team names, ordered alphabetically 
		string _query = "SELECT T.TeamName " +
			"FROM Teams T " +
			"Order by T.TeamName COLLATE NOCASE;";

		// Start the connection and reader
		StartConnection (_filename, _query);

		// Retrieve the data from the Database
		for (int i = 0; _dbr.Read(); i++) {
			// As long as i is under 15, activate the line gameobjects (max number of lines is smaller than number of teams)
			if (i < 15)
				Lines [i].gameObject.SetActive (true);

			// First activate the amount of gameobjects needed for the number of teams
			Logos [i].gameObject.SetActive (true);
			TeamNames [i].gameObject.SetActive (true);

			// Assign the retrieved data to the TeamName Text components
			TeamNames [i].gameObject.GetComponent<Text> ().text = _dbr.GetString (0);
			
		}
		CloseConnection ();
	}

	// This Button click is leading the user to the Teams scene, setting the PlayerPrefs for team to the team that has been clicked on
	public void button_teamView (string teamName){
		PlayerPrefs.SetString ("TeamName", teamName);
		Application.LoadLevel ("Teams");
	}

	// Similar to the button_teamView click, query in TeamController needs the ID of the team, so it needs to be saved
	public void button_teamView2 (string teamID){
		PlayerPrefs.SetString ("TeamID", teamID);
	}
}
