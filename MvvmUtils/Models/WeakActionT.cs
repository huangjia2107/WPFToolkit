using System; 
using MvvmUtils.Helpers;

namespace MvvmUtils.Models
{
    public class WeakAction<T> : WeakAction
    {
        //Fields
        private Action<T> _staticAction;

        //Properties
        public override string MethodName
        {
            get { return _staticAction == null ? Method.Name : _staticAction.Method.Name; }
        }

        public override bool IsAlive
        {
            get
            {
                if (Reference != null)
                    return Reference.IsAlive;

                return (_staticAction != null);
            }
        }

        public override bool IsStatic
        {
            get { return _staticAction != null; }
        }

        public WeakAction(Action<T> action)
            : this(action == null ? null : action.Target, action)
        {

        }

        public WeakAction(object target, Action<T> action)
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

        public override void Execute()
        {
            Execute(default(T));
        }

        public void Execute(T parameter)
        {
            if (_staticAction != null)
                _staticAction.Invoke(parameter);
            else
            {
                if (IsAlive && Method != null && ActionReference != null && ActionTarget != null)
                    Method.Invoke(ActionTarget, new object[] { parameter });
            }
        }

        public override void MarkForDeletion()
        {
            _staticAction = null;
            base.MarkForDeletion();
        }
    }
}
