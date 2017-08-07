using System; 
using MvvmUtils.Helpers;

namespace MvvmUtils.Models
{
    public class WeakFuncT<T, TResult> : WeakFunc<TResult>
    {
        //Fields
        private Func<T, TResult> _staticFunc;

        //Properties
        public override string MethodName
        {
            get { return _staticFunc != null ? _staticFunc.Method.Name : Method.Name; }
        }

        public override bool IsAlive
        {
            get
            {
                if (Reference != null)
                    return Reference.IsAlive;

                return (_staticFunc != null);
            }
        }

        public override bool IsStatic
        {
            get { return _staticFunc != null; }
        } 

        public WeakFuncT(Func<T, TResult> func)
            : this(func == null ? null : func.Target, func)
        { }

        public WeakFuncT(object target, Func<T, TResult> func)
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

        public override TResult Execute()
        {
            return default(TResult);
        }

        public TResult Execute(T paramater)
        {
            if (_staticFunc != null)
                return _staticFunc.Invoke(paramater);
            else
            {
                if (IsAlive && Method != null && FuncReference != null && FuncTarget != null)
                    return (TResult)Method.Invoke(FuncTarget, new object[] { paramater });
            }

            return default(TResult);
        }

        public override void MarkForDeletion()
        {
            _staticFunc = null;
            base.MarkForDeletion();
        }
    }
}
