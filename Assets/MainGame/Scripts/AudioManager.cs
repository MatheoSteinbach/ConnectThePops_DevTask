using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] AudioClip tapSound;
    [SerializeField] AudioClip mergeSound;

    private AudioSource source;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        source = GetComponent<AudioSource>();
    }

    public void MergeSound()
    {
        source.PlayOneShot(mergeSound);
    }
    public void ConnectSound(float _pitch)
    {
        source.pitch = _pitch;
        source.PlayOneShot(tapSound);
    }
    public void ToggleMute(bool _toggle)
    {
        if(!_toggle)
        {
            source.volume = 0;
        }
        else
        {
            source.volume = 1f;
        }
    }
}
