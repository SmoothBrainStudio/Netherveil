using System.Diagnostics;

namespace StateMachine
{
    public abstract class BaseState<T>
    {
        private T context;
        private StateFactory<T> factory;

        protected T Context => context;
        protected StateFactory<T> Factory => factory;

        private bool isSwitch = false;

        public BaseState(T currentContext, StateFactory<T> currentFactory)
        {
            context = currentContext;
            factory = currentFactory;
        }

        protected abstract void EnterState();
        protected abstract void UpdateState();
        protected abstract void ExitState();
        protected abstract void CheckSwitchStates();

        public void Update()
        {
            CheckSwitchStates();

            if (!isSwitch)
            {
                UpdateState();
            }
                

            isSwitch = false;
        }

        protected virtual void SwitchState(BaseState<T> newState)
        {
            // current state exits state
            ExitState();

            isSwitch = true;

            // new state enters state
            newState.EnterState();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
