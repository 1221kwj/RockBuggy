using System;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdMobBannerView : MonoBehaviour
{
	private BannerView bannerView;
	//private AdRequest request;

	void Start()
    {
		//정식 버전
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

		// Initialize the Google Mobile Ads SDK.
		MobileAds.Initialize(appId);

		this.RequestBanner();
	}

	private void RequestBanner()
	{
		// 정식 버전
#if UNITY_ANDROID
		string adUnitId = "ca-app-pub-9595832947554889/2238770473";
#elif UNITY_IPHONE
		            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
		            string adUnitId = "unexpected_platform";
#endif

		AdRequest request = new AdRequest.Builder().Build();

		// 테스트 버전
		//#if UNITY_ANDROID
		//		string adUnitId = "ca-app-pub-3940256099942544/6300978111";
		//#elif UNITY_IPHONE
		//            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
		//#else
		//            string adUnitId = "unexpected_platform";
		//#endif

		//		// 테스트 기기 등록
		//		AdRequest request = new AdRequest.Builder().AddTestDevice
		//		(
		//			"33BE2250B43518CCDA7DE426D04EE231"
		//		).Build();

		/* Ad Mob Banner View Event Trigger Instruction
		 * @ Event Default Appearance
		 * - void "MethodName" (object sender, EventArgs args)
		 * @ Kind of Event
		 * 1. OnAdLoaded				: Called when an ad request has successfully loaded.
		 * 2. OnAdFailedToLoad			: Called when an ad request failed to load.
		 * 3. OnAdOpening				: Called when an ad is clicked.
		 * 4. OnAdClosed				: Called when the user returned from the app after an ad click.
		 * 5. OnAdLeavingApplication	: Called when the ad click caused the user to leave the application.
		 */

		// Create a 320x50 banner at the top of the screen.
		bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

		// Load the banner with request.
		bannerView.LoadAd(request);
	}

	public void DestroyAdBanner()
	{
		bannerView.Destroy();
	}

	public void HiddenAdBanner()
	{
		bannerView.Hide();
	}

	public void ShowAdBanner()
	{
		bannerView.Show();

		CustomMenuManager.Instance.TutorialPass();
	}
}
