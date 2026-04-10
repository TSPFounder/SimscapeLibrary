using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape block component with ports, parameters, variables, and connections.
    /// </summary>
    public class SimscapeComponent
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComponentType Type { get; set; }
        public LibraryTypeEnum LibraryType { get; set; }

        // Domain
        public SimscapeDomain? Domain { get; set; }

        // Ports
        public List<SimscapePort> Ports { get; set; } = [];

        // Parameters and variables
        public List<ComponentParameter> Parameters { get; set; } = [];
        public List<ComponentVariable> Variables { get; set; } = [];

        // Equations (Simscape component equations as string expressions)
        public List<string> Equations { get; set; } = [];

        // Owning model
        public SimscapeModel? CurrentSimscapeModel { get; set; }

        // Connections to other components
        public List<SimscapeComponent> ConnectedComponents { get; set; } = [];

        #endregion

        #region Enumerations

        public enum LibraryTypeEnum
        {
            General = 0,
            Battery,
            Driveline,
            Electrical,
            Fluids,
            Multibody
        }

        public enum ComponentType
        {
            Source,
            Sensor,
            Passive,
            Transformer,
            Converter,
            Actuator,
            Reference,
            Custom
        }

        #endregion

        #region Constructors

        public SimscapeComponent() { }

        public SimscapeComponent(string name, ComponentType type)
        {
            Name = name;
            Type = type;
        }

        public SimscapeComponent(string name, ComponentType type, SimscapeDomain domain)
        {
            Name = name;
            Type = type;
            Domain = domain;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a port to this component.
        /// </summary>
        public void AddPort(string name, PortDirection direction)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Ports.Add(new SimscapePort { Name = name, Direction = direction });
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

        /// <summary>
        /// Adds a configurable parameter to this component.
        /// </summary>
        public void AddParameter(string name, string unit, double defaultValue = 0.0)
        {
            Parameters.Add(new ComponentParameter
            {
                Name = name,
                Unit = unit,
                DefaultValue = defaultValue,
                Value = defaultValue
            });
        }

        /// <summary>
        /// Sets the value of an existing parameter by name.
        /// </summary>
        public bool SetParameterValue(string name, double value)
        {
            var param = Parameters.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (param is null) return false;
            param.Value = value;
            return true;
        }

        /// <summary>
        /// Adds a state variable with an initial condition.
        /// </summary>
        public void AddVariable(string name, string unit, double initialValue = 0.0)
        {
            Variables.Add(new ComponentVariable
            {
                Name = name,
                Unit = unit,
                InitialValue = initialValue
            });
        }

        /// <summary>
        /// Adds an equation expression (e.g. "v == R * i").
        /// </summary>
        public void AddEquation(string equation)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(equation);
            Equations.Add(equation);
        }

        /// <summary>
        /// Connects this component to another component.
        /// </summary>
        public void Connect(SimscapeComponent other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other == this) return;

            if (!ConnectedComponents.Contains(other))
                ConnectedComponents.Add(other);
            if (!other.ConnectedComponents.Contains(this))
                other.ConnectedComponents.Add(this);
        }

        /// <summary>
        /// Disconnects this component from another component.
        /// </summary>
        public bool Disconnect(SimscapeComponent other)
        {
            ArgumentNullException.ThrowIfNull(other);
            var removed = ConnectedComponents.Remove(other);
            other.ConnectedComponents.Remove(this);
            return removed;
        }

        /// <summary>
        /// Validates the component has a name, at least one port, and a domain.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            Ports.Count > 0 &&
            Domain is not null;

        #endregion
    }

    #region Supporting Types

    public class ComponentParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double DefaultValue { get; set; }
        public double Value { get; set; }
    }

    public class ComponentVariable
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double InitialValue { get; set; }
    }

    #endregion
}
