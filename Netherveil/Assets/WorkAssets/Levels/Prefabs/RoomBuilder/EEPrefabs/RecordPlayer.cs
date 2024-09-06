using FMOD.Studio;
using FMODUnity;
using UnityEngine;


public class RecordPlayer : MonoBehaviour, IInterractable
{

    public EventReference AllMyTearsMusic;
    public EventReference EnzoMusic;
    [SerializeField] ParticleSystem MusicNote;
    bool IsCollide;
    bool IsMusicPlaying = false;
    EventInstance eventMusic;
    Hero hero;
    PlayerInteractions interactions;
    bool isSelect = false;

    void Start()
    {
        MusicNote.Pause();
        hero = FindObjectOfType<Hero>();
        interactions = hero.GetComponent<PlayerInteractions>();
    }

    private void OnTriggerEnter(Collider collide)
    {
        if (collide.tag == "Player")
        {
            IsCollide = true;
        }
    }


    void Update()
    {
        Interraction();
        eventMusic.getPlaybackState(out PLAYBACK_STATE playbackState);
        IsMusicPlaying = playbackState == PLAYBACK_STATE.PLAYING;
        if (playbackState == PLAYBACK_STATE.STOPPING)
        {
            MusicNote.Stop();
        }
    }

    void playMusic()
    {
        switch (UnityEngine.Random.Range(0, 2))
        {
            case 0:
                eventMusic = AudioManager.Instance.PlaySound(AllMyTearsMusic);
                break;
            case 1:
                eventMusic = AudioManager.Instance.PlaySound(EnzoMusic);
                break;
            default:
                eventMusic = AudioManager.Instance.PlaySound(AllMyTearsMusic);
                break;
        }

        
        MusicNote.Play();
        eventMusic.getPlaybackState(out PLAYBACK_STATE playbackState);
        eventMusic.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
        IsMusicPlaying = playbackState == PLAYBACK_STATE.PLAYING;
    }

    private void Interraction()
    {
        bool isInRange = Vector2.Distance(interactions.transform.position.ToCameraOrientedVec2(), transform.position.ToCameraOrientedVec2())
            <= hero.Stats.GetValue(Stat.CATCH_RADIUS);

        if (isInRange && !interactions.InteractablesInRange.Contains(this))
        {
            interactions.InteractablesInRange.Add(this);
        }
        else if (!isInRange && interactions.InteractablesInRange.Contains(this))
        {
            interactions.InteractablesInRange.Remove(this);
            Deselect();
        }
    }

    public void Interract()
    {
        if (IsCollide == true && !IsMusicPlaying)
        {
            playMusic();
        }
    }

    public void Select()
    {
        if (isSelect)
            return;

        isSelect = true;
    }

    public void Deselect()
    {
        if (!isSelect)
            return;

        isSelect = false;
    }
}
