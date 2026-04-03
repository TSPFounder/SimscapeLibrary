using System;
using System.Collections.Generic;
using System.Linq;
using Simulation;
using CommunityMaker.Simscape;

/*  Example Usage:
 * var model = SimscapeModelBuilder.Create("HybridPowertrain")
    .WithVersion("1.0")
    .WithDescription("Hybrid electric vehicle powertrain model")
    .WithDomain(DomainType.MechanicalRotational)
    .WithLibraryType(SimscapeModel.LibraryTypeEnum.Driveline)
    .WithSolver(SimscapeModel.SolverType.VariableStep)
    .WithSimulationTime(0, 30.0)
    .AddElement("Engine", SimscapeElement.ElementType.Source, e =>
    {
        e.AddPort("shaft_out", PortDirection.Output);
        e.AddParameter("MaxTorque", "N*m", 250.0);
    })
    .AddElement("Transmission", SimscapeElement.ElementType.Transformer, e =>
    {
        e.AddPort("shaft_in", PortDirection.Input);
        e.AddPort("shaft_out", PortDirection.Output);
    })
    .Connect("Engine", "Transmission")
    .UseDriveline(dl =>
    {
        dl.SetEngine("ICE", 250, 6000);
        dl.AddGear("1st", GearType.Simple, 3.5);
        dl.AddGear("2nd", GearType.Simple, 2.1);
        dl.AddClutch("MainClutch", ClutchType.Friction, 400);
    })
    .UseElectrical(el =>
    {
        el.AddMachine("TractionMotor", MachineType.PermanentMagnetSynchronous, 75000, 400);
        el.AddPowerSource("Battery", PowerSourceType.Battery, 400);
    })
    .Build(); * */

namespace SimscapeLibrary
{
    /// <summary>
    /// Fluent builder for constructing and validating <see cref="SimscapeModel"/> instances.
    /// Supports all Simscape add-ins through lambda-based configuration.
    /// </summary>
    public class SimscapeModelBuilder
    {
        #region State

        private readonly SimscapeModel _model = new();
        private readonly List<SimscapeAddin> _addins = [];
        private readonly List<BuilderConnection> _deferredConnections = [];
        private readonly List<string> _errors = [];

        #endregion

        #region Static Factory

        /// <summary>
        /// Creates a new builder with the given model name.
        /// </summary>
        public static SimscapeModelBuilder Create(string name) =>
            new SimscapeModelBuilder().WithName(name);

        #endregion

        #region Identification

        /// <summary>
        /// Sets the model name.
        /// </summary>
        public SimscapeModelBuilder WithName(string name)
        {
            _model.Name = name;
            return this;
        }

        /// <summary>
        /// Sets the model version.
        /// </summary>
        public SimscapeModelBuilder WithVersion(string version)
        {
            _model.Version = version;
            return this;
        }

        /// <summary>
        /// Sets the model description.
        /// </summary>
        public SimscapeModelBuilder WithDescription(string description)
        {
            _model.Description = description;
            return this;
        }

        /// <summary>
        /// Sets the model file path.
        /// </summary>
        public SimscapeModelBuilder WithPath(string path)
        {
            _model.Path = path;
            return this;
        }

        #endregion

        #region Domain & Classification

        /// <summary>
        /// Assigns a physical domain by type, using default Across/Through variables.
        /// </summary>
        public SimscapeModelBuilder WithDomain(DomainType type)
        {
            _model.Domain = new SimscapeDomain(type.ToString(), type);
            return this;
        }

        /// <summary>
        /// Assigns a pre-configured domain instance.
        /// </summary>
        public SimscapeModelBuilder WithDomain(SimscapeDomain domain)
        {
            _model.Domain = domain;
            return this;
        }

        /// <summary>
        /// Sets the Simscape library type.
        /// </summary>
        public SimscapeModelBuilder WithLibraryType(SimscapeModel.LibraryTypeEnum libraryType)
        {
            _model.LibraryType = libraryType;
            return this;
        }

        #endregion

        #region Solver & Simulation Parameters

        /// <summary>
        /// Sets the solver type.
        /// </summary>
        public SimscapeModelBuilder WithSolver(SimscapeModel.SolverType solver)
        {
            _model.Solver = solver;
            return this;
        }

        /// <summary>
        /// Sets the simulation start and stop time.
        /// </summary>
        public SimscapeModelBuilder WithSimulationTime(double startTime, double stopTime)
        {
            _model.StartTime = startTime;
            _model.StopTime = stopTime;
            return this;
        }

        /// <summary>
        /// Sets the maximum solver step size.
        /// </summary>
        public SimscapeModelBuilder WithMaxStepSize(double stepSize)
        {
            _model.MaxStepSize = stepSize;
            return this;
        }

        #endregion

        #region Elements

        /// <summary>
        /// Adds an element to the model with optional inline configuration.
        /// </summary>
        public SimscapeModelBuilder AddElement(
            string name,
            SimscapeElement.ElementType type,
            Action<SimscapeElement>? configure = null)
        {
            var element = new SimscapeElement(name, type)
            {
                Domain = _model.Domain,
                CurrentSimscapeModel = _model
            };
            configure?.Invoke(element);
            _model.AddElement(element);
            return this;
        }

        /// <summary>
        /// Adds a pre-built element to the model.
        /// </summary>
        public SimscapeModelBuilder AddElement(SimscapeElement element)
        {
            element.CurrentSimscapeModel = _model;
            _model.AddElement(element);
            return this;
        }

        #endregion

        #region Ports

        /// <summary>
        /// Adds a physical signal port to the model.
        /// </summary>
        public SimscapeModelBuilder AddPort(string name, PortDirection direction)
        {
            _model.Ports.Add(new SimscapePort(name, direction));
            return this;
        }

        /// <summary>
        /// Adds a conserving port tied to a physical domain.
        /// </summary>
        public SimscapeModelBuilder AddConservingPort(string name, DomainType domain)
        {
            _model.Ports.Add(new SimscapePort(name, domain));
            return this;
        }

        #endregion

        #region Connections

        /// <summary>
        /// Connects two elements by name. Connections are resolved at build time.
        /// </summary>
        public SimscapeModelBuilder Connect(string elementNameA, string elementNameB)
        {
            _deferredConnections.Add(new BuilderConnection(elementNameA, elementNameB));
            return this;
        }

        #endregion

        #region Add-in Configuration

        /// <summary>
        /// Registers and configures a generic add-in by type.
        /// </summary>
        public SimscapeModelBuilder UseAddin<T>(Action<T>? configure = null) where T : SimscapeAddin, new()
        {
            var existing = _addins.OfType<T>().FirstOrDefault();
            if (existing is not null)
            {
                configure?.Invoke(existing);
            }
            else
            {
                var addin = new T();
                configure?.Invoke(addin);
                _addins.Add(addin);
            }
            return this;
        }

        /// <summary>
        /// Registers and configures the Multibody add-in.
        /// </summary>
        public SimscapeModelBuilder UseMultibody(Action<MultibodyAddin> configure) =>
            UseAddin(configure);

        /// <summary>
        /// Registers and configures the Driveline add-in.
        /// </summary>
        public SimscapeModelBuilder UseDriveline(Action<DrivelineAddin> configure) =>
            UseAddin(configure);

        /// <summary>
        /// Registers and configures the Electrical add-in.
        /// </summary>
        public SimscapeModelBuilder UseElectrical(Action<ElectricAddin> configure) =>
            UseAddin(configure);

        /// <summary>
        /// Registers and configures the Fluids add-in.
        /// </summary>
        public SimscapeModelBuilder UseFluids(Action<FluidsAddin> configure) =>
            UseAddin(configure);

        /// <summary>
        /// Returns all registered add-ins.
        /// </summary>
        public IReadOnlyList<SimscapeAddin> GetAddins() => _addins.AsReadOnly();

        /// <summary>
        /// Returns a registered add-in by type, or null if not registered.
        /// </summary>
        public T? GetAddin<T>() where T : SimscapeAddin =>
            _addins.OfType<T>().FirstOrDefault();

        #endregion

        #region Build

        /// <summary>
        /// Builds and validates the model. Throws <see cref="InvalidOperationException"/> on validation failure.
        /// </summary>
        public SimscapeModel Build()
        {
            if (!TryBuild(out var model, out var errors))
            {
                throw new InvalidOperationException(
                    $"Model validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
            return model!;
        }

        /// <summary>
        /// Attempts to build the model, returning false with error details on failure.
        /// </summary>
        public bool TryBuild(out SimscapeModel? model, out IReadOnlyList<string> errors)
        {
            _errors.Clear();

            ResolveConnections();
            ValidateModel();
            ValidateAddins();

            if (_errors.Count > 0)
            {
                model = null;
                errors = _errors.AsReadOnly();
                return false;
            }

            model = _model;
            errors = [];
            return true;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Clears all builder state and starts fresh.
        /// </summary>
        public SimscapeModelBuilder Reset()
        {
            _model.Name = string.Empty;
            _model.Version = string.Empty;
            _model.Description = string.Empty;
            _model.Path = string.Empty;
            _model.Domain = null;
            _model.LibraryType = SimscapeModel.LibraryTypeEnum.General;
            _model.Elements.Clear();
            _model.Ports.Clear();
            _model.ResetSimulationParameters();
            _addins.Clear();
            _deferredConnections.Clear();
            _errors.Clear();
            return this;
        }

        #endregion

        #region Validation (private)

        private void ResolveConnections()
        {
            foreach (var conn in _deferredConnections)
            {
                var a = _model.FindElement(conn.ElementNameA);
                var b = _model.FindElement(conn.ElementNameB);

                if (a is null)
                    _errors.Add($"Connection error: element '{conn.ElementNameA}' not found.");
                if (b is null)
                    _errors.Add($"Connection error: element '{conn.ElementNameB}' not found.");

                if (a is not null && b is not null)
                    a.Connect(b);
            }
        }

        private void ValidateModel()
        {
            if (string.IsNullOrWhiteSpace(_model.Name))
                _errors.Add("Model name is required.");

            if (_model.Elements.Count == 0)
                _errors.Add("Model must contain at least one element.");

            if (_model.Domain is not null && !_model.Domain.Validate())
                _errors.Add("Model domain is incomplete (missing Across/Through variable definitions).");

            if (_model.StopTime <= _model.StartTime)
                _errors.Add("StopTime must be greater than StartTime.");

            if (_model.MaxStepSize <= 0)
                _errors.Add("MaxStepSize must be greater than zero.");

            foreach (var element in _model.Elements)
            {
                if (string.IsNullOrWhiteSpace(element.Name))
                    _errors.Add("An element is missing a name.");
            }
        }

        private void ValidateAddins()
        {
            foreach (var addin in _addins)
            {
                if (!addin.Validate())
                    _errors.Add($"Add-in '{addin.DisplayName}' ({addin.Kind}) failed validation.");

                if (!addin.CheckAvailability() && addin.IsInstalled)
                    _errors.Add($"Add-in '{addin.DisplayName}' is installed but not licensed.");
            }
        }

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Represents a deferred element-to-element connection resolved at build time.
    /// </summary>
    internal sealed class BuilderConnection(string elementNameA, string elementNameB)
    {
        public string ElementNameA { get; } = elementNameA;
        public string ElementNameB { get; } = elementNameB;
    }

    #endregion
}
