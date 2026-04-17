using UnityEngine;

namespace Project.Scripts.GUI.Core
{
    public abstract class ViewBase : MonoBehaviour, IView
    {
        public RectTransform Root;

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnEnable()
        {
            
        }

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void Initialize()
        {
            
        }

        public virtual void Open()
        {
        }

        public virtual void OpenImmediately()
        {
            if (Root != null)
            {
                Root.gameObject.SetActive(true);
            }
        }

        public virtual void Close()
        {
        }

        public virtual void CloseImmediately()
        {
            if (Root != null)
            {
                Root.gameObject.SetActive(false);
            }
        }
    }
}