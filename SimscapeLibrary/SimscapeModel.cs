using System;
using System.Collections.Generic;
using SimscapeLibrary;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape model containing components, ports, and solver settings.
    /// </summary>
    public class SimscapeModel
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Classification
        public LibraryTypeEnum LibraryType { get; set; }
        public SolverType Solver { get; set; } = SolverType.Auto;

        // Simulation parameters
        public double StartTime { get; set; }
        public double StopTime { get; set; } = 10.0;
        public double MaxStepSize { get; set; } = 1e-3;

        // Domain
        public SimscapeDomain? Domain { get; set; }

        // Components
        public SimscapeComponent? CurrentSimscapeComponent { get; set; }
        public List<SimscapeComponent> Components { get; set; } = [];

        // Ports
        public List<SimscapePort> Ports { get; set; } = [];

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

        public enum SolverType
        {
            Auto,
            FixedStep,
            VariableStep
        }

        #endregion

        #region Constructors

        public SimscapeModel() { }

        public SimscapeModel(string name, LibraryTypeEnum libraryType)
        {
            Name = name;
            LibraryType = libraryType;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a component to the model.
        /// </summary>
        public void AddComponent(SimscapeComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (!Components.Contains(component))
                Components.Add(component);
        }

        /// <summary>
        /// Removes a component from the model.
        /// </summary>
        public bool RemoveComponent(SimscapeComponent component) =>
            Components.Remove(component);

        /// <summary>
        /// Adds a port to the model.
        /// </summary>
        public void AddPort(string name, PortDirection direction)
        {
            Ports.Add(new SimscapePort(name, direction));
        }

        /// <summary>
        /// Finds a component by name.
        /// </summary>
        public SimscapeComponent? FindComponent(string name) =>
            Components.Find(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Validates the model has a name, at least one component, and a valid domain.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            Components.Count > 0 &&
            (Domain?.Validate() ?? true);

        /// <summary>
        /// Resets simulation parameters to defaults.
        /// </summary>
        public void ResetSimulationParameters()
        {
            StartTime = 0.0;
            StopTime = 10.0;
            MaxStepSize = 1e-3;
            Solver = SolverType.Auto;
        }

        #endregion
    }
}
