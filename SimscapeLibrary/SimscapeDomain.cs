using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape physical domain defining Across and Through variables.
    /// </summary>
    public class SimscapeDomain
    {
        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DomainType Type { get; set; }

        // Across / Through variable definitions
        public string AcrossVariableName { get; set; } = string.Empty;
        public string AcrossVariableUnit { get; set; } = string.Empty;
        public string ThroughVariableName { get; set; } = string.Empty;
        public string ThroughVariableUnit { get; set; } = string.Empty;

        // Domain parameters
        public List<DomainParameter> Parameters { get; set; } = [];

        // Components belonging to this domain
        public List<SimscapeComponent> Components { get; set; } = [];

        #region Constructors

        public SimscapeDomain() { }

        public SimscapeDomain(string name, DomainType type)
        {
            Name = name;
            Type = type;
            ApplyDefaultVariables(type);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a component to this domain.
        /// </summary>
        public void AddComponent(SimscapeComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (!Components.Contains(component))
                Components.Add(component);
        }

        /// <summary>
        /// Removes a component from this domain.
        /// </summary>
        public bool RemoveComponent(SimscapeComponent component) =>
            Components.Remove(component);

        /// <summary>
        /// Adds a parameter definition to this domain.
        /// </summary>
        public void AddParameter(string name, string unit, double defaultValue = 0.0)
        {
            Parameters.Add(new DomainParameter
            {
                Name = name,
                Unit = unit,
                DefaultValue = defaultValue
            });
        }

        /// <summary>
        /// Validates that Across and Through variables are defined.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(AcrossVariableName) &&
            !string.IsNullOrWhiteSpace(ThroughVariableName) &&
            !string.IsNullOrWhiteSpace(AcrossVariableUnit) &&
            !string.IsNullOrWhiteSpace(ThroughVariableUnit);

        /// <summary>
        /// Sets default Across/Through variables based on domain type.
        /// </summary>
        private void ApplyDefaultVariables(DomainType type)
        {
            (AcrossVariableName, AcrossVariableUnit, ThroughVariableName, ThroughVariableUnit) = type switch
            {
                DomainType.Electrical       => ("Voltage",      "V",     "Current",    "A"),
                DomainType.MechanicalTranslational => ("Velocity", "m/s", "Force",     "N"),
                DomainType.MechanicalRotational    => ("AngularVelocity", "rad/s", "Torque", "N*m"),
                DomainType.Hydraulic        => ("Pressure",     "Pa",    "FlowRate",   "m^3/s"),
                DomainType.Thermal          => ("Temperature",  "K",     "HeatFlow",   "W"),
                DomainType.Magnetic         => ("MagneticPotential", "A", "MagneticFlux", "Wb"),
                DomainType.Pneumatic        => ("Pressure",     "Pa",    "MassFlowRate","kg/s"),
                _                           => (string.Empty,   string.Empty, string.Empty, string.Empty)
            };
        }

        #endregion
    }

    #region Supporting Types

    public enum DomainType
    {
        Electrical,
        MechanicalTranslational,
        MechanicalRotational,
        Hydraulic,
        Thermal,
        Magnetic,
        Pneumatic
    }

    public class DomainParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double DefaultValue { get; set; }
    }

    #endregion
}
