using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour {
    float powertime=0f;
    bool powerup = false;
    bool multiplepowerup = false;
    // Health powerup variables
    bool healthpower = false;
    float healthtimelimit = 10;
    float currenthealth = 0f;
    float healthboost = 100f;
    public PlayerNetworkMover pnm;
    


	// Use this for initialization
	void Start () {
        powertime = 0f;
	
	}
  
	
	// Update is called once per frame
	void Update () {
        //INitalise Health
        if(Input.GetKeyDown(KeyCode.Q) && (!powerup||multiplepowerup))
        {
            Debug.Log(currenthealth);
            currenthealth = pnm.health;
            pnm.health += healthboost;
            healthpower = true;
        }
        //During Health power up
        if(healthpower)
        {
            if (powertime < healthtimelimit)
            {
                powertime += Time.deltaTime;
            }
            else
            {
                pnm.health = Mathf.Min(currenthealth, pnm.health);
                powertime = 0f;
                healthpower = false;
                powerup = false;
                
            }
        }
	
	}
}
