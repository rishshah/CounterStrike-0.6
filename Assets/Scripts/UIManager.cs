using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {
    bool isActive = true;
    public Text time;
	public Text bullets;
	public Text health;
    public GameObject CT;
	public GameObject T;
    public NetworkManager nm;
    bool assigned = false;
    bool prevhealthpower = false;
    Color green = new Color(67 / 255f, 163 / 255f, 17 / 255f, 255 / 255f);
    PlayerShooting ps;
	PlayerNetworkMover pnm;
    PowerUp pu;

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
	}
	void Displaytime()
	{   
		time.text = minutes.ToString() + " : " + seconds.ToString();
		//  Debug.Log();

	}
	void TimeConvert()
	{
		minutes = (int)timesec / 60;
		seconds = (int)timesec % 60;
	}
    void Init()
    {
        for (int i = 0; i < CT.transform.childCount; i++)
        {
			if (CT.transform.GetChild(i).gameObject.GetPhotonView().isMine && !CT.transform.GetChild(i).gameObject.GetComponent<PlayerNetworkMover>().isBot)
            {
                pnm = CT.transform.GetChild(i).gameObject.GetComponent<PlayerNetworkMover>();
                ps = CT.transform.GetChild(i).gameObject.GetComponentInChildren<PlayerShooting>();
                pu = CT.transform.GetChild(i).gameObject.GetComponentInChildren<PowerUp>();
                Debug.Log("Found " + i.ToString());
                bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
                health.text = pnm.health.ToString();
                assigned = true;
				return;
            }
        }

		for (int i = 0; i < T.transform.childCount; i++)
		{
			if (T.transform.GetChild(i).gameObject.GetPhotonView().isMine && !T.transform.GetChild(i).gameObject.GetComponent<PlayerNetworkMover>().isBot)
			{
				pnm = T.transform.GetChild(i).gameObject.GetComponent<PlayerNetworkMover>();
				ps = T.transform.GetChild(i).gameObject.GetComponentInChildren<PlayerShooting>();
                pu = T.transform.GetChild(i).gameObject.GetComponentInChildren<PowerUp>();
                Debug.Log("Found " + i.ToString());
				bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
				health.text = pnm.health.ToString();
				assigned = true;
				return;
			}
		}
    }
	// Update is called once per frame
	void Update () {
        if (!assigned)
        {
            if (nm.isJoinedRoom) Init();

        }
		timesec -= Time.deltaTime;
		TimeConvert();
		Displaytime();
        //Debug.Log(Time.deltaTime);
        if (assigned)
        {
            if (isActive != Input.GetKey(KeyCode.Tab))
            {
                for (int i = 0; i < this.gameObject.transform.childCount; i++)
                {
                    this.gameObject.transform.GetChild(i).gameObject.SetActive(isActive);
                }
                isActive = !isActive;
            }
			//this.gameObject.SetActive(!Input.GetKey(KeyCode.Tab));
            bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
            health.text = pnm.health.ToString();
            if (pu.healthpower)
            {
                health.color = Color.red;
                health.fontSize = 30 + ((int)pu.healthtimelimit - (int)pu.powertime) * 2;
            }
            if (prevhealthpower != pu.healthpower)
            {
                if (!pu.healthpower)
                {
                    health.color = green;
                    health.fontSize = 30;
                }
            }
            prevhealthpower = pu.healthpower;
        }
	}
}
