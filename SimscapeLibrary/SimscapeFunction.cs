using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation
{
    /// <summary>
    /// Represents a reusable Simscape function used within component equations
    /// (inline expressions, lookup tables, piecewise definitions, or external MATLAB functions).
    /// </summary>
    public class SimscapeFunction
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Classification
        public FunctionType Type { get; set; }

        // Expression (for inline functions, e.g. "x^2 + 2*x + 1")
        public string Expression { get; set; } = string.Empty;

        // Inputs and outputs
        public List<FunctionPort> Inputs { get; set; } = [];
        public List<FunctionPort> Outputs { get; set; } = [];

        // Lookup table data (for FunctionType.LookupTable)
        public LookupTableData? LookupTable { get; set; }

        // Piecewise segments (for FunctionType.Piecewise)
        public List<PiecewiseSegment> PiecewiseSegments { get; set; } = [];

        // External reference (for FunctionType.External)
        public string ExternalFilePath { get; set; } = string.Empty;
        public string ExternalFunctionName { get; set; } = string.Empty;

        // Continuity
        public bool IsContinuous { get; set; } = true;

        #endregion

        #region Constructors

        public SimscapeFunction() { }

        /// <summary>
        /// Creates an inline function with the given expression.
        /// </summary>
        public SimscapeFunction(string name, string expression)
        {
            Name = name;
            Expression = expression;
            Type = FunctionType.Inline;
        }

        /// <summary>
        /// Creates a function of the given type.
        /// </summary>
        public SimscapeFunction(string name, FunctionType type)
        {
            Name = name;
            Type = type;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an input port to the function.
        /// </summary>
        public void AddInput(string name, string unit = "")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Inputs.Add(new FunctionPort { Name = name, Unit = unit });
        }

        /// <summary>
        /// Adds an output port to the function.
        /// </summary>
        public void AddOutput(string name, string unit = "")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Outputs.Add(new FunctionPort { Name = name, Unit = unit });
        }

        /// <summary>
        /// Removes an input by name.
        /// </summary>
        public bool RemoveInput(string name) =>
            Inputs.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Removes an output by name.
        /// </summary>
        public bool RemoveOutput(string name) =>
            Outputs.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Configures a 1-D lookup table from breakpoint and value arrays.
        /// </summary>
        public void SetLookupTable1D(double[] breakpoints, double[] values, InterpolationMethod method = InterpolationMethod.Linear)
        {
            ArgumentNullException.ThrowIfNull(breakpoints);
            ArgumentNullException.ThrowIfNull(values);
            if (breakpoints.Length != values.Length)
                throw new ArgumentException("Breakpoints and values arrays must have the same length.");

            Type = FunctionType.LookupTable;
            LookupTable = new LookupTableData
            {
                Breakpoints1 = [.. breakpoints],
                Values = [.. values],
                Interpolation = method
            };
        }

        /// <summary>
        /// Configures a 2-D lookup table from two breakpoint axes and a value grid.
        /// </summary>
        public void SetLookupTable2D(double[] breakpoints1, double[] breakpoints2, double[,] values, InterpolationMethod method = InterpolationMethod.Linear)
        {
            ArgumentNullException.ThrowIfNull(breakpoints1);
            ArgumentNullException.ThrowIfNull(breakpoints2);
            ArgumentNullException.ThrowIfNull(values);
            if (values.GetLength(0) != breakpoints1.Length || values.GetLength(1) != breakpoints2.Length)
                throw new ArgumentException("Values grid dimensions must match breakpoint array lengths.");

            Type = FunctionType.LookupTable;
            LookupTable = new LookupTableData
            {
                Breakpoints1 = [.. breakpoints1],
                Breakpoints2 = [.. breakpoints2],
                Values2D = values,
                Interpolation = method
            };
        }

        /// <summary>
        /// Adds a piecewise segment with a condition and expression.
        /// </summary>
        public void AddPiecewiseSegment(string condition, string expression)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(expression);
            Type = FunctionType.Piecewise;
            PiecewiseSegments.Add(new PiecewiseSegment
            {
                Condition = condition,
                Expression = expression
            });
        }

        /// <summary>
        /// Sets the external MATLAB function reference.
        /// </summary>
        public void SetExternalFunction(string filePath, string functionName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
            Type = FunctionType.External;
            ExternalFilePath = filePath;
            ExternalFunctionName = functionName;
        }

        /// <summary>
        /// Evaluates a 1-D lookup table using linear interpolation for the given input.
        /// Returns null if the lookup table is not configured.
        /// </summary>
        public double? EvaluateLookup1D(double input)
        {
            if (LookupTable is null || LookupTable.Breakpoints1.Count == 0 || LookupTable.Values.Count == 0)
                return null;

            var bp = LookupTable.Breakpoints1;
            var vals = LookupTable.Values;

            // Clamp to table bounds
            if (input <= bp[0]) return vals[0];
            if (input >= bp[^1]) return vals[^1];

            // Find bracketing interval
            for (int i = 0; i < bp.Count - 1; i++)
            {
                if (input >= bp[i] && input <= bp[i + 1])
                {
                    double fraction = (input - bp[i]) / (bp[i + 1] - bp[i]);
                    return vals[i] + fraction * (vals[i + 1] - vals[i]);
                }
            }
            return null;
        }

        /// <summary>
        /// Generates a Simscape-style function signature string.
        /// </summary>
        public string ToSignature()
        {
            var inputs = string.Join(", ", Inputs.Select(p => p.Name));
            var outputs = string.Join(", ", Outputs.Select(p => p.Name));
            return $"[{outputs}] = {Name}({inputs})";
        }

        /// <summary>
        /// Validates the function based on its type.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            Type switch
            {
                FunctionType.Inline => !string.IsNullOrWhiteSpace(Expression),
                FunctionType.LookupTable => LookupTable is not null && LookupTable.Breakpoints1.Count > 0,
                FunctionType.Piecewise => PiecewiseSegments.Count > 0,
                FunctionType.External => !string.IsNullOrWhiteSpace(ExternalFunctionName),
                _ => false
            };

        public override string ToString() => $"{Name} ({Type})";

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a 1-D lookup table function.
        /// </summary>
        public static SimscapeFunction CreateLookup1D(string name, double[] breakpoints, double[] values)
        {
            var fn = new SimscapeFunction(name, FunctionType.LookupTable);
            fn.SetLookupTable1D(breakpoints, values);
            fn.AddInput("x");
            fn.AddOutput("y");
            return fn;
        }

        /// <summary>
        /// Creates a simple inline function.
        /// </summary>
        public static SimscapeFunction CreateInline(string name, string expression, string inputName = "x", string outputName = "y")
        {
            var fn = new SimscapeFunction(name, expression);
            fn.AddInput(inputName);
            fn.AddOutput(outputName);
            return fn;
        }

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Classifies how a Simscape function is evaluated.
    /// </summary>
    public enum FunctionType
    {
        Inline,
        LookupTable,
        Piecewise,
        External
    }

    /// <summary>
    /// Interpolation method for lookup table evaluation.
    /// </summary>
    public enum InterpolationMethod
    {
        Linear,
        Nearest,
        Cubic,
        Spline
    }

    /// <summary>
    /// An input or output port of a Simscape function.
    /// </summary>
    public class FunctionPort
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public override string ToString() =>
            string.IsNullOrWhiteSpace(Unit) ? Name : $"{Name} ({Unit})";
    }

    /// <summary>
    /// Stores breakpoint and value data for 1-D or 2-D lookup tables.
    /// </summary>
    public class LookupTableData
    {
        public List<double> Breakpoints1 { get; set; } = [];
        public List<double> Breakpoints2 { get; set; } = [];   // 2-D only
        public List<double> Values { get; set; } = [];          // 1-D
        public double[,]? Values2D { get; set; }                // 2-D
        public InterpolationMethod Interpolation { get; set; } = InterpolationMethod.Linear;
    }

    /// <summary>
    /// A single segment of a piecewise function definition.
    /// </summary>
    public class PiecewiseSegment
    {
        public string Condition { get; set; } = string.Empty;   // e.g. "x >= 0 && x < 1"
        public string Expression { get; set; } = string.Empty;  // e.g. "2*x + 1"

        public override string ToString() =>
            string.IsNullOrWhiteSpace(Condition)
                ? $"otherwise: {Expression}"
                : $"if ({Condition}): {Expression}";
    }

    #endregion
}
