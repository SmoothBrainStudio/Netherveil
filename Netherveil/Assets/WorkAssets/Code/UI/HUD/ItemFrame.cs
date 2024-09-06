using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemFrame : MonoBehaviour
{
    [SerializeField] protected Image background;
    [SerializeField] protected Image item;

    public void SetFrame(Sprite _backgroundSprite, Sprite _itemSprite)
    {
        background.gameObject.SetActive(true);
        item.gameObject.SetActive(true);

        background.sprite = _backgroundSprite;
        item.sprite = _itemSprite;
        

        if (_backgroundSprite == null)
            background.gameObject.SetActive(false);
        if (_itemSprite == null)
            item.gameObject.SetActive(false);
    }
}
