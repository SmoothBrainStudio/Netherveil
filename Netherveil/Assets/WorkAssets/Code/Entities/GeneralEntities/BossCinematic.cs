using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class BossCinematic : MonoBehaviour
{
    private PlayableDirector director;

    public bool EnablePlayerMouvement
    {
        set
        {
            if (value)
            {
                Utilities.Hero.State = (int)Entity.EntityState.MOVE;
                Utilities.Hero.GetComponent<PlayerInput>().EnableGameplayInputs();
            }
            else
            {
                Utilities.Hero.State = (int)Hero.PlayerState.MOTIONLESS;
                Utilities.Hero.GetComponent<PlayerInput>().DisableGameplayInputs();
            }
        }
    }
    public bool EnableHUD
    {
        set
        {
            HudHandler.current.SetActive(value, 0.25f);
        }
    }

    private void Awake()
    {
        director = GetComponentInChildren<PlayableDirector>();
    }

    private void OnEnable()
    {
        foreach (var output in director.playableAsset.outputs)
        {
            CinemachineTrack cinemachineTrack = output.sourceObject as CinemachineTrack;
            if (cinemachineTrack != null)
            {
                CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
                director.SetGenericBinding(cinemachineTrack, brain);
            }
        }
    }

    public void Play()
    {
        director.Play();
    }
}
