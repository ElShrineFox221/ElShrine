using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace ElShrine.Wpf.Common
{
    public static class EventBatch
    {
        private static readonly ConditionalWeakTable<UIElement, ConcurrentDictionary<RoutedEvent, Delegate>> handlerMap = [];
        public static void RegisterAction(UIElement target, IEnumerable<RoutedEvent> events, Action refreshAction)
        {
            foreach (var routedEvent in events)
            {
                Type handlerType = routedEvent.HandlerType;
                Delegate wrapper = CreateCompatibleDelegate(handlerType, refreshAction);
                target.AddHandler(routedEvent, wrapper);
                if(!handlerMap.TryGetValue(target, out var handlesByEvent)) handlerMap.TryAdd(target, handlesByEvent = []);
                handlesByEvent[routedEvent] = wrapper;
            }
        }
        public static void UnregisterAction(UIElement target, IEnumerable<RoutedEvent> events)
        {
            if (handlerMap.TryGetValue(target, out var handlersByEvent))
            {
                foreach (var routedEvent in events)
                {
                    if (handlersByEvent.TryGetValue(routedEvent, out var handler))
                    {
                        target.RemoveHandler(routedEvent, handler);
                        handlersByEvent.Remove(routedEvent, out _);
                    }
                }
            }
        }

        private static Delegate CreateCompatibleDelegate(Type delegateType, Action action)
        {
            var invokeMethod = delegateType.GetMethod(nameof(EventHandler.Invoke))!;
            var parameters = invokeMethod.GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToArray();
            var actionInvoke = Expression.Call(Expression.Constant(action), action.GetType().GetMethod(nameof(Action.Invoke))!);
            return Expression.Lambda(delegateType, actionInvoke, parameters).Compile();
        }
    }
}
