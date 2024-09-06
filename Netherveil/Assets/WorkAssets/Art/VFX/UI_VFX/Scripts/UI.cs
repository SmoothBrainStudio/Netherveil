using System.Collections.Generic;
using UnityEngine;


public class UI : MonoBehaviour
{

    [SerializeField] TMPro.TMP_Dropdown DropDownMenu;
    ParticleSystem[] dropOption;
    void Start()
    {
        DropDownMenu.ClearOptions();

        ParticleSystem[] tempArray = FindObjectsOfType<ParticleSystem>();
        List<ParticleSystem> tempList = new List<ParticleSystem>();
        foreach (ParticleSystem particle in tempArray)
        {
            if (particle.transform.parent != null)
            {
                if (particle.GetComponentInParent<ParticleSystem>() == null)
                {
                    tempList.Add(particle);
                }
            }
            else
            {
                tempList.Add(particle);
            }
        }
        dropOption = tempList.ToArray();

        for (int i = 0; i < dropOption.Length; i++) 
        {

            DropDownMenu.AddOptions(new List<string> { dropOption[i].name });
            Debug.Log(dropOption[i]);
        }

        for (int i = 0;i < DropDownMenu.options.Count; i++) 
        { 
            Debug.Log(DropDownMenu.options[i].text);
        }

    }

    public void PlayParticleSystem()
    {
        int index = DropDownMenu.value;
        for (int i = 0;i < dropOption.Length;i++) 
        {
            
            if (DropDownMenu.options[index].text == dropOption[i].name) 
            {
                if (dropOption[i].isPlaying) 
                { 
                
                }
                else
                {
                    dropOption[i].Play();
                }
            }
        }
    }

    public void PauseParticleSystem()
    {
        int index = DropDownMenu.value;
        for (int i = 0; i < dropOption.Length; i++)
        {

            if (DropDownMenu.options[index].text == dropOption[i].name)
            {
                if (dropOption[i].isPaused)
                {
                    dropOption[i].Play();
                }
                else if (dropOption[i].isPlaying)
                {
                    dropOption[i].Pause();
                }

            }
        }
    }

    public void StopParticleSystem()
    {
        int index = DropDownMenu.value;
        for (int i = 0; i < dropOption.Length; i++)
        {

            if (DropDownMenu.options[index].text == dropOption[i].name)
            {
                dropOption[i].Stop();
                dropOption[i].Clear();
            }
        }
    }
    
    public void StopAllParticleSystem()
    {
        int index = DropDownMenu.value;
        for (int i = 0; i < dropOption.Length; i++)
        {

            if (DropDownMenu.options[i].text == dropOption[i].name)
            {
                dropOption[i].Stop();
                dropOption[i].Clear();
            }
        }
    }

    void Update()
    {



            
    }
}
