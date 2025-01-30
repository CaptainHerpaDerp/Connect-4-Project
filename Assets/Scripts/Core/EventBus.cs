using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Accessed by other classes to subscribe to and publish events
    /// </summary>
    public class EventBus : Singleton<EventBus>
    {
        private Dictionary<string, Delegate> eventHandlers = new Dictionary<string, Delegate>();

        /// <summary>
        /// Subscribe to an event with a single parameter.
        /// </summary>
        public void Subscribe<T>(string eventName, Action<T> handler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = null;
            }

            eventHandlers[eventName] = Delegate.Combine(eventHandlers[eventName], handler);
        }

        /// <summary>
        /// Subscribe to an event with two parameters.
        /// </summary>
        public void Subscribe<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = null;
            }

            eventHandlers[eventName] = Delegate.Combine(eventHandlers[eventName], handler);
        }

        /// <summary>
        /// Subscribe to an event with three parameters.
        /// </summary>
        public void Subscribe<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = null;
            }

            eventHandlers[eventName] = Delegate.Combine(eventHandlers[eventName], handler);
        }

        /// <summary>
        /// Subscribe to an event with no parameters.
        /// </summary>
        public void Subscribe(string eventName, Action handler)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = null;
            }

            eventHandlers[eventName] = Delegate.Combine(eventHandlers[eventName], handler);
        }

        /// <summary>
        /// Unsubscribe from an event.
        /// </summary>
        public void Unsubscribe(string eventName, Delegate handler)
        {
            if (eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = Delegate.Remove(eventHandlers[eventName], handler);
            }
        }

        /// <summary>
        /// Publish an event with a single parameter.
        /// </summary>
        public void Publish<T>(string eventName, T eventData)
        {
            if (eventHandlers.TryGetValue(eventName, out var handler) && handler != null)
            {
                foreach (var invocation in handler.GetInvocationList())
                {
                    (invocation as Action<T>)?.Invoke(eventData);
                }
            }
        }

        /// <summary>
        /// Publish an event with two parameters.
        /// </summary>
        public void Publish<T1, T2>(string eventName, T1 data1, T2 data2)
        {
            if (eventHandlers.TryGetValue(eventName, out var handler) && handler != null)
            {
                foreach (var invocation in handler.GetInvocationList())
                {
                    (invocation as Action<T1, T2>)?.Invoke(data1, data2);
                }
            }
        }

        public void Publish<T1, T2, T3>(string eventName, T1 data1, T2 data2, T3 data3)
        {
            if (eventHandlers.TryGetValue(eventName, out var handler) && handler != null)
            {
                foreach (var invocation in handler.GetInvocationList())
                {
                    (invocation as Action<T1, T2, T3>)?.Invoke(data1, data2, data3);
                }
            }
        }

        /// <summary>
        /// Publish an event with no parameters.
        /// </summary>
        public void Publish(string eventName)
        {
            if (eventHandlers.TryGetValue(eventName, out var handler) && handler != null)
            {
                foreach (var invocation in handler.GetInvocationList())
                {
                    (invocation as Action)?.Invoke();
                }
            }
        }
    }
}
