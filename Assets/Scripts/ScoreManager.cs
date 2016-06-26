using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class ScoreManager : MonoBehaviour {

	Dictionary<string, Dictionary<string, int>>   playerScores ;
	public int counter=0;
	void Init(){
		if (playerScores != null)
			return;
		playerScores = new Dictionary<string, Dictionary<string, int>>();

	}

	public int GetScore(string username , string scoreType){
		Init ();
		if (playerScores.ContainsKey (username) == false) {
			return 0;
		}
		if (playerScores [username].ContainsKey (scoreType) == false) {
			return 0;
		}
		return playerScores[username][scoreType];
	}
	public void SetScore(string username , string scoreType , int value){
		Init ();
		counter++;
		if (playerScores.ContainsKey (username) == false) {
			playerScores[username]=new Dictionary<string, int>();
		}
		playerScores[username][scoreType]= value;
		Debug.Log (counter);
	}
	public void ChangeScore(string username , string scoreType , int amount){
		
		int currScore = GetScore (username, scoreType);
		Debug.Log (currScore);
		SetScore (username, scoreType, currScore + amount);

	}
	public string[] GetPlayerNames(string sortType){
		return playerScores.Keys.OrderByDescending (n => GetScore (n, sortType)).ToArray();
	}
}
