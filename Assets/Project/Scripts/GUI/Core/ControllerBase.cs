namespace Project.Scripts.GUI.Core
{
    public abstract class ControllerBase<TView, TModel> : IController
        where TView : IView
        where TModel : IModel
    {
        protected TView View { get; }
        protected TModel Model { get; }
        
        protected ControllerBase(TView view, TModel model)
        {
            View = view;
            Model = model;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Enable()
        {
        }

        public virtual void Disable()
        {
        }

        public virtual void Destroy()
        {
            
        }
    }
}