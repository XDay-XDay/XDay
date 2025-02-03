

namespace XDay
{
    public interface IStateManager
    {
        static IStateManager Create()
        {
            return new StateManager();
        }

        T CreateState<T>() where T : State, new();
        void ChangeState<T>() where T : State, new();
        void PushState<T>() where T : State, new();
        void PopState(int n);
        void Update(float dt);
    }
}
