#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

public class PreviewPicture : MonoBehaviour
{
    [SerializeField] private string spritePath = "Assets/SampleSceneAssets/Art/Sprites/Items";
    [SerializeField] private Vector2Int RenderSize = new Vector2Int(2048, 2048);
    [SerializeField] private RenderTexture renderTexture;

    [SerializeField] private GameObject[] toPictures;

    public void TakePicture()
    {
        StartCoroutine(TakePictureRoutine());
    }

    private IEnumerator TakePictureRoutine()
    {
        //renderTexture = new RenderTexture(RenderSize.x, RenderSize.y, 16);
        //renderTexture.Create();

        DisableAllToPictures();
        GameObject lastObject = null;

        foreach (var p in toPictures)
        {
            if (lastObject != null)
                lastObject.SetActive(false);

            p.SetActive(true);
            lastObject = p;

            yield return new WaitForEndOfFrame();

            Picture(spritePath, p.name);
        }

        //renderTexture.Release();
        //renderTexture = null;
    }

    private void DisableAllToPictures()
    {
        foreach (var p in toPictures)
        {
            p.SetActive(false);
        }
    }

    private void Picture(string path, string fileName)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();

        System.IO.File.WriteAllBytes($"{path}/{fileName}.png", bytes);
        AssetDatabase.Refresh();
    }
}
#endif