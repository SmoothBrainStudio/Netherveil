using Map.Generation;
using UnityEngine;
using UnityEngine.Playables;

namespace Map
{
    public class NextLevelTrigger : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                director.Play();
                Utilities.Hero.State = (int)Hero.PlayerState.MOTIONLESS;
            }
        }

        public void CallFade(bool enable)
        {
            if (enable)
                InGameManager.current.publicFade.FadeIn();
            else
                InGameManager.current.publicFade.FadeOut();
        }

        public void CallNextLevel()
        {
            Utilities.Hero.State = (int)Entity.EntityState.MOVE;

            MapUtilities.onFinishStage?.Invoke();

            if (MapUtilities.Stage == 3)
            {
                FindObjectOfType<LevelLoader>().LoadScene("Outro", true);
                return;
            }

            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            mapGen.DestroyMap();
            mapGen.generate = true;
        }
    }
}
