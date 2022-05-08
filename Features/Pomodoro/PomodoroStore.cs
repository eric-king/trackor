using Fluxor;
using System.Timers;

namespace Trackor.Features.Pomodoro
{
    public record PomodoroTickAction();
    public record PomodoroInitializeTimerAction();
    public record PomodoroSetInitializedAction();
    public record PomodoroStartTimerAction();
    public record PomodoroStopTimerAction();
    public record PomodoroSetRunningAction(bool Running);
    public record PomodoroSetFinishedAction();
    public record PomodoroResetAction();

    public record PomodoroState
    {
        public bool Initialized { get; init; }
        public bool Running { get; init; }
        public bool Finished { get; init; }
        public TimeSpan TimeSpan { get; init; }
        public TimeSpan Elapsed { get; init; }
    }

    public class PomodoroFeature : Feature<PomodoroState>
    {
        public override string GetName() => "Pomodoro";

        protected override PomodoroState GetInitialState()
        {
            return new PomodoroState
            {
                Initialized = false,
                Running = false,
                Finished = false,
                TimeSpan = TimeSpan.FromSeconds(5),
                Elapsed = TimeSpan.Zero,
            };
        }
    }

    public static class PomodoroReducers
    {
        [ReducerMethod(typeof(PomodoroSetInitializedAction))]
        public static PomodoroState OnSetInitialized(PomodoroState state)
        {
            return state with
            {
                Initialized = true
            };
        }

        [ReducerMethod(typeof(PomodoroTickAction))]
        public static PomodoroState OnPomodoroTick(PomodoroState state)
        {
            var newTimeSpan = state.TimeSpan.Subtract(TimeSpan.FromSeconds(1));
            var newElapsed = state.Elapsed.Add(TimeSpan.FromSeconds(1));
            return state with
            {
                TimeSpan = newTimeSpan,
                Elapsed = newElapsed
            };
        }

        [ReducerMethod]
        public static PomodoroState OnSetRunning(PomodoroState state, PomodoroSetRunningAction action)
        {
            return state with
            {
                Running = action.Running
            };
        }

        [ReducerMethod(typeof(PomodoroSetFinishedAction))]
        public static PomodoroState OnSetFinished(PomodoroState state)
        {
            return state with
            {
                Running = false,
                Finished = true
            };
        }

        [ReducerMethod]
        public static PomodoroState OnReset(PomodoroState state, PomodoroResetAction action)
        {
            return state with
            {
                TimeSpan = TimeSpan.FromSeconds(5),
                Elapsed = TimeSpan.Zero,
                Running = false,
                Finished = false
            };
        }
    }

    public class PomodoroEffects
    {
        private PomodoroTimerService _timerService;
        private IState<PomodoroState> _state;

        public PomodoroEffects(PomodoroTimerService timerService, IState<PomodoroState> state)
        {
            _timerService = timerService;
            _state = state;
        }

        [EffectMethod(typeof(PomodoroInitializeTimerAction))]
        public async Task OnTimerInitialize(IDispatcher dispatcher)
        {
            // bail if already initialized
            if (_state.Value.Initialized) return;

            await Task.Yield();
            var timer = _timerService.Timer;
            timer.Elapsed += (object source, ElapsedEventArgs e) => dispatcher.Dispatch(new PomodoroTickAction());
            dispatcher.Dispatch(new PomodoroSetInitializedAction());
        }

        [EffectMethod(typeof(PomodoroStartTimerAction))]
        public async Task OnTimerStart(IDispatcher dispatcher)
        {
            await Task.Yield();
            _timerService.Timer.Start();
            dispatcher.Dispatch(new PomodoroSetRunningAction(true));
        }

        [EffectMethod(typeof(PomodoroStopTimerAction))]
        public async Task OnTimerStop(IDispatcher dispatcher)
        {
            await Task.Yield();
            _timerService.Timer.Stop();
            dispatcher.Dispatch(new PomodoroSetRunningAction(false));
        }

        [EffectMethod(typeof(PomodoroTickAction))]
        public async Task OnTimerTick(IDispatcher dispatcher)
        {
            await Task.Yield();

            if (_state.Value.TimeSpan == TimeSpan.Zero)
            {
                _timerService.Timer.Stop();
                dispatcher.Dispatch(new PomodoroSetFinishedAction());
            }
        }

        [EffectMethod(typeof(PomodoroResetAction))]
        public async Task OnReset(IDispatcher dispatcher)
        {
            await Task.Yield();
            _timerService.Timer.Stop();
        }
    }
}
