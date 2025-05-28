using UnityEngine;

public class ResetTag : MonoBehaviour
{
    public ScoreManager scoreManager;
    public TagHitEffect tagHitEffect;

    public void OnHit()
    {
        if (tagHitEffect)
            tagHitEffect.PlayHitEffect(transform.position);

        if (scoreManager)
            scoreManager.ResetGame();
    }
}
