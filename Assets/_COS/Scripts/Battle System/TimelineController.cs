using System.Threading.Tasks;
using UnityEngine;

public class TimelineController : MonoBehaviour
{
    public static TimelineController Instance { get; private set; }

    private void Awake() => Instance = this;

    /// <summary>
    /// Play attack animation for the attacker (single animation even if AttackPoints > 1).
    /// Must return a Task that completes when the animation/VFX finished.
    /// Replace the body with Timeline/Playable logic when wiring animations.
    /// </summary>
    public Task PlayAttackAnimationAsync(Combatant attacker, Combatant target)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Example implementation: trigger an Animator or Timeline here.
        // For prototype we just wait a fixed time (simulate animation).
        float simulatedMs = 900f; // animation length in ms (tweak)
        StartCoroutine(WaitThenComplete(simulatedMs / 1000f, tcs));

        return tcs.Task;
    }

    private System.Collections.IEnumerator WaitThenComplete(float seconds, TaskCompletionSource<bool> tcs)
    {
        yield return new WaitForSeconds(seconds);
        tcs.SetResult(true);
    }
}
