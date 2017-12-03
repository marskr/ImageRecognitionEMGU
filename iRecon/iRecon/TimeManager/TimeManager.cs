/*
ITimerImplementor(Implementor) - definiuje interfejs dla klas z implementacją. Nie musi dokładnie odpowiadać interfejsowi 
         klasy Abstraction (różnice między nimi mogą być dość znaczne). Zwykle interfejs klasy Implementator udostępnia 
         jedynie operacje, a w klasie Abstraction zdefiniowane są oparte na nich operacje wyższego poziomu.
TimerAbstraction(Abstraction) - definiuje interfejs abstrakcji, przechowuje referencję do obiektu typu Implementor. 
TimerRefinedAbstraction(RefinedAbstraction) - wzbogacona abstrakcja, rozszerza interfejs zdefiniowany w klasie Abstraction.
TimerNonFractionalConcrete & TimerFractionalConcrete(ConcreteImplementor) - obejmuje implementację interfejsu klasy 
         Implementor i definiuje jej implementację konkretną.
 */

using System;
using System.Diagnostics;

namespace iRecon.TimeManager
{
    // Abstract logic class with reference to the Bridge
    public abstract class TimerAbstraction
    {
        public ITimerImplementor _iTimer;
        public abstract void MeasureStart();
        public abstract void MeasureStop();
        public abstract void MeasureRestart();
        public abstract TimeSpan MeasureResult();
        public abstract string Prompt();
    }
    public class TimerRefinedAbstraction : TimerAbstraction
    {
        public TimerRefinedAbstraction()
        {
            sw = new Stopwatch();
        }
        public Stopwatch sw;
        public override void MeasureStart()
        {
            // use the bridge to start measure
            _iTimer.MeasureStartImp(sw);
        }
        public override void MeasureStop()
        {
            // use the bridge to stop measure
            _iTimer.MeasureStopImp(sw);
        }
        public override void MeasureRestart()
        {
            _iTimer.MeasureRestartImp(sw);
        }
        public override TimeSpan MeasureResult()
        {
            return _iTimer.MeasureResultImp(sw);
        }
        public override string Prompt()
        {
            return _iTimer.PromptImp(sw);
        }
    }
    // Abstract Bridge component
    public interface ITimerImplementor
    {
        void MeasureStartImp(Stopwatch sw);
        void MeasureStopImp(Stopwatch sw);
        void MeasureRestartImp(Stopwatch sw);
        TimeSpan MeasureResultImp(Stopwatch sw);
        string PromptImp(Stopwatch sw);
    }
    public class TimerNonFractional : ITimerImplementor
    {
        public void MeasureStartImp(Stopwatch sw)
        {
            sw.Start();
        }
        public void MeasureStopImp(Stopwatch sw)
        {
            sw.Stop();
        }
        public void MeasureRestartImp(Stopwatch sw)
        {
            sw.Restart();
        }
        public TimeSpan MeasureResultImp(Stopwatch sw)
        {
            return sw.Elapsed;
        }
        public string PromptImp(Stopwatch sw)
        {
            return "Non-fractional:\n" + $"Days: {sw.Elapsed.Days} Hours: {sw.Elapsed.Hours} Minutes: {sw.Elapsed.Minutes} " +
                    $"Seconds: {sw.Elapsed.Seconds} Miliseconds: {sw.Elapsed.Milliseconds} Ticks: {sw.Elapsed.Ticks}\n";
        }
    }
    public class TimerFractional : ITimerImplementor
    {
        public void MeasureStartImp(Stopwatch sw)
        {
            sw.Start();
        }
        public void MeasureStopImp(Stopwatch sw)
        {
            sw.Stop();
        }
        public void MeasureRestartImp(Stopwatch sw)
        {
            sw.Restart();
        }
        public TimeSpan MeasureResultImp(Stopwatch sw)
        {
            return sw.Elapsed;
        }
        public string PromptImp(Stopwatch sw)
        {
            return "Fractional:\n" + $"Days: {sw.Elapsed.TotalDays} Hours: {sw.Elapsed.TotalHours} Minutes: {sw.Elapsed.TotalMinutes} " +
                    $"Seconds: {sw.Elapsed.TotalSeconds} Miliseconds: {sw.Elapsed.TotalMilliseconds}\n";
        }
    }
}
