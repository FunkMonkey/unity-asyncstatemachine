using UnityEngine;
using UniRx.Async;
using AsyncStateMachine;

public class SimpleExample : MonoBehaviour
{

    enum State
    {
        STATE_1,
        STATE_2,
        STATE_3,
        STATE_4
    };

    Statemachine<State> statemachine = new Statemachine<State>();

    // State1: using UniTasks
    async UniTask State1_Enter(State from)
    {
        Debug.Log("Entering State 1");
        await UniTask.Delay(2000);
        Debug.Log("Done Entering State 1");
    }

    async UniTask State1_Exit(State to)
    {
        Debug.Log("Exiting State 1");
        await UniTask.Delay(2000);
        Debug.Log("Done Exiting State 1");
    }

    // State2: using synchrounous callback functions
    void State2_Enter(State from)
    {
        Debug.Log("Entering State 2");
        Debug.Log("Entered State 2");
    }

    void State2_Exit(State to)
    {
        Debug.Log("Exiting State 2");
        Debug.Log("Exited State 2");
    }

    // State 3: using IState interface and thus UniTasks
    class State3 : IState<State>
    {
        public async UniTask OnEnter(State from)
        {
            Debug.Log("Entering State 3");
            await UniTask.Delay(2000);
            Debug.Log("Done Entering State 3");
        }

        public async UniTask OnExit(State to)
        {
            Debug.Log("Exiting State 3");
            await UniTask.Delay(2000);
            Debug.Log("Done Exiting State 3");
        }
    }

    // State 4: using ISyncState interface
    class State4 : ISyncState<State>
    {
        public void OnEnter(State from)
        {
            Debug.Log("Entering State 4");
            Debug.Log("Done Entering State 4");
        }

        public void OnExit(State to)
        {
            Debug.Log("Exiting State 4");
            Debug.Log("Done Exiting State 4");
        }
    }

    void Start()
    {
        statemachine.DEBUG_MODE = true;

        // setting up the statemachine
        statemachine.AddStateWithTasks(State.STATE_1, State1_Enter, State1_Exit);
        statemachine.AddStateWithCallbacks(State.STATE_2, State2_Enter, State2_Exit);
        statemachine.AddState(State.STATE_3, new State3());
        statemachine.AddSyncState(State.STATE_4, new State4());

        // setting up optional event callbacks
        statemachine.OnStateEntering += (trans => Debug.Log("Statemachine: Entering state " + trans.To));
        statemachine.OnStateEntered += (trans => Debug.Log("Statemachine: Entered state " + trans.To));
        statemachine.OnStateExiting += (trans => Debug.Log("Statemachine: Exiting state " + trans.From));
        statemachine.OnStateExited += (trans => Debug.Log("Statemachine: Exited state " + trans.From));

        // initial transition, we fire and forget here using a discard _
        _ = statemachine.TransitionToState(State.STATE_1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _ = statemachine.TransitionToState(State.STATE_1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _ = statemachine.TransitionToState(State.STATE_2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _ = statemachine.TransitionToState(State.STATE_3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _ = statemachine.TransitionToState(State.STATE_4);
        }

    }
}
