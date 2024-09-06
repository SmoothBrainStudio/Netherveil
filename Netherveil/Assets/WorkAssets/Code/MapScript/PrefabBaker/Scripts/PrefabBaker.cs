using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PrefabLightMapBaker
{
    [ExecuteInEditMode]
    public class PrefabBaker : MonoBehaviour
    {
        [SerializeField] public LightInfo[] lights;
        [SerializeField] public Renderer[] renderers;
        [SerializeField] public int[] renderersLightmapIndex;
        [SerializeField] public Vector4[] renderersLightmapOffsetScale;
        [SerializeField] public Texture2D[] texturesColor;
        [SerializeField] public Texture2D[] texturesDir;
        [SerializeField] public Texture2D[] texturesShadow;

        public Texture2D[][] AllTextures() => new Texture2D[][] {
            texturesColor, texturesDir, texturesShadow
        };

        public bool HasBakeData => (renderers?.Length ?? 0) > 0 && (texturesColor?.Length ?? 0) > 0;

        public bool BakeApplied
        {
            get
            {
                bool hasColors = Utils.SceneHasAllLightmaps(texturesColor);
                bool hasDirs = Utils.SceneHasAllLightmaps(texturesDir);
                bool hasShadows = Utils.SceneHasAllLightmaps(texturesShadow);

                return hasColors && hasDirs && hasShadows;
            }
        }

        void Start()
        {
            //BakeApply();

            // Warnning : this will mess up the renderer lightmaps reference
            // // StaticBatchingUtility.Combine( gameObject );
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BakeApply();
        }


        void OnEnable()
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage())
            {
                return;
            }
#endif
            if (!Application.isPlaying || SceneManager.loadedSceneCount > 0)
            {
                BakeApply();
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void BakeApply()
        {
            if (!HasBakeData)
            {
                Debug.LogWarning("PrefabBaker doesn't have bake data", this);
                return;
            }

            if (!BakeApplied)
            {
                if (Utils.Apply(this))
                {
                    //Debug.Log("[PrefabBaker] Addeded prefab lightmap data to current scene", gameObject);
                }
            }
        }
    }
}