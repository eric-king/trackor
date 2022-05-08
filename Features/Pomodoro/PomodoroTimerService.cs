namespace Trackor.Features.Pomodoro;

public class PomodoroTimerService
{
    public System.Timers.Timer Timer { get; private set; }

    public PomodoroTimerService()
    {
        Timer = new System.Timers.Timer(1000);
    }
}
