using System;
using UnityEngine;
using GoogleMobileAds.Api;

using RGSK;
using NWH.VehiclePhysics;

public class AdMobRewardedAds : MonoBehaviour
{
	private RewardBasedVideoAd rewardedAd;
	private VehicleController playerControl;

	private string adUnitId;

	// Start is called before the first frame update
	void Start()
    {
		if (RaceManager.instance != null)
		{
			if (RaceManager.instance.currentPlayer != null)
				playerControl = RaceManager.instance.currentPlayer.GetComponent<VehicleController>();
		}

		// 정식 버전
#if UNITY_ANDROID
		string appId = "ca-app-pub-9595832947554889~8247497822";
#elif UNITY_IPHONE
		            string appId = "ca-app-pub-3940256099942544~1458002511";
#else
		            string appId = "unexpected_platform";
#endif

		// 테스트 버전
		//#if UNITY_ANDROID
		//		string appId = "ca-app-pub-3940256099942544~3347511713";
		//#elif UNITY_IPHONE
		//            string appId = "ca-app-pub-3940256099942544~1458002511";
		//#else
		//            string appId = "unexpected_platform";
		//#endif

		MobileAds.Initialize(appId);

		this.rewardedAd = RewardBasedVideoAd.Instance;

		rewardedAd.OnAdRewarded += RewardedAd_OnUserEarnedReward;
		rewardedAd.OnAdClosed += RewardedAd_OnAdClosed;

		RequestRewardBasedVideo();
	}

	private void RequestRewardBasedVideo()
	{
		// 정식 버전
#if UNITY_ANDROID
		adUnitId = "ca-app-pub-9595832947554889/2200964229";
#elif UNITY_IPHONE
            adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            adUnitId = "unexpected_platform";
#endif
		AdRequest request = new AdRequest.Builder().Build();

		//		// 테스트 버전
		//#if UNITY_ANDROID
		//		adUnitId = "ca-app-pub-3940256099942544/5224354917";
		//#elif UNITY_IPHONE
		//            adUnitId = "ca-app-pub-3940256099942544/1712485313";
		//#else
		//            adUnitId = "unexpected_platform";
		//#endif
		//		// 테스트 기기 등록
		//		request = new AdRequest.Builder().AddTestDevice
		//		(
		//			"33BE2250B43518CCDA7DE426D04EE231"
		//		).Build();

		this.rewardedAd.LoadAd(request, adUnitId);
	}

	private void RewardedAd_OnUserEarnedReward(object sender, Reward e)
	{
#if UNITY_EDITOR
		Debug.Log("Ready to Give Reward");
#endif

		if (RaceManager.instance != null)
		{
			if (RaceManager.instance._raceState == RaceManager.RaceState.KnockedOut)
			{
				if (playerControl == null && RaceManager.instance.currentPlayer != null)
					playerControl = RaceManager.instance.currentPlayer.GetComponent<VehicleController>();

				if (RaceManager.instance._raceType == RaceManager.RaceType.FuelLimit)
					playerControl.fuel.amount += playerControl.fuel.capacity * 0.15f;
				else if (RaceManager.instance._raceType == RaceManager.RaceType.Checkpoints)
					RaceManager.instance.currentPlayer.GetComponent<Statistics>().lapTimeCounter += 15;

				if (RaceUI.instance != null)
				{
					RaceUI.instance.ContinueRace();
					RaceUI.instance.continueAfterwatchAdCount--;
				}

				RaceManager.instance.currentPlayer.GetComponent<Statistics>().knockedOut = false;
				playerControl.engine.Toggle();
			}
			else if (RaceManager.instance._raceState == RaceManager.RaceState.Complete)
			{
				if (RaceUI.instance !=null)
				{
					RaceUI.instance.newRewardAfterWatchAdCount--;
					RaceUI.instance.arrow.GetComponent<HighlightText>().Hightlight = false;
				}
			}
		}

#if UNITY_EDITOR
		RewardedAd_OnAdClosed(new object(), new EventArgs());
#endif
	}
	
	private void RewardedAd_OnAdClosed(object sender, EventArgs e)
	{
#if UNITY_EDITOR
		Debug.Log("Ads is Closed.");
#endif

		RequestRewardBasedVideo();
	}

	public void RewardAdsShow()
	{
#if UNITY_EDITOR
		RewardedAd_OnUserEarnedReward(new object(), new Reward());
#else
		if (rewardedAd.IsLoaded() == true)
			rewardedAd.Show();
#endif

#if UNITY_EDITOR
		if (rewardedAd.IsLoaded() == false)
			Debug.Log("Not yet Loaded");
#endif
	}
}
