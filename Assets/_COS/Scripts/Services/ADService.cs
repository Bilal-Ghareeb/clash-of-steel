using UnityEngine;
using Unity.Services.LevelPlay;

public class ADService 
{
    private bool m_isAdsEnabled = false;
    private LevelPlayRewardedAd m_rewardedVideoAd;

    public void Init()
    {
        LevelPlay.ValidateIntegration();

        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        LevelPlay.Init(AdConfig.AppKey);
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        Debug.Log($"[LevelPlaySample] Received SdkInitializationCompletedEvent with Config: {config}");
        EnableReardedAds();
        m_isAdsEnabled = true;
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.Log($"[LevelPlaySample] Received SdkInitializationFailedEvent with Error: {error}");
    }

    private void EnableReardedAds()
    {
        m_rewardedVideoAd = new LevelPlayRewardedAd(AdConfig.RewardedVideoAdUnitId);
        Debug.Log("[LevelPlaySample] LoadRewardedVideoButtonClicked");
        m_rewardedVideoAd.LoadAd();
        m_rewardedVideoAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
        m_rewardedVideoAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
        m_rewardedVideoAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
        m_rewardedVideoAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
        m_rewardedVideoAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
        m_rewardedVideoAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
        m_rewardedVideoAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
        m_rewardedVideoAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;
    }

    public void ShowRewardedAd()
    {
        Debug.Log("[LevelPlaySample] ShowRewardedVideoButtonClicked");
        if (m_isAdsEnabled && m_rewardedVideoAd.IsAdReady())
        {
            Debug.Log("[LevelPlaySample] Showing Rewarded Video Ad");
            m_rewardedVideoAd.ShowAd();
            m_rewardedVideoAd.LoadAd();
        }
        else
        {
            Debug.Log("[LevelPlaySample] LevelPlay Rewarded Video Ad is not ready");
        }
    }

    private void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
    }

    private void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
    }

    private void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    private void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    private void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
    }

    private void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
    }

    private void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
    }

    private void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
    }
}
