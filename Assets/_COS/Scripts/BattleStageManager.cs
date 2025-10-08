using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleStageManager : MonoBehaviour
{
    public static BattleStageManager Instance { get; private set; }

    private List<StageData> m_stages;
    private StageData m_currentStage;

    public StageData CurrentStage => m_currentStage;

    public event Action<StageData> OnStageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayFabManager.Instance.OnLoginAndDataReady += Initialize;
    }

    public async void Initialize()
    {
        m_stages = PlayFabManager.Instance.BattleStages.ToList();

        await PlayFabManager.Instance.FetchPlayerStageProgressAsync();

        int currentId = PlayFabManager.Instance.CurrentStageId;
        m_currentStage = m_stages.FirstOrDefault(s => s.id == currentId) ?? m_stages.FirstOrDefault();

        Debug.Log($"StageManager initialized with current stage: {m_currentStage?.name}");
        OnStageChanged?.Invoke(m_currentStage);
    }
}
