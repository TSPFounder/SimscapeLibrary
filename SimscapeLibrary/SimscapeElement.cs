using System;
using System.Collections.Generic;
using Simulation;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape block element with ports, parameters, variables, and connections.
    /// </summary>
    public class SimscapeElement
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ElementType Type { get; set; }
        public LibraryTypeEnum LibraryType { get; set; }

        // Domain
        public SimscapeDomain? Domain { get; set; }

        // Ports
        public List<SimscapePort> Ports { get; set; } = [];

        // Parameters and variables
        public List<ElementParameter> Parameters { get; set; } = [];
        public List<ElementVariable> Variables { get; set; } = [];

        // Equations (Simscape component equations as string expressions)
        public List<string> Equations { get; set; } = [];

        // Owning model
        public SimscapeModel? CurrentSimscapeModel { get; set; }

        // Connections to other elements
        public List<SimscapeElement> ConnectedElements { get; set; } = [];

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

        public enum ElementType
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

        public SimscapeElement() { }

        public SimscapeElement(string name, ElementType type)
        {
            Name = name;
            Type = type;
        }

        public SimscapeElement(string name, ElementType type, SimscapeDomain domain)
        {
            Name = name;
            Type = type;
            Domain = domain;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a port to this element.
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
        /// Adds a configurable parameter to this element.
        /// </summary>
        public void AddParameter(string name, string unit, double defaultValue = 0.0)
        {
            Parameters.Add(new ElementParameter
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
            Variables.Add(new ElementVariable
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
        /// Connects this element to another element.
        /// </summary>
        public void Connect(SimscapeElement other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other == this) return;

            if (!ConnectedElements.Contains(other))
                ConnectedElements.Add(other);
            if (!other.ConnectedElements.Contains(this))
                other.ConnectedElements.Add(this);
        }

        /// <summary>
        /// Disconnects this element from another element.
        /// </summary>
        public bool Disconnect(SimscapeElement other)
        {
            ArgumentNullException.ThrowIfNull(other);
            var removed = ConnectedElements.Remove(other);
            other.ConnectedElements.Remove(this);
            return removed;
        }

        /// <summary>
        /// Validates the element has a name, at least one port, and a domain.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            Ports.Count > 0 &&
            Domain is not null;

        #endregion
    }

    #region Supporting Types

    public class ElementParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double DefaultValue { get; set; }
        public double Value { get; set; }
    }

    public class ElementVariable
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double InitialValue { get; set; }
    }

    #endregion
}
