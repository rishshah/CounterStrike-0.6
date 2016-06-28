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

    public string[] GetPlayerNamesCT(string sortType){
		List<string> ct = new List<string>();	
		foreach (string key in playerScores.Keys) {
            if (playerScores[key].ContainsKey("Team")) {
                if (playerScores[key]["Team"] == 1)
                {
                    ct.Add(key);
                }
			}
            else
            {
                Debug.Log("COUNTER TERRO ::Key : " + key + "has not team assigned WHY??");
            }
        }
		return ct.OrderByDescending (n => GetScore (n, sortType)).ToArray();
	}

    public string[] GetPlayerNamesT(string sortType){
		List<string> t = new List<string> ();
		foreach (string key in playerScores.Keys) {
            if (playerScores[key].ContainsKey("Team"))
            {
                if (playerScores[key]["Team"] == 0)
                {
                    t.Add(key);
                }
            }
            else
            {
                Debug.Log("TERRO :: Key : " + key + "has not team assigned WHY??");
            }
        }
		return t.OrderByDescending (n => GetScore (n, sortType)).ToArray();
	}
}
