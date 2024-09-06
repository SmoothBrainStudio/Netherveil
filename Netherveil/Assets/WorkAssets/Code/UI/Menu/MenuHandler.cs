using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] protected List<MenuPart> menuItems = new List<MenuPart>();

    private void Start()
    {
        CloseAllMenus();
    }

    private void CloseAllMenus()
    {
        menuItems.ForEach(m => m.CloseMenu());
    }

    public void OpenMenu(MenuPart item)
    {
        CloseAllMenus();
        item.OpenMenu();
    }

    public void OpenMenu(int index)
    {
        CloseAllMenus();
        menuItems[index].OpenMenu();
    }
}
