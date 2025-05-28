using UnityEngine;

public class TagHitEffect : MonoBehaviour
{
    public GameObject hitEffectPrefab;
    public GameObject resetEffectPrefab;
    public AudioClip hitSound;
    public AudioClip resetSound;

    public void PlayHitEffect(Vector3 hitPosition, bool isReset = false)
    {
        GameObject prefab = isReset ? resetEffectPrefab : hitEffectPrefab;
        AudioClip sound = isReset ? resetSound : hitSound;

        if (prefab)
            Instantiate(prefab, hitPosition, Quaternion.identity);

        if (sound)
            AudioSource.PlayClipAtPoint(sound, hitPosition);
    }
}
