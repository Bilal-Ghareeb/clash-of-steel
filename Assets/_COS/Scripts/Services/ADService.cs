using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class ADService 
{
    private bool m_isAdsEnabled = false;
    private LevelPlayRewardedAd m_rewardedVideoAd;
    private Action m_onAdFinished; 


    public void Init()
    {
        LevelPlay.ValidateIntegration();

        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        LevelPlay.Init(AdConfig.AppKey);
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
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

        m_rewardedVideoAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
        m_rewardedVideoAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
        m_rewardedVideoAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
        m_rewardedVideoAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
        m_rewardedVideoAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
        m_rewardedVideoAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
        m_rewardedVideoAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
        m_rewardedVideoAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;

        m_rewardedVideoAd.LoadAd();

    }

    public void ShowRewardedAd(Action onAdFinished = null)
    {
        m_rewardedVideoAd.LoadAd();
        if (m_isAdsEnabled && m_rewardedVideoAd.IsAdReady())
        {
            m_onAdFinished = onAdFinished;
            m_rewardedVideoAd.ShowAd();
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
        m_onAdFinished?.Invoke();
        m_onAdFinished = null;
        m_rewardedVideoAd.LoadAd();
    }

    private void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
    }

    private void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[ADService] Ad closed.");

        if (m_onAdFinished != null)
        {
            Debug.Log("[ADService] Ad closed without reward.");
            m_onAdFinished = null;
        }

        m_rewardedVideoAd.LoadAd();
    }

    private void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
    }
}
