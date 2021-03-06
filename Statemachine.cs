﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx.Async;

namespace AsyncStateMachine {

    public delegate void StateChange<T>(Transition<T> transition);
    public delegate UniTask OnAsyncEnterCallback<T>(T from);
    public delegate UniTask OnAsyncExitCallback<T>(T to);
    public delegate void OnSyncEnterCallback<T>(T from);
    public delegate void OnSyncExitCallback<T>(T to);

    public delegate void DebugMessage(string msg);

    public enum DEBUG_MODE {
        NONE,
        UNITY_DEBUG_LOG,
        EVENT
    }

    public class Statemachine<T> {

        public DEBUG_MODE DebugMode = DEBUG_MODE.UNITY_DEBUG_LOG;

        public T CurrentState { get; private set; }
        public Transition<T> CurrentTransition { get; private set; }

        protected bool firstTransition = true;

        protected Dictionary<T, IState<T>> states = new Dictionary<T, IState<T>>();

        public event StateChange<T> OnStateEntering;
        public event StateChange<T> OnStateExiting;
        public event StateChange<T> OnStateEntered;
        public event StateChange<T> OnStateExited;

        public event DebugMessage OnDebugMessage;

        public Statemachine() {
        }

        public async UniTask TransitionToState(T state) {
            if (CurrentTransition != null) {
                DebugLog("Warning: Statemachine already transitioning from " + CurrentTransition.From + " to " + CurrentTransition.To);
                return;
            }

            if (!firstTransition && state.Equals(CurrentState)) // depending on the type of T, currentState may already be set when we begin
            {
                DebugLog("Warning: Statemachine is already in state " + state);
                return;
            }

            CurrentTransition = new Transition<T>(CurrentState, state);

            // exiting
            if (!firstTransition) {
                CurrentTransition.Phase = TransitionPhase.EXITING_FROM;
                OnStateExiting?.Invoke(CurrentTransition);
                await states[CurrentTransition.From].OnExit(CurrentTransition.To);
                CurrentTransition.Phase = TransitionPhase.EXITED_FROM;
                OnStateExited?.Invoke(CurrentTransition);
            }

            // entering
            CurrentState = CurrentTransition.To;
            CurrentTransition.Phase = TransitionPhase.ENTERING_TO;
            OnStateEntering?.Invoke(CurrentTransition);
            await states[CurrentTransition.To].OnEnter(CurrentTransition.From);
            CurrentTransition.Phase = TransitionPhase.ENTERED_TO;
            Transition<T> prevTransition = CurrentTransition;
            CurrentTransition = null;
            OnStateEntered?.Invoke(prevTransition);
            firstTransition = false;
        }

        public bool CurrentStateIs(T state, bool includeTransition = false) {
            if (CurrentTransition == null) {
                return state.Equals(CurrentState);
            } else if (includeTransition) {
                if (CurrentTransition.Phase == TransitionPhase.EXITING_FROM)
                    return state.Equals(CurrentTransition.From);
                if (CurrentTransition.Phase == TransitionPhase.ENTERING_TO)
                    return state.Equals(CurrentTransition.To);
            }
            return false;
        }

        public IState<T> AddState(T id, IState<T> state) {
            states.Add(id, state);
            return state;
        }

        public IState<T> AddState(T id, ISyncState<T> syncState) {
            return AddSyncState(id, syncState);
        }

        public IState<T> AddState(T id, OnSyncEnterCallback<T> onEnterCB, OnSyncExitCallback<T> onExitCB) {
            return AddStateWithCallbacks(id, onEnterCB, onExitCB);
        }

        public State<T> AddState(T id, OnAsyncEnterCallback<T> onEnterCB, OnAsyncExitCallback<T> onExitCB) {
            return AddStateWithTasks(id, onEnterCB, onExitCB);
        }

        public IState<T> AddState(T id, OnSyncEnterCallback<T> onEnterCB, OnAsyncExitCallback<T> onExitCB) {
            UniTask OnEnter(T from) { onEnterCB(from); return UniTask.CompletedTask; }

            State<T> state = new State<T>(id, OnEnter, onExitCB);
            AddState(id, state);
            return state;
        }

        public State<T> AddState(T id, OnAsyncEnterCallback<T> onEnterCB, OnSyncExitCallback<T> onExitCB) {
            UniTask OnExit(T to) { onExitCB(to); return UniTask.CompletedTask; }

            State<T> state = new State<T>(id, onEnterCB, OnExit);
            AddState(id, state);
            return state;
        }

        public IState<T> AddSyncState(T id, ISyncState<T> syncState) {
            UniTask OnEnter(T from) { syncState.OnEnter(from); return UniTask.CompletedTask; }
            UniTask OnExit(T to) { syncState.OnExit(to); return UniTask.CompletedTask; }

            State<T> state = new State<T>(id, OnEnter, OnExit);
            AddState(id, state);
            return state;
        }

        public IState<T> AddStateWithCallbacks(T id, OnSyncEnterCallback<T> onEnterCB, OnSyncExitCallback<T> onExitCB) {
            UniTask OnEnter(T from) { onEnterCB(from); return UniTask.CompletedTask; }
            UniTask OnExit(T to) { onExitCB(to); return UniTask.CompletedTask; }

            State<T> state = new State<T>(id, OnEnter, OnExit);
            AddState(id, state);
            return state;
        }

        public State<T> AddStateWithTasks(T id, OnAsyncEnterCallback<T> onEnterCB, OnAsyncExitCallback<T> onExitCB) {
            State<T> state = new State<T>(id, onEnterCB, onExitCB);
            AddState(id, state);
            return state;
        }

        public void RemoveState(T id) {
            if (!states.ContainsKey(id))
                throw new Exception("StateMachine does not contain the state " + id);

            // TODO: how to remove state that is currently active or in transition?
            IState<T> state = states[id];
            states.Remove(id);
        }

        public void RemoveAllStates() {
            states.Clear();
        }

        public bool IsInTransition() {
            return CurrentTransition != null;
        }

        protected void DebugLog(string msg) {
            switch (DebugMode) {
                case DEBUG_MODE.UNITY_DEBUG_LOG:
                    Debug.Log(msg);
                    break;
                case DEBUG_MODE.EVENT:
                    OnDebugMessage?.Invoke(msg);
                    break;

            }
        }
    }
}
