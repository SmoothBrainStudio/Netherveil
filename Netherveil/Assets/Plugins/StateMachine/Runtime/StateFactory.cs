using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class StateFactory<T>
    {
        private Dictionary<Type, Func<BaseState<T>>> stateFactories = new Dictionary<Type, Func<BaseState<T>>>();

        public StateFactory(T context)
        {
            Type[] allValidTypes = typeof(T).Assembly.GetTypes();
            foreach (Type type in allValidTypes)
            {
                if (typeof(BaseState<T>).IsAssignableFrom(type) && !type.IsAbstract && type != typeof(BaseState<T>))
                {
                    stateFactories[type] = () => (BaseState<T>)Activator.CreateInstance(type, context, this);
                }
            }
        }

        public BaseState<T> GetState<U>()
        {
            return GetState(typeof(U));
        }

        public BaseState<T> GetState(Type stateType)
        {
            if (stateFactories.ContainsKey(stateType))
            {
                return stateFactories[stateType]();
            }
            else
            {
                throw new ArgumentException($"{stateType.Name} is not a valid state Type.");
            }
        }
    }
}