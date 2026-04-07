using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation
{
    /// <summary>
    /// Represents a reusable Simscape component definition that encapsulates
    /// ports, parameters, variables, equations, and functions into an instantiable unit.
    /// </summary>
    public class SimscapeComponent
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string LibraryPath { get; set; } = string.Empty;

        // Domain
        public SimscapeDomain? Domain { get; set; }

        // Ports
        public List<SimscapePort> Ports { get; set; } = [];

        // Nodes and branches (internal topology)
        public List<SimscapeNode> Nodes { get; set; } = [];
        public List<SimscapeBranch> Branches { get; set; } = [];

        // Parameters (user-configurable constants)
        public List<SimscapeParameter> Parameters { get; set; } = [];

        // Variables (Across, Through, internal state)
        public List<SimscapeVariable> Variables { get; set; } = [];

        // Equations (physics relationships)
        public List<SimscapeEquation> Equations { get; set; } = [];

        // Functions (helper computations)
        public List<SimscapeFunction> Functions { get; set; } = [];

        // Initialization
        public List<SimscapeEquation> InitialEquations { get; set; } = [];

        // Metadata
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        #endregion

        #region Constructors

        public SimscapeComponent() { }

        public SimscapeComponent(string name, SimscapeDomain domain)
        {
            Name = name;
            Domain = domain;
        }

        #endregion

        #region Port Methods

        /// <summary>
        /// Adds a physical signal port.
        /// </summary>
        public void AddPort(string name, PortDirection direction)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Ports.Add(new SimscapePort(name, direction));
        }

        /// <summary>
        /// Adds a conserving port tied to the component's domain.
        /// </summary>
        public void AddConservingPort(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (Domain is null)
                throw new InvalidOperationException("Domain must be set before adding conserving ports.");
            Ports.Add(new SimscapePort(name, Domain.Type));
        }

        /// <summary>
        /// Removes a port by name.
        /// </summary>
        public bool RemovePort(string name) =>
            Ports.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a port by name.
        /// </summary>
        public SimscapePort? FindPort(string name) =>
            Ports.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Node & Branch Methods

        /// <summary>
        /// Adds an internal node to the component topology.
        /// </summary>
        public SimscapeNode AddNode(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var domainType = Domain?.Type ?? DomainType.Electrical;
            var node = new SimscapeNode(name, domainType);
            Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Adds a branch between two nodes.
        /// </summary>
        public SimscapeBranch AddBranch(string name, SimscapeNode from, SimscapeNode to)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            var domainType = Domain?.Type ?? DomainType.Electrical;
            var branch = new SimscapeBranch(name, domainType, from, to);
            Branches.Add(branch);
            return branch;
        }

        /// <summary>
        /// Finds a node by name.
        /// </summary>
        public SimscapeNode? FindNode(string name) =>
            Nodes.Find(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Finds a branch by name.
        /// </summary>
        public SimscapeBranch? FindBranch(string name) =>
            Branches.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Parameter & Variable Methods

        /// <summary>
        /// Declares a parameter with optional bounds.
        /// </summary>
        public SimscapeParameter AddParameter(string name, string unit, double defaultValue,
            double? min = null, double? max = null)
        {
            var param = min.HasValue && max.HasValue
                ? new SimscapeParameter(name, unit, defaultValue, min.Value, max.Value)
                : new SimscapeParameter(name, unit, defaultValue);
            Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// Removes a parameter by name.
        /// </summary>
        public bool RemoveParameter(string name) =>
            Parameters.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a parameter by name.
        /// </summary>
        public SimscapeParameter? FindParameter(string name) =>
            Parameters.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Declares a variable (Across, Through, or internal).
        /// </summary>
        public SimscapeVariable AddVariable(string name, string unit, VariableKind kind,
            double initialValue = 0.0)
        {
            var variable = new SimscapeVariable(name, unit, kind, initialValue);
            Variables.Add(variable);
            return variable;
        }

        /// <summary>
        /// Removes a variable by name.
        /// </summary>
        public bool RemoveVariable(string name) =>
            Variables.RemoveAll(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a variable by name.
        /// </summary>
        public SimscapeVariable? FindVariable(string name) =>
            Variables.Find(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Equation & Function Methods

        /// <summary>
        /// Adds an equation to the component.
        /// </summary>
        public SimscapeEquation AddEquation(string name, string expression,
            EquationType type = EquationType.Algebraic)
        {
            var eq = new SimscapeEquation(name, expression, type) { OwningElement = null };
            Equations.Add(eq);
            return eq;
        }

        /// <summary>
        /// Adds an initial condition equation.
        /// </summary>
        public SimscapeEquation AddInitialEquation(string name, string expression)
        {
            var eq = new SimscapeEquation(name, expression, EquationType.InitialCondition);
            InitialEquations.Add(eq);
            return eq;
        }

        /// <summary>
        /// Removes an equation by name.
        /// </summary>
        public bool RemoveEquation(string name) =>
            Equations.RemoveAll(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds an equation by name.
        /// </summary>
        public SimscapeEquation? FindEquation(string name) =>
            Equations.Find(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a helper function to the component.
        /// </summary>
        public void AddFunction(SimscapeFunction function)
        {
            ArgumentNullException.ThrowIfNull(function);
            Functions.Add(function);
        }

        /// <summary>
        /// Removes a function by name.
        /// </summary>
        public bool RemoveFunction(string name) =>
            Functions.RemoveAll(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a function by name.
        /// </summary>
        public SimscapeFunction? FindFunction(string name) =>
            Functions.Find(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Instantiation & Validation

        /// <summary>
        /// Creates a <see cref="SimscapeElement"/> instance from this component definition.
        /// </summary>
        public SimscapeElement Instantiate(string instanceName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);

            var element = new SimscapeElement(instanceName, SimscapeElement.ElementType.Custom)
            {
                Domain = Domain
            };

            // Copy ports
            foreach (var port in Ports)
                element.AddPort(port.Name, port.Direction);

            // Copy parameters with default values
            foreach (var param in Parameters)
                element.AddParameter(param.Name, param.Unit, param.DefaultValue);

            // Copy equations as string expressions
            foreach (var eq in Equations)
                element.AddEquation(eq.Expression);

            return element;
        }

        /// <summary>
        /// Resets all parameter values to their defaults.
        /// </summary>
        public void ResetParameters()
        {
            foreach (var param in Parameters)
                param.Reset();
        }

        /// <summary>
        /// Resets all variable values to their initial conditions.
        /// </summary>
        public void ResetVariables()
        {
            foreach (var variable in Variables)
                variable.Reset();
        }

        /// <summary>
        /// Updates the modified timestamp.
        /// </summary>
        public void Touch() => ModifiedDate = DateTime.UtcNow;

        /// <summary>
        /// Validates the component has a name, domain, ports, and at least one equation.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            Domain is not null &&
            Ports.Count > 0 &&
            Equations.Count > 0 &&
            Parameters.All(p => p.Validate()) &&
            Variables.All(v => v.Validate()) &&
            Equations.All(e => e.Validate());

        public override string ToString() =>
            $"{Name} ({Domain?.Type}, {Ports.Count} ports, {Equations.Count} equations)";

        #endregion
    }
}
