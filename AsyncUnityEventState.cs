using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;

namespace AsyncStateMachine {

    public abstract class AsyncUnityEventState<TState, TEvent> : MonoBehaviour, IState<TState> where TEvent : UnityEvent<TState> {

        public TEvent OnEntering;
        public TEvent OnEntered;
        public TEvent OnExiting;
        public TEvent OnExited;

        public async UniTask OnEnter(TState from) {
            OnEntering.Invoke(from);
            await OnEnterInternal(from);
            OnEntered.Invoke(from);
        }

        public async UniTask OnExit(TState to) {
            OnExiting.Invoke(to);
            await OnExitInternal(to);
            OnExited.Invoke(to);
        }

        protected abstract UniTask OnEnterInternal(TState from);
        protected abstract UniTask OnExitInternal(TState from);
    }

}
