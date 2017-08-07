using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading; 
using MvvmUtils.Interfaces;
using MvvmUtils.Models;

namespace MvvmUtils.Messaging
{
    public class Messenger : IMessenger
    {
        private struct WeakActionAndToken
        {
            public WeakAction Action;
            public object Token;
        }

        private static IMessenger _defaultInstance;

        private bool _isRequestCleanup;

        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesActionDictionary;
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfStrictActionDictionary;

        private readonly object _registerLock = new object();
        private static readonly object CreationLock = new object();
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public static IMessenger Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (CreationLock)
                        _defaultInstance = new Messenger();
                }

                return _defaultInstance;
            }
        }

        #region IMessenger Members

        public virtual void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            Register<TMessage>(recipient, null, false, action);
        }

        public virtual void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            Register<TMessage>(recipient, null, receiveDerivedMessagesToo, action);
        }

        public virtual void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            Register<TMessage>(recipient, token, false, action);
        }

        public virtual void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (_registerLock)
            {
                Dictionary<Type, List<WeakActionAndToken>> dictionary;
                if (receiveDerivedMessagesToo)
                {
                    if (_recipientsOfSubclassesActionDictionary == null)
                        _recipientsOfSubclassesActionDictionary = new Dictionary<Type, List<WeakActionAndToken>>();

                    dictionary = _recipientsOfSubclassesActionDictionary;
                }
                else
                {
                    if (_recipientsOfStrictActionDictionary == null)
                        _recipientsOfStrictActionDictionary = new Dictionary<Type, List<WeakActionAndToken>>();

                    dictionary = _recipientsOfStrictActionDictionary;
                }

                lock (dictionary)
                {
                    Type msgType = typeof(TMessage);
                    List<WeakActionAndToken> list;
                    if (!dictionary.ContainsKey(msgType))
                    {
                        list = new List<WeakActionAndToken>();
                        dictionary.Add(msgType, list);
                    }
                    else
                        list = dictionary[msgType];

                    list.Add(new WeakActionAndToken
                    {
                        Action = new WeakAction<TMessage>(recipient, action),
                        Token = token
                    });
                }
            }

            RequestCleanup();
        }

        public virtual void Send<TMessage>(TMessage message)
        {
            SendToTarget(message, null, null);
        }

        public virtual void Send<TMessage>(TMessage message, object token)
        {
            SendToTarget(message, null, token);
        }

        public virtual void Send<TMessage, TTarget>(TMessage message)
        {
            SendToTarget(message, typeof(TTarget), null);
        }

        public virtual void Unregister(object recipient)
        {
            UnregisterFromDictionary(recipient, _recipientsOfStrictActionDictionary);
            UnregisterFromDictionary(recipient, _recipientsOfSubclassesActionDictionary);

            RequestCleanup();
        }

        public virtual void Unregister<TMessage>(object recipient)
        {
            Unregister<TMessage>(recipient, null, null);
        }

        public virtual void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            Unregister<TMessage>(recipient, null, action);
        }

        public virtual void Unregister<TMessage>(object recipient, object token)
        {
            Unregister<TMessage>(recipient, token, null);
        }

        public virtual void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            UnregisterFromDictionary(recipient, token, action, _recipientsOfStrictActionDictionary);
            UnregisterFromDictionary(recipient, token, action, _recipientsOfSubclassesActionDictionary);

            RequestCleanup();
        }

        #endregion

        #region Reset

        public static void Reset()
        {
            _defaultInstance = null;
        }

        public void ResetDefault()
        {
            Reset();
        }

        #endregion

        #region Cleanup

        public void RequestCleanup()
        {
            if (!_isRequestCleanup)
            {
                Action cleanupAction = new Action(Cleanup);
                if (_synchronizationContext != null)
                    _synchronizationContext.Post(_ => cleanupAction(), null);
                else
                    cleanupAction();

                _isRequestCleanup = true;
            }
        }

        public void Cleanup()
        {
            CleanupDictionary(_recipientsOfStrictActionDictionary);
            CleanupDictionary(_recipientsOfSubclassesActionDictionary);

            _isRequestCleanup = false;
        }

        private void CleanupDictionary(Dictionary<Type, List<WeakActionAndToken>> dictionary)
        {
            if (dictionary == null)
                return; ;

            lock (dictionary)
            {
                List<Type> removeList = new List<Type>();
                foreach (var current in dictionary)
                {
                    foreach (var weakActionAndToken in current.Value.Where(at => at.Action == null || !at.Action.IsAlive).ToList())
                    {
                        current.Value.Remove(weakActionAndToken);
                    }
                    if (current.Value.Count == 0)
                        removeList.Add(current.Key);
                }

                removeList.ForEach(type => dictionary.Remove(type));
            }
        }

        #endregion

        #region Send

        private void SendToTarget<TMessage>(TMessage message, Type targetType, object token)
        {
            Type msgType = typeof(TMessage);
            List<WeakActionAndToken> resultList = null;
            if (_recipientsOfSubclassesActionDictionary != null)
            {
                foreach (var type in _recipientsOfSubclassesActionDictionary.Keys.ToList())
                {
                    if (msgType == type || msgType.IsSubclassOf(type) || type.IsAssignableFrom(msgType))
                    {
                        lock (_recipientsOfSubclassesActionDictionary)
                            resultList = _recipientsOfSubclassesActionDictionary[type];

                        SendToList(message, resultList, targetType, token);
                    }
                }
            }

            if (_recipientsOfStrictActionDictionary != null)
            {
                lock (_registerLock)
                {
                    if (_recipientsOfStrictActionDictionary.ContainsKey(msgType))
                        resultList = _recipientsOfStrictActionDictionary[msgType];
                }

                SendToList(message, resultList, targetType, token);
            }

            RequestCleanup();
        }

        private void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionAndTokens, Type targetType,
            object token)
        {
            if (weakActionAndTokens != null)
            {
                foreach (var weakActionAndToken in weakActionAndTokens)
                {
                    WeakAction<TMessage> weakAction = weakActionAndToken.Action as WeakAction<TMessage>;
                    if (weakAction != null
                        && weakAction.IsAlive
                        && weakAction.Target != null
                        && (targetType == null || weakAction.Target.GetType() == targetType || targetType.IsAssignableFrom(weakAction.Target.GetType()))
                        && ((weakActionAndToken.Token == null && token == null) || (weakActionAndToken.Token != null && weakActionAndToken.Token.Equals(token)))
                        )
                    {
                        weakAction.Execute(message);
                    }
                }
            }
        }

        #endregion

        #region Unregister

        private static void UnregisterFromDictionary(object recipient, Dictionary<Type, List<WeakActionAndToken>> dictionary)
        {
            if (recipient == null || dictionary == null || dictionary.Count == 0)
                return;

            lock (dictionary)
            {
                foreach (var type in dictionary.Keys)
                {
                    dictionary[type].ForEach(weakActionAndToken =>
                    {
                        if (weakActionAndToken.Action != null && weakActionAndToken.Action.Target == recipient)
                            weakActionAndToken.Action.MarkForDeletion();
                    });
                }
            }
        }

        private static void UnregisterFromDictionary<TMessage>(object recipient, object token, Action<TMessage> action, Dictionary<Type, List<WeakActionAndToken>> dictionary)
        {
            var msgType = typeof(TMessage);
            if (recipient == null || dictionary == null || dictionary.Count == 0 || !dictionary.ContainsKey(msgType))
                return;

            lock (dictionary)
            {
                foreach (var weakAction in from weakActionAndToken in dictionary[msgType] let weakAction = weakActionAndToken.Action as WeakAction<TMessage> where weakAction != null && recipient == weakAction.Target && (action == null || action.Method.Name == weakAction.MethodName) && (token == null || token.Equals(weakActionAndToken.Token)) select weakAction)
                {
                    weakAction.MarkForDeletion();
                }
            }
        }

        #endregion
    }
}
