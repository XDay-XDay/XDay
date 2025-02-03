

using System;

namespace XDay
{
    public interface IEventSystem
    {
        static IEventSystem Create()
        {
            return new EventSystem();
        }

        void Register<Event>(object key, Action<Event> action) where Event : struct;
        void Unregister<Event>(object key, Action<Event> action) where Event : struct;
        void Unregister(object key);
        void Broadcast<Event>(Event e, object receiver = null) where Event : struct;
    }
}
