using UnityEngine;

namespace Project.Scripts.GUI.Core
{
    public abstract class SystemBase<TModel,TView,TController> : MonoBehaviour where TView : ViewBase where TModel : IModel where TController : ControllerBase<TView, TModel>
    {
        public TView View;
        public TController Controller { get; private set; }
        public TModel Model { get; private set; }

        protected virtual void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            Controller.Enable();
        }

        private void OnDisable()
        {
            Controller.Disable();
        }

        private void OnDestroy()
        {
            Controller.Destroy();
        }

        protected virtual void Build()
        {
            Model = CreateModel();
            Controller = CreateController(View, Model);
            Controller.Initialize();
        }

        protected abstract TModel CreateModel();
        protected abstract TController CreateController(TView view, TModel model);
    }
}