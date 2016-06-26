using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {

	public Text time;
	public Text bullets;
	public Text health;
	public PlayerShooting ps;
	public PlayerNetworkMover pnm;
	int minutes;
	int seconds;
	float timesec;
	float Roundtime = 600f;
	// Use this for initialization
	void Start()
	{
		timesec = Roundtime;
		TimeConvert();
		Displaytime();
		bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
		health.text = pnm.health.ToString();    

	}
	void Displaytime()
	{   
		time.text = minutes.ToString() + " : " + seconds.ToString();
		///  Debug.Log();

	}
	void TimeConvert()
	{
		minutes = (int)timesec / 60;
		seconds = (int)timesec % 60;
	}

	// Update is called once per frame
	void Update () {
		timesec -= Time.deltaTime;
		TimeConvert();
		Displaytime();
		//Debug.Log(Time.deltaTime);
		bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
		health.text = pnm.health.ToString();
	}
}
