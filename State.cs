//using UniRx;

using UniRx.Async;

namespace AsyncStateMachine {

    public interface IState<T> {
        UniTask OnEnter(T from);
        UniTask OnExit(T to);
    }

    public interface ISyncState<T> {
        void OnEnter(T from);
        void OnExit(T to);
    }

    public class State<T> : IState<T> {
        public T ID;

        protected OnAsyncEnterCallback<T> OnEnterCB;
        protected OnAsyncExitCallback<T> OnExitCB;

        public State(T id, OnAsyncEnterCallback<T> onEnter, OnAsyncExitCallback<T> onExit) {
            ID = id;
            OnEnterCB = onEnter;
            OnExitCB = onExit;
        }

        public UniTask OnEnter(T from) {
            return OnEnterCB(from);
        }

        public UniTask OnExit(T to) {
            return OnExitCB(to);
        }
    }

}
