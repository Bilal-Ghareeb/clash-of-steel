using UnityEngine.Playables;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Cinemachine;

public class TimelineController : MonoBehaviour
{
    public static TimelineController Instance { get; private set; }

    [SerializeField] private PlayableDirector m_director;
    [SerializeField] private CinemachineCamera m_cinematicCamer;

    private void Awake() => Instance = this;

    public async Task PlayTimelineAsync(Combatant source, PlayableAsset asset)
    {
        if (asset == null)
        {
            Debug.LogWarning($"No timeline found for {source?.DisplayName}");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();

        m_director.playableAsset = asset;

        foreach (var output in asset.outputs)
        {
            if (output.streamName == "CharacterRoot" && source.CombatantAnimator != null)
                m_director.SetGenericBinding(output.sourceObject, source.CombatantAnimator);
            else if (output.streamName == "CinematicCamera" && m_cinematicCamer != null)
                m_director.SetGenericBinding(output.sourceObject, m_cinematicCamer);
        }

        m_director.stopped += OnTimelineStopped;
        m_director.Play();

        void OnTimelineStopped(PlayableDirector d)
        {
            if (d == m_director)
            {
                m_director.stopped -= OnTimelineStopped;
                tcs.TrySetResult(true);
            }
        }

        await tcs.Task;
    }


    public Task PlayEntranceAsync(Combatant c) => PlayTimelineAsync(c, c.Timelines?.entranceTimeline);
    public Task PlayAttackAsync(Combatant c) => PlayTimelineAsync(c, c.Timelines?.attackTimeline);
    public Task PlayDeathAsync(Combatant c) => PlayTimelineAsync(c, c.Timelines?.deathTimeline);
}
