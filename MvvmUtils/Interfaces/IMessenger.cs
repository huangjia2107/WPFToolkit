﻿using System;

namespace MvvmUtils.Interfaces
{
    public interface IMessenger
    {
        void Register<TMessage>(object recipient, Action<TMessage> action);
        void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action);
        void Register<TMessage>(object recipient, object token, Action<TMessage> action);
        void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action);

        void Send<TMessage>(TMessage message);
        void Send<TMessage>(TMessage message, object token);
        void Send<TMessage, TTarget>(TMessage message);

        void Unregister(object recipient);
        void Unregister<TMessage>(object recipient);
        void Unregister<TMessage>(object recipient, Action<TMessage> action);
        void Unregister<TMessage>(object recipient, object token);
        void Unregister<TMessage>(object recipient, object token, Action<TMessage> action);
    }
}
