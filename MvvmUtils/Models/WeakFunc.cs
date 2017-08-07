using System; 
using System.Reflection; 
using MvvmUtils.Helpers;

namespace MvvmUtils.Models
{
    public class WeakFunc<TResult>
    {
        //Fields
        private Func<TResult> _staticFunc;

        //Properties
        protected WeakReference Reference { get; set; }
        public object Target
        {
            get { return Reference == null ? null : Reference.Target; }
        }

        protected WeakReference FuncReference { get; set; }
        protected object FuncTarget
        {
            get { return FuncReference == null ? null : FuncReference.Target; }
        }

        protected MethodInfo Method { get; set; }
        public virtual string MethodName
        {
            get { return _staticFunc != null ? _staticFunc.Method.Name : Method.Name; }
        }

        public virtual bool IsAlive
        {
            get
            {
                if (Reference != null)
                    return Reference.IsAlive;

                return (_staticFunc != null);
            }
        }

        public virtual bool IsStatic
        {
            get { return _staticFunc != null; }
        }

        protected WeakFunc() { }

        public WeakFunc(Func<TResult> func)
            : this(func == null ? null : func.Target, func)
        { }

        public WeakFunc(object target, Func<TResult> func)
        {
            Argument.IsNotNull("func", func);

            if (func.Method.IsStatic)
            {
                _staticFunc = func;
                if (target != null)
                    Reference = new WeakReference(target);
            }
            else
            {
                Method = func.Method;
                FuncReference = new WeakReference(func.Target);
                Reference = new WeakReference(target);
            }
        }

        public virtual TResult Execute()
        {
            if (_staticFunc != null)
                return _staticFunc.Invoke();
            else
            {
                if (IsAlive && Method != null && FuncReference != null && FuncTarget != null)
                    return (TResult)Method.Invoke(FuncTarget, null);
            }

            return default(TResult);
        }

        public virtual void MarkForDeletion()
        {
            Reference = null;
            FuncReference = null;
            Method = null;
            _staticFunc = null;
        }
    }
}
