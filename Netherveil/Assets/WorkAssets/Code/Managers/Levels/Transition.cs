using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Transition : MonoBehaviour
{
    [SerializeField] private bool playOnAwake = true;

    private event Action onTransitionEnd;

    private Animator animator;
    private bool enable = false;
    private bool transitionEnd = true;

    public Action OnTransitionEnd => onTransitionEnd;
    public bool TransitionEnd => transitionEnd;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playOnAwake)
        {
            FadeOut();
        }
    }

    private void OnEnable()
    {
        onTransitionEnd += () => transitionEnd = true;
    }

    private void OnDisable()
    {
        onTransitionEnd -= () => transitionEnd = true;
    }

    public void Toggle()
    {
        if (enable)
            FadeIn();
        else
            FadeOut();
    }

    public void FadeOut()
    {
        if (enable)
        {
            Debug.LogWarning("Transition already enable");
            return;
        }

        enable = true;
        transitionEnd = false;
        animator.ResetTrigger("FadeOut");
        animator.SetTrigger("FadeOut");
    }

    public void FadeIn()
    {
        if (!enable)
        {
            Debug.LogWarning("Transition already disable");
            return;
        }

        enable = false;
        transitionEnd = false;
        animator.ResetTrigger("FadeIn");
        animator.SetTrigger("FadeIn");
    }
}
