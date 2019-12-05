using AsyncStateMachine;
using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SampleStateEvent: UnityEvent<SimpleExample.State> { }

public class SampleAsyncEventState : AsyncUnityEventState<SimpleExample.State, SampleStateEvent>
{
    protected override async UniTask OnEnterInternal(SimpleExample.State from)
    {
        Debug.Log("Entering SampleAsyncEventState");
        await UniTask.Delay(2000);
        Debug.Log("Done Entering SampleAsyncEventState");
    }

    protected override async UniTask OnExitInternal(SimpleExample.State from)
    {
        Debug.Log("Exiting SampleAsyncEventState");
        await UniTask.Delay(2000);
        Debug.Log("Done Exiting SampleAsyncEventState");
    }
}
