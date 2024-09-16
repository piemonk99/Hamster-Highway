using UnityEngine;

public class AudioClipPlayer
{
    public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position)
    {
        return PlayClipAtPoint(clip, position, Options.Instance.Volume);
    }

    public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume)
    {
        GameObject gameObject = new GameObject("One shot audio");
        gameObject.transform.position = position;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0;
        audioSource.Play();
        Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        return audioSource;
    }
}
