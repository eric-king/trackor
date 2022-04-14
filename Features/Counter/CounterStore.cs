using Fluxor;

namespace Trackor.Features.Counter;

public record CounterIncrementAction();

public record CounterState 
{
    public int ClickCount { get; init; }
}

public class CounterFeature : Feature<CounterState>
{
    public override string GetName() => "Counter";

    protected override CounterState GetInitialState()
    {
        return new CounterState
        {
            ClickCount = 0
        };
    }
}

public static class CounterReducers
{
    [ReducerMethod(typeof(CounterIncrementAction))]
    public static CounterState ReduceIncrementCounterAction(CounterState state) => state with { ClickCount = state.ClickCount + 1 };
}
