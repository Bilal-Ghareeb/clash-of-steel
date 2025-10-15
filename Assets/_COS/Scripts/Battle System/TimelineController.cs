using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineController : MonoBehaviour
{
    public static TimelineController Instance { get; private set; }

    [SerializeField] private PlayableDirector m_director;
    [SerializeField] private CinemachineBrain m_cameraBrain;
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
            else if (output.streamName == "CinematicCamera" && m_cameraBrain != null)
            {
                m_director.SetGenericBinding(output.sourceObject, m_cameraBrain);

                if (m_director.playableAsset is TimelineAsset timeline)
                {
                    foreach (var track in timeline.GetOutputTracks())
                    {
                        if (track is CinemachineTrack)
                        {
                            foreach (var clip in track.GetClips())
                            {
                                var shot = clip.asset as CinemachineShot;
                                if (shot == null)
                                    continue;

                                shot.VirtualCamera.exposedName = UnityEditor.GUID.Generate().ToString();
                                m_director.SetReferenceValue(shot.VirtualCamera.exposedName, m_cinematicCamer);

                                m_director.RebuildGraph();

                                var vcam = m_cinematicCamer;
                                if (vcam != null)
                                {
                                    vcam.LookAt = source.ModelRoot;
                                }

                                Debug.Log($"[CinematicCamera] Assigned '{vcam.name}' to clip '{clip.displayName}' and set target to '{source.ModelRoot.name}'");
                            }
                        }
                    }
                }
            }


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
