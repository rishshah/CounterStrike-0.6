using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour
{
    public float powertime = 0f;
    bool powerup = false;
    bool multiplepowerup = false;
    // Health powerup variables
    public bool healthpower = false;
    public float healthtimelimit = 10f;
    public float currenthealth = 0f;
    float healthboost = 100f;
    public PlayerNetworkMover pnm;

    // Flash powerup variables
    bool speedpower = false;
    public float speedtimelimit = 10f;
    float speedboost = 2f;
    public FirstPersonController fps;

	//Invisibility variables
	public bool invpower = false;
	public float invtimelimit = 10f;
	public Camera camGun;
	int oldmask;

    // Use this for initialization
    void Start()
    {
        powertime = 0f;
		oldmask= camGun.cullingMask;
    }
    void Healthend()
    {
        pnm.health = Mathf.Min(currenthealth, pnm.health);
        powertime = 0f;
        healthpower = false;
        powerup = false;
    }
	void SetBodyInv(bool state){

		transform.Find ("Head").GetComponent<MeshRenderer> ().enabled = !state;
		for (int i = 0; i < 3; i++) {
			transform.Find ("Head").GetChild (i).GetComponent<MeshRenderer> ().enabled = !state;
		}
		transform.Find ("Body").GetComponent<MeshRenderer> ().enabled = !state;
	}


    // Update is called once per frame
    void Update()
    {
        // #########################
        //INitalise Health
		if (Input.GetKeyDown(KeyCode.Keypad1) && (!powerup || multiplepowerup))
        {
            //  Debug.Log(currenthealth);
            currenthealth = pnm.health;
            pnm.health += healthboost;
            healthpower = true;
            powerup = true;
        }
        //During Health power up
        if (healthpower)
        {
            if (powertime < healthtimelimit)
            {
                powertime += Time.deltaTime;
                if (currenthealth >= pnm.health)
                {
                    Healthend();
                }
            }
            else
            {
                Healthend();
            }

        }
        // ###########################
        if (Input.GetKeyDown(KeyCode.Keypad2) && (!powerup || multiplepowerup))
        {
            fps.m_WalkSpeed *= speedboost;
            fps.m_RunSpeed *= speedboost;
            speedpower = true;
            powerup = true;

        }

        if (speedpower)
        {
            if (powertime < speedtimelimit)
            {
                powertime += Time.deltaTime;
            }
            else
            {
                fps.m_WalkSpeed /= speedboost;
                fps.m_RunSpeed /= speedboost;
                powerup = false;
                speedpower = false;
                powertime = 0f;

            }
        }

		// ###########################
		if (Input.GetKeyDown (KeyCode.Keypad3) && (!powerup || multiplepowerup)) {
			pnm.SetGunLayer (12);
			SetBodyInv (true);
			camGun.cullingMask |= (1 << 12);
			invpower = true;
			powerup = true;
		}

		if (invpower)
		{
			if (powertime < invtimelimit)
			{
				powertime += Time.deltaTime;
			}
			else
			{	
				invpower = false;
				powerup = false;
				pnm.SetGunLayer (11);
				camGun.cullingMask  = oldmask;
				SetBodyInv (false);
				powertime = 0f;
			}
		}

    }
}
