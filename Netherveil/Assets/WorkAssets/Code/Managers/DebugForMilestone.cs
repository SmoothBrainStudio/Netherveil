using Map;
using UnityEngine;

public class DebugForMilestone : MonoBehaviour
{
    public float corruptionDelta = 1;
    public float benedictionDelta = 1;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Utilities.Hero.HealPlayer(10000000);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            foreach (var enemy in MapUtilities.currentRoomData.Enemies)
            {
                if (enemy != null)
                {
                    var test = enemy.GetComponentInChildren<IDamageable>();
                    if (test != null)
                    {
                        test.Death();
                    }
                }
            }
            MapUtilities.currentRoomData.Enemies.Clear();
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            Utilities.Hero.Inventory.Blood += 100;
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            Utilities.Hero.Stats.IncreaseValue(Stat.CORRUPTION, corruptionDelta);
            Utilities.Hero.DebugCallLaunchUpgrade();
            Utilities.Hero.ChangeStatsBasedOnAlignment();
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            Utilities.Hero.Stats.IncreaseValue(Stat.CORRUPTION, 25);
            Utilities.Hero.DebugCallLaunchUpgrade();
            Utilities.Hero.ChangeStatsBasedOnAlignment();
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            Utilities.Hero.Stats.IncreaseValue(Stat.CORRUPTION, 100);
            Utilities.Hero.DebugCallLaunchUpgrade();
            Utilities.Hero.ChangeStatsBasedOnAlignment();
        }
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            Utilities.Hero.Stats.DecreaseValue(Stat.CORRUPTION, benedictionDelta);
            Utilities.Hero.DebugCallLaunchUpgrade();
            Utilities.Hero.ChangeStatsBasedOnAlignment();
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            Utilities.Hero.Stats.DecreaseValue(Stat.CORRUPTION, 50);
            Utilities.Hero.DebugCallLaunchUpgrade();
            Utilities.Hero.ChangeStatsBasedOnAlignment();
        }
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            Utilities.Hero.Stats.DecreaseValue(Stat.CORRUPTION, 100);
            Utilities.Hero.DebugCallLaunchUpgrade();
            Utilities.Hero.ChangeStatsBasedOnAlignment();
        }
        if (Input.GetKey(KeyCode.KeypadDivide))
        {
            for (int i = 0; i < 10; i++)
                FloatingTextGenerator.CreateActionText(Utilities.Player.transform.position, "<sprite name=\"omg\">", 3);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            foreach (var enemy in MapUtilities.currentRoomData.Enemies)
            {
                if (enemy != null)
                {
                    var test = enemy.GetComponentInChildren<Mobs>();
                    if (test != null)
                    {
                        test.AddStatus(new Electricity(1.3f, 1f));
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            LevelLoader.current.LoadScene("Outro", true);
        }
    }
}
