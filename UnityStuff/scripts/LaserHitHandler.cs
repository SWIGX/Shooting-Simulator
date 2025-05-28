using UnityEngine;

public class LaserHitHandler : MonoBehaviour
{
    public ScoreManager scoreManager;
    public TagHitEffect tagHitEffect;

    public void RegisterHit(GameObject hitObject, Vector3 hitPoint)
    {
        if (tagHitEffect)
            tagHitEffect.PlayHitEffect(hitPoint);

        ResetTag resetTag = hitObject.GetComponent<ResetTag>();
        if (resetTag != null)
        {
            resetTag.OnHit();
            return;
        }

        scoreManager.AddShot(true);
    }

    public void RegisterMiss()
    {
        scoreManager.AddShot(false);
    }
}
