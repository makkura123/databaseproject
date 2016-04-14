using UnityEngine.UI;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;
using System.Linq;

public class ChampionControll : MonoBehaviour{
	// Declare the textfields for Name of champion, pickrate and banrate, and champ image
	[SerializeField]
	Text
		_championName;
	[SerializeField]
	Text
		_pickRate;
	[SerializeField]
	Text
		_banRate;

	[SerializeField]
	Image
		_champImg;

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

	void Start (){
		// get champ image game object

		// Retrieve active Champion from PlayerPrefs to ask DB about stats
		string activeChampion = PlayerPrefs.GetString ("Champion");
	
		activeChampion = activeChampion.Replace ("'", "''");
		Debug.Log (activeChampion);
		// Query is going to return the Name of the Champion, the pickrate and the banrate based on the stats in TeamStats
		string _query = "Select ('" + activeChampion + "') as ChampionName, " +
			"Round(CAST(P.Rate as FLOAT)*100/Tot.Num,1) AS PickRate, " +
				"Round(CAST(B.Rate as FLOAT)*100/Tot.Num,1) AS BanRate " +
				"From " +
				"(select count(*) as Rate " +
				"from PlayerStats P, Champions C " +
				"where P.ChampionID = C.ChampID " +
				"and C.ChampionName = '" + activeChampion + "') as P, " +
				"(select count(*) as Rate from( " +
				"select * " +
				"from TeamStats T, Champions C " +
				"where T.Ban1 = C.ChampID " +
				"and C.ChampionName = '" + activeChampion + "' " +
				" UNION " +
				"select * " +
				"from TeamStats T, Champions C " +
				"where T.Ban2 = C.ChampID " +
				"and C.ChampionName = '" + activeChampion + "' " +
				" UNION " +
				"select * " +
				"from TeamStats T, Champions C " +
				"where T.Ban3 = C.ChampID " +
				"and C.ChampionName = '" + activeChampion + "' " +
				")) as B, " +
				"(select count(*) as Num from Match) as Tot ";
		// Start DB connection
		StartConnection (_filename, _query);


		// Read answers from the reader (everything returned in string-form by DB)
		for (int i = 0; _dbr.Read(); i++) {
			_championName.text = _dbr.GetString (0);
			_pickRate.text = _dbr.GetString (1) + "%";
			_banRate.text = _dbr.GetString (2) + "%";

		}

		// Close the Database
		CloseConnection ();

		StartCoroutine(GetChampImageRoutine(activeChampion));
	}

	IEnumerator GetChampImageRoutine(string champName)
	{
		champName = champName.Replace ("'", "");
		champName = champName.Replace (".", "");
		champName = champName.Replace (" ", "");

		// For some reason Riot API has an issue with chogath....
		if (champName == "ChoGath")
			champName = new String(champName.Select((ch, index) => (index == 0) ? ch : Char.ToLower(ch)).ToArray());

		Debug.Log (champName);
		var champImageRequest = new WWW ("http://ddragon.leagueoflegends.com/cdn/6.7.1/img/champion/" + champName + ".png");

		while (!champImageRequest.isDone)
		{
			yield return 0;
		}

		if (champImageRequest.error == null)
		{
			var champSprite = Sprite.Create (champImageRequest.texture, new Rect (0, 0, 120, 120), new Vector2 (60, 60));
			_champImg.sprite = champSprite;
		}
	}
}
