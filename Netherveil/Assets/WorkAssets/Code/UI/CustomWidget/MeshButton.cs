using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeshUI
{
    [RequireComponent(typeof(Collider))]
    public class MeshButton : Selectable, ISubmitHandler
    {
        private bool isHovered = false;
        private bool isPressed = false;
        private bool isSelect = false;

        [Header("Events"), Space]
        [SerializeField] private UnityEvent onHoverEnter;
        [SerializeField] private UnityEvent onHoverExit;
        [SerializeField] private UnityEvent onPress;
        [SerializeField] private UnityEvent onRelease;

        private void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!isHovered)
                    {
                        OnHover();
                    }

                    isHovered = true;

                    if (Input.GetMouseButtonDown(0))
                    {
                        OnPress();
                        isPressed = true;
                    }
                }
                else
                {
                    if (isHovered)
                    {
                        OnOut();
                    }

                    isHovered = false;
                }
            }
            else
            {
                if (isHovered)
                {
                    OnOut();
                }

                isHovered = false;
            }

            if (Input.GetMouseButtonUp(0) && isPressed)
            {
                OnRelease();
                isPressed = false;
            }
        }

        public void OnHover()
        {
            if (!isSelect)
                onHoverEnter?.Invoke();
        }

        public void OnOut()
        {
            if (!isSelect)
                onHoverExit?.Invoke();
        }

        public void OnPress()
        {
            onPress?.Invoke();
        }

        public void OnRelease()
        {
            onRelease?.Invoke();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            OnHover();
            isSelect = true;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            isSelect = false;
            OnOut();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            OnPress();
        }
    }
}
