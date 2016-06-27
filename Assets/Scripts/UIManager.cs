using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {

    public Text time;
	public Text bullets;
	public Text health;
    public GameObject CT;
    public NetworkManager nm;
    bool assigned = false;
    PlayerShooting ps;
	PlayerNetworkMover pnm;

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
            if (CT.transform.GetChild(i).gameObject.GetPhotonView().isMine)
            {
                pnm = CT.transform.GetChild(i).gameObject.GetComponent<PlayerNetworkMover>();
                ps = CT.transform.GetChild(i).gameObject.GetComponentInChildren<PlayerShooting>();
                Debug.Log("Found " + i.ToString());
                bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
                health.text = pnm.health.ToString();
                assigned = true;
            }
        }

    }
	// Update is called once per frame
	void Update () {
        if (!assigned)
        {
            if (nm.isJoinedRoom) Init();

        }
        this.gameObject.SetActive(!Input.GetKey(KeyCode.Tab));
		timesec -= Time.deltaTime;
		TimeConvert();
		Displaytime();
        //Debug.Log(Time.deltaTime);
        if (assigned)
        {
            bullets.text = ps.bulletsInMagzin.ToString() + "/" + ps.bulletsOutMagzin.ToString();
            health.text = pnm.health.ToString();
        }
	}
}
