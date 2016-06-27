using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class TPlayerscore : MonoBehaviour {

	public Dictionary<string, Dictionary<string, int>>   Tplayers ;
	public void Init(){
		if (Tplayers != null)
			return;
		Tplayers = new Dictionary<string, Dictionary<string, int>>();
	}

	public int GetScore(string username , string scoreType){
		Init ();
		if (Tplayers.ContainsKey (username) == false) {
			return 0;
		}
		if (Tplayers [username].ContainsKey (scoreType) == false) {
			return 0;
		}
		return Tplayers[username][scoreType];
	}
	public string[] GetPlayerNames(string sortType){
		return Tplayers.Keys.OrderByDescending (n => GetScore (n, sortType)).ToArray();
	}
}
