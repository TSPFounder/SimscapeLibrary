using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape equation defining a physical relationship between variables and parameters.
    /// </summary>
    public class SimscapeEquation
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Equation expression (e.g. "v == R * i", "i == C * v.der")
        public string Expression { get; set; } = string.Empty;
        public string LeftHandSide { get; set; } = string.Empty;
        public string RightHandSide { get; set; } = string.Empty;

        // Classification
        public EquationType Type { get; set; }

        // Referenced symbols
        public List<SimscapeVariable> Variables { get; set; } = [];
        public List<SimscapeParameter> Parameters { get; set; } = [];

        // Conditional support (for mode-switching equations)
        public bool IsConditional { get; set; }
        public string Condition { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Owning element
        public SimscapeElement? OwningElement { get; set; }

        #endregion

        #region Constructors

        public SimscapeEquation() { }

        public SimscapeEquation(string name, string expression, EquationType type = EquationType.Algebraic)
        {
            Name = name;
            Expression = expression;
            Type = type;
            ParseSides(expression);
        }

        /// <summary>
        /// Creates a conditional equation that is only active when the condition holds.
        /// </summary>
        public SimscapeEquation(string name, string expression, string condition)
            : this(name, expression)
        {
            IsConditional = true;
            Condition = condition;
            Type = EquationType.Conditional;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds a variable to this equation.
        /// </summary>
        public void AddVariable(SimscapeVariable variable)
        {
            ArgumentNullException.ThrowIfNull(variable);
            if (!Variables.Contains(variable))
                Variables.Add(variable);
        }

        /// <summary>
        /// Removes a variable reference from this equation.
        /// </summary>
        public bool RemoveVariable(SimscapeVariable variable) =>
            Variables.Remove(variable);

        /// <summary>
        /// Binds a parameter to this equation.
        /// </summary>
        public void AddParameter(SimscapeParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            if (!Parameters.Contains(parameter))
                Parameters.Add(parameter);
        }

        /// <summary>
        /// Removes a parameter reference from this equation.
        /// </summary>
        public bool RemoveParameter(SimscapeParameter parameter) =>
            Parameters.Remove(parameter);

        /// <summary>
        /// Returns the expression with all parameter names replaced by their current values.
        /// </summary>
        public string SubstituteParameters()
        {
            var result = Expression;
            foreach (var param in Parameters)
                result = result.Replace(param.Name, param.Value.ToString("G"));
            return result;
        }

        /// <summary>
        /// Checks whether the expression references a given variable name.
        /// </summary>
        public bool ReferencesVariable(string variableName) =>
            !string.IsNullOrWhiteSpace(variableName) &&
            Expression.Contains(variableName, StringComparison.Ordinal);

        /// <summary>
        /// Checks whether the expression contains a time derivative (e.g. ".der").
        /// </summary>
        public bool ContainsDerivative() =>
            Expression.Contains(".der", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Updates the full expression and re-parses left/right-hand sides.
        /// </summary>
        public void SetExpression(string expression)
        {
            Expression = expression;
            ParseSides(expression);

            // Auto-detect differential type
            if (ContainsDerivative() && Type == EquationType.Algebraic)
                Type = EquationType.Differential;
        }

        /// <summary>
        /// Validates the equation has a name and a non-empty expression.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Expression) &&
            !string.IsNullOrWhiteSpace(LeftHandSide) &&
            !string.IsNullOrWhiteSpace(RightHandSide) &&
            (!IsConditional || !string.IsNullOrWhiteSpace(Condition));

        public override string ToString() =>
            IsConditional
                ? $"{Name}: if ({Condition}) {{ {Expression} }}"
                : $"{Name}: {Expression}";

        /// <summary>
        /// Splits the expression at "==" into left and right-hand sides.
        /// </summary>
        private void ParseSides(string expression)
        {
            var parts = expression.Split("==", 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                LeftHandSide = parts[0];
                RightHandSide = parts[1];
            }
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates an Ohm's law equation: v == R * i.
        /// </summary>
        public static SimscapeEquation OhmsLaw(SimscapeVariable v, SimscapeVariable i, SimscapeParameter r)
        {
            var eq = new SimscapeEquation("OhmsLaw", $"{v.Name} == {r.Name} * {i.Name}");
            eq.AddVariable(v);
            eq.AddVariable(i);
            eq.AddParameter(r);
            return eq;
        }

        /// <summary>
        /// Creates a capacitor equation: i == C * v.der.
        /// </summary>
        public static SimscapeEquation CapacitorLaw(SimscapeVariable i, SimscapeVariable v, SimscapeParameter c)
        {
            var eq = new SimscapeEquation("CapacitorLaw", $"{i.Name} == {c.Name} * {v.Name}.der", EquationType.Differential);
            eq.AddVariable(i);
            eq.AddVariable(v);
            eq.AddParameter(c);
            return eq;
        }

        /// <summary>
        /// Creates an inductor equation: v == L * i.der.
        /// </summary>
        public static SimscapeEquation InductorLaw(SimscapeVariable v, SimscapeVariable i, SimscapeParameter l)
        {
            var eq = new SimscapeEquation("InductorLaw", $"{v.Name} == {l.Name} * {i.Name}.der", EquationType.Differential);
            eq.AddVariable(v);
            eq.AddVariable(i);
            eq.AddParameter(l);
            return eq;
        }

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Classifies the type of a Simscape equation.
    /// </summary>
    public enum EquationType
    {
        Algebraic,
        Differential,
        Conditional,
        Conservation,
        InitialCondition
    }

    #endregion
}