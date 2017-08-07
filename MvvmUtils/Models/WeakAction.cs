using System;
using System.Reflection;
using MvvmUtils.Helpers;

namespace MvvmUtils.Models
{
    public class WeakAction
    {
        //Fields
        private Action _staticAction;

        //Properties
        protected WeakReference Reference { get; set; }
        public object Target
        {
            get { return Reference == null ? null : Reference.Target; }
        }

        protected WeakReference ActionReference { get; set; }
        protected object ActionTarget
        {
            get { return ActionReference == null ? null : ActionReference.Target; }
        }

        protected MethodInfo Method { get; set; }
        public virtual string MethodName
        {
            get { return _staticAction != null ? _staticAction.Method.Name : Method.Name; }
        }

        public virtual bool IsAlive
        {
            get
            {
                if (Reference != null)
                    return Reference.IsAlive;

                return (_staticAction != null);
            }
        }

        public virtual bool IsStatic
        {
            get { return _staticAction != null; }
        }

        protected WeakAction() { }

        public WeakAction(Action action)
            : this(action == null ? null : action.Target, action)
        {

        }

        public WeakAction(object target, Action action)
        {
            Argument.IsNotNull("action", action);

            if (action.Method.IsStatic)
            {
                _staticAction = action;
                if (target != null)
                    Reference = new WeakReference(target);
            }
            else
            {
                Method = action.Method;
                ActionReference = new WeakReference(action.Target);
                Reference = new WeakReference(target);
            }
        }

        public virtual void Execute()
        {
            if (_staticAction != null)
                _staticAction.Invoke();
            else
            {
                if (IsAlive && Method != null && ActionReference != null && ActionTarget != null)
                    Method.Invoke(ActionTarget, null);
            }
        }

        public virtual void MarkForDeletion()
        {
            Reference = null;
            ActionReference = null;
            Method = null;
            _staticAction = null;
        }
    }
}
