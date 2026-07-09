
#if PHOTON_MULTIPLAYER
public class PhotonManager : MonoBehaviour
{
    public Camera lobbyCamera;
    public Text networkStatusText;
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public float spawnRadius = 20f;


    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings("NVP Multiplayer v1");
        PhotonNetwork.sendRate = 25;
    }


    public virtual void OnConnectedToMaster()
    {
        Debug.Log("Connected to master.");
    }


    public virtual void OnJoinedLobby()
    {
        Debug.Log("Jointed lobby.");
        lobbyCamera.gameObject.SetActive(false);

        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        PhotonNetwork.JoinOrCreateRoom("NVP Multiplayer Room", roomOptions, null);
    }


    public virtual void OnJoinedRoom()
    {
        var watchdog = 0;
        while (true)
        {
            var randVal = (Random.value - 0.5f) * 2f * spawnRadius;
            var randomSpawnPoint = new Vector3(spawnPoint.position.x + randVal, spawnPoint.position.y,
                spawnPoint.position.z + randVal);
            var hits = Physics.OverlapSphere(randomSpawnPoint, 3f);

            if (hits.Length == 0 || ++watchdog > 20) break;
        }

        PhotonNetwork.Instantiate(playerPrefab.name,
            spawnPoint.position + new Vector3(Random.value * 15f, 0, Random.value * 15f), spawnPoint.rotation, 0);
    }


    private void Update()
    {
        networkStatusText.text = PhotonNetwork.connectionStateDetailed.ToString();
    }
}
#endif