using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a time-dependent variable in a Simscape component (Across, Through, or internal state).
    /// </summary>
    public class SimscapeVariable
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        // Classification
        public VariableKind Kind { get; set; }
        public InitializationPriority Priority { get; set; } = InitializationPriority.None;

        // Values
        public double Value { get; set; }
        public double InitialValue { get; set; }
        public double DefaultInitialValue { get; set; }

        // Time derivative (for state variables with dynamics)
        public double DerivativeValue { get; set; }
        public bool HasDerivative { get; set; }

        // Bounds
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }

        // History (sampled values over time)
        public List<TimeSample> History { get; set; } = [];

        #endregion

        #region Constructors

        public SimscapeVariable() { }

        public SimscapeVariable(string name, string unit, VariableKind kind, double initialValue = 0.0)
        {
            Name = name;
            Unit = unit;
            Kind = kind;
            InitialValue = initialValue;
            DefaultInitialValue = initialValue;
            Value = initialValue;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the current value. Returns false if out of bounds.
        /// </summary>
        public bool SetValue(double value)
        {
            if (MinValue.HasValue && value < MinValue.Value)
                return false;
            if (MaxValue.HasValue && value > MaxValue.Value)
                return false;

            Value = value;
            return true;
        }

        /// <summary>
        /// Sets the value, clamping to [Min, Max].
        /// </summary>
        public void SetValueClamped(double value)
        {
            if (MinValue.HasValue && value < MinValue.Value)
                value = MinValue.Value;
            if (MaxValue.HasValue && value > MaxValue.Value)
                value = MaxValue.Value;

            Value = value;
        }

        /// <summary>
        /// Sets bounds for the variable.
        /// </summary>
        public void SetBounds(double min, double max)
        {
            if (min > max)
                throw new ArgumentException("Min must be less than or equal to Max.");
            MinValue = min;
            MaxValue = max;
        }

        /// <summary>
        /// Records the current value at the given simulation time.
        /// </summary>
        public void RecordSample(double time)
        {
            History.Add(new TimeSample { Time = time, Value = Value });
        }

        /// <summary>
        /// Returns the recorded value closest to the given time, or null if no history.
        /// </summary>
        public TimeSample? GetSampleAt(double time)
        {
            if (History.Count == 0)
                return null;

            TimeSample closest = History[0];
            double minDelta = Math.Abs(time - closest.Time);

            for (int i = 1; i < History.Count; i++)
            {
                double delta = Math.Abs(time - History[i].Time);
                if (delta < minDelta)
                {
                    minDelta = delta;
                    closest = History[i];
                }
            }
            return closest;
        }

        /// <summary>
        /// Clears all recorded history samples.
        /// </summary>
        public void ClearHistory() => History.Clear();

        /// <summary>
        /// Resets the value and derivative to initial conditions.
        /// </summary>
        public void Reset()
        {
            Value = InitialValue;
            DerivativeValue = 0.0;
        }

        /// <summary>
        /// Resets the initial value back to the default.
        /// </summary>
        public void ResetInitialValue() => InitialValue = DefaultInitialValue;

        /// <summary>
        /// Returns true if the current value is within bounds.
        /// </summary>
        public bool IsWithinBounds() =>
            (!MinValue.HasValue || Value >= MinValue.Value) &&
            (!MaxValue.HasValue || Value <= MaxValue.Value);

        /// <summary>
        /// Validates the variable has a name, unit, and in-bounds value.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Unit) &&
            IsWithinBounds();

        public override string ToString() =>
            $"{Name} = {Value} {Unit} ({Kind}{(HasDerivative ? $", d/dt={DerivativeValue}" : "")})";

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Classifies the role of a variable in a Simscape component.
    /// </summary>
    public enum VariableKind
    {
        Across,
        Through,
        Internal
    }

    /// <summary>
    /// A recorded value at a specific simulation time.
    /// </summary>
    public class TimeSample
    {
        public double Time { get; set; }
        public double Value { get; set; }

        public override string ToString() => $"t={Time}: {Value}";
    }

    #endregion
}
