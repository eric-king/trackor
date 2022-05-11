using Fluxor;
using SqliteWasmHelper;
using System.Timers;
using Trackor.Features.Database;

namespace Trackor.Features.Pomodoro
{
    public record PomodoroTickAction();
    public record PomodoroInitializeTimerAction();
    public record PomodoroLoadDurationAction();
    public record PomodoroSaveDurationAction(int Duration);
    public record PomodoroSetDurationAction(int Duration);
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
        public int DefaultDurationInMinutes { get; init; }
    }

    public class PomodoroFeature : Feature<PomodoroState>
    {
        public override string GetName() => "Pomodoro";

        protected override PomodoroState GetInitialState()
        {
            int defaultduration = 25;
            return new PomodoroState
            {
                Initialized = false,
                Running = false,
                Finished = false,
                TimeSpan = TimeSpan.FromMinutes(defaultduration),
                Elapsed = TimeSpan.Zero,
                DefaultDurationInMinutes = defaultduration
            };
        }
    }

    public static class PomodoroReducers
    {
        [ReducerMethod]
        public static PomodoroState OnSetDuration(PomodoroState state, PomodoroSetDurationAction action)
        {
            return state with
            {
                DefaultDurationInMinutes = action.Duration,
                TimeSpan = TimeSpan.FromMinutes(action.Duration)
            };
        }

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
                TimeSpan = TimeSpan.FromMinutes(state.DefaultDurationInMinutes),
                Elapsed = TimeSpan.Zero,
                Running = false,
                Finished = false
            };
        }
    }

    public class PomodoroEffects
    {
        private readonly PomodoroTimerService _timerService;
        private readonly IState<PomodoroState> _state;
        private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

        public PomodoroEffects(PomodoroTimerService timerService, IState<PomodoroState> state, ISqliteWasmDbContextFactory<TrackorContext> db)
        {
            _timerService = timerService;
            _state = state;
            _db = db;
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

        [EffectMethod(typeof(PomodoroLoadDurationAction))]
        public async Task OnLoadDuration(IDispatcher dispatcher)
        {
            // bail if already initialized
            if (_state.Value.Initialized) return;

            using var dbContext = await _db.CreateDbContextAsync();
            var appSetting = dbContext.ApplicationSettings.SingleOrDefault(x => x.Key == "PomodoroDuration");
            if (appSetting is null)
            {
                appSetting = new ApplicationSetting { Key = "PomodoroDuration", Value = 25.ToString() };
                dbContext.ApplicationSettings.Add(appSetting);
                dbContext.SaveChanges();
            }
            dispatcher.Dispatch(new PomodoroSetDurationAction(int.Parse(appSetting.Value)));
        }

        [EffectMethod]
        public async Task OnSaveDuration(PomodoroSaveDurationAction action, IDispatcher dispatcher)
        {
            using var dbContext = await _db.CreateDbContextAsync();
            var appSetting = dbContext.ApplicationSettings.Single(x => x.Key == "PomodoroDuration");
            appSetting.Value = action.Duration.ToString();
            dbContext.SaveChanges();
            dispatcher.Dispatch(new PomodoroSetDurationAction(action.Duration));
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
