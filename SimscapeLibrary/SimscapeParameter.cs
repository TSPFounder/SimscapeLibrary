using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a configurable parameter in a Simscape component with units, bounds, and access control.
    /// </summary>
    public class SimscapeParameter
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        // Value
        public double DefaultValue { get; set; }
        public double Value { get; set; }

        // Bounds
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }

        // Classification
        public ParameterDataType DataType { get; set; } = ParameterDataType.Real;
        public ParameterAccess Access { get; set; } = ParameterAccess.Public;
        public ExternalAccess ExternalVisibility { get; set; } = ExternalAccess.Modify;
        public InitializationPriority Priority { get; set; } = InitializationPriority.None;

        // Enumeration support (when DataType is Enumeration)
        public List<string> EnumerationValues { get; set; } = [];
        public int EnumerationIndex { get; set; }

        #endregion

        #region Constructors

        public SimscapeParameter() { }

        public SimscapeParameter(string name, string unit, double defaultValue)
        {
            Name = name;
            Unit = unit;
            DefaultValue = defaultValue;
            Value = defaultValue;
        }

        public SimscapeParameter(string name, string unit, double defaultValue, double min, double max)
            : this(name, unit, defaultValue)
        {
            MinValue = min;
            MaxValue = max;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the value, clamping to bounds if defined.
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
        /// Sets the value, clamping to [Min, Max] instead of rejecting out-of-range values.
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
        /// Sets the bounds for this parameter.
        /// </summary>
        public void SetBounds(double min, double max)
        {
            if (min > max)
                throw new ArgumentException("Min must be less than or equal to Max.");

            MinValue = min;
            MaxValue = max;
        }

        /// <summary>
        /// Clears any defined bounds.
        /// </summary>
        public void ClearBounds()
        {
            MinValue = null;
            MaxValue = null;
        }

        /// <summary>
        /// Resets the value to the default.
        /// </summary>
        public void Reset() => Value = DefaultValue;

        /// <summary>
        /// Returns true if the current value is within [Min, Max] (or unbounded).
        /// </summary>
        public bool IsWithinBounds() =>
            (!MinValue.HasValue || Value >= MinValue.Value) &&
            (!MaxValue.HasValue || Value <= MaxValue.Value);

        /// <summary>
        /// Validates the parameter has a name, unit, and a value within bounds.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Unit) &&
            IsWithinBounds();

        public override string ToString() =>
            $"{Name} = {Value} {Unit}" +
            (MinValue.HasValue || MaxValue.HasValue
                ? $" [{MinValue?.ToString() ?? "-∞"}, {MaxValue?.ToString() ?? "∞"}]"
                : "");

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Data type of a Simscape parameter value.
    /// </summary>
    public enum ParameterDataType
    {
        Real,
        Integer,
        Boolean,
        Enumeration
    }

    /// <summary>
    /// Access level of a parameter within a Simscape component.
    /// </summary>
    public enum ParameterAccess
    {
        Public,
        Private
    }

    /// <summary>
    /// Controls how a parameter is exposed to the model workspace.
    /// </summary>
    public enum ExternalAccess
    {
        Modify,
        Observe,
        None
    }

    /// <summary>
    /// Initialization priority for variable/parameter initial conditions.
    /// </summary>
    public enum InitializationPriority
    {
        None,
        Low,
        High
    }

    #endregion
}
