using UnityEngine;

public class DisableDashTuto : MonoBehaviour
{
    public FadeOutText textTofadeOut;
    bool fistEnter = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            if(!fistEnter) 
            {
                fistEnter = true;
                textTofadeOut.fadeOut = true;
            }
        }
    }
}
