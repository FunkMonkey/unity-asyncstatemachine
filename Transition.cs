namespace AsyncStateMachine {
    public enum TransitionPhase {
        NOT_STARTED,
        EXITING_FROM,
        EXITED_FROM,
        ENTERING_TO,
        ENTERED_TO,
        FINISHED
    }

    public class Transition<T> {
        public T From;
        public T To;
        public TransitionPhase Phase = TransitionPhase.NOT_STARTED;

        public Transition(T from, T to) {
            From = from;
            To = to;
        }
    }

}
