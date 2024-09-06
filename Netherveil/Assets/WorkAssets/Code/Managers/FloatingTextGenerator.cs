using UnityEngine;

static public class FloatingTextGenerator
{
    static readonly int MAX_SIZE = 95;
    static readonly int MIN_SIZE = 40;
    static readonly int ACTION_TEXT_SIZE = 35;
    static Color critColor = new(0.67f, 0.06f, 0.06f);
    static Color healColor = new(0.5f, 0.72f, 0.09f);
    static Color actionColor = new(0.75f, 0.75f, 0.75f);

    public static void CreateDamageText(int dmgPt, Vector3 pos, bool isCrit = false, int randScale = 1)
    {
        Color color = (isCrit ? critColor : Color.white);
        CreateNumberText(dmgPt, pos, color, randScale);
    }

    public static void CreateEffectDamageText(int dmgPt, Vector3 pos, Color customColor, int randScale = 1) 
    {
        CreateNumberText(dmgPt, pos, customColor, randScale);
    }

    public static void CreateHealText(int healPt, Vector3 pos, int randScale = 1)
    {
        CreateNumberText(healPt, pos, healColor, randScale);
    }

    public static void CreateActionText(Vector3 pos, string text, int randScale = 1)
    {
        FloatingText newText = CreateText(pos, text, actionColor, randScale);
        newText.SetSize(ACTION_TEXT_SIZE);
    }

    public static void CreateActionText(Vector3 pos, string text,Color customColor, int randScale = 1)
    {
        FloatingText newText = CreateText(pos, text, customColor, randScale);
        newText.SetSize(ACTION_TEXT_SIZE);
    }

    private static void CreateNumberText(int nb, Vector3 pos, Color color, int randScale = 1)
    {
        FloatingText newText = CreateText(pos, nb.ToString(), color, randScale);
        newText.toggleTextReduction = true;
        int size = Mathf.Clamp(nb + MIN_SIZE, MIN_SIZE, MAX_SIZE);
        newText.SetSize(size);
    }

    private static FloatingText CreateText(Vector3 pos, string text, Color color, int randScale = 1)
    {
        // offset
        pos += Vector3.up * 2;
        Vector3 randomOffsetVec = Random.onUnitSphere * randScale;
        pos += new Vector3(randomOffsetVec.x, 0f, randomOffsetVec.z);

        var newText = GameObject.Instantiate(GameResources.Get<GameObject>("FloatingText"), pos, Camera.main.transform.rotation).GetComponent<FloatingText>();
        newText.SetColor(color);
        newText.SetText(text);

        return newText;
    }
}
