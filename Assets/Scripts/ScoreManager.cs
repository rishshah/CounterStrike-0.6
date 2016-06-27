using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class ScoreManager : MonoBehaviour {

	public Dictionary<string, Dictionary<string, int>>   playerScores ;
	public int counter=0;
    public delegate void Score(string username, string scoreType, int value);
    public event Score SetScoreRPC;
    public void Init(){
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
	public void ChangeScore(string username , string scoreType , int amount){
		
		int currScore = GetScore (username, scoreType);
		Debug.Log (currScore);
        if(SetScoreRPC!=null)
            SetScoreRPC (username, scoreType, currScore + amount);

	}
	public string[] GetPlayerNames(string sortType){
		return playerScores.Keys.OrderByDescending (n => GetScore (n, sortType)).ToArray();
	}
}
