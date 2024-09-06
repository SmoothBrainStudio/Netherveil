using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsMenu : MenuHandler
{
    public VideoSettingsPart VideoSettingsPart => menuItems.First(x => x.GetComponent<VideoSettingsPart>()) as VideoSettingsPart;
    public AudioSettingsPart AudioSettingsPart => menuItems.First(x => x.GetComponent<AudioSettingsPart>()) as AudioSettingsPart;
    public ControlsSettingsPart ControlsSettingsPart => menuItems.First(x => x.GetComponent<ControlsSettingsPart>()) as ControlsSettingsPart;

    private void Start()
    {
        OpenMenu(0);
    }
}
