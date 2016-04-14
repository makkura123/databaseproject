using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;

public class championPoolControl : MonoBehaviour {
	[SerializeField]
	GameObject 
		_scrollView;

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
		
		// After checking if the file exists, create a new sqlite connection and open database
		_dbcon = new SqliteConnection ("URI=file:" + filepath);
		_dbcon.Open ();
		
		// Send query to DB
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
	void Start () {

		// Query returns all of the champion names
		string query = "select ChampionName " +
				"from Champions " +
				"order by 1";

		StartConnection (_filename, query);

		// Create gameobjects based on the created prefab (necessary since there are so many champions)
		for (int i = 0; _dbr.Read (); i++) {

	
			// Create the Object
			GameObject championHolder =  Instantiate(Resources.Load("ChampionPrefab"))as GameObject;
			championHolder.transform.SetParent(_scrollView.transform, false);

			// Grab the Text Component and assign the champion name to it
			championHolder.gameObject.GetComponentInChildren<Text>().text = _dbr.GetString(0);
		
			string championName = _dbr.GetString (0);
			// Add on click listener to each of the Buttons based on champion
			championHolder.gameObject.GetComponent<Button>().onClick.AddListener(() => see_champion(championName));

		
		}
		CloseConnection ();
	}

	// Function to see the champion stats
	public void see_champion(string champName){

		PlayerPrefs.SetString ("Champion", champName);
		Application.LoadLevel ("championPool");
	}
}
