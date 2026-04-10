using System;
using System.Collections.Generic;
using System.Linq;
using Simulation;
using CommunityMaker.Simscape;

#region Example
/*  Example Usage:
 * var model = SimscapeModelBuilder.Create("HybridPowertrain")
    .WithVersion("1.0")
    .WithDescription("Hybrid electric vehicle powertrain model")
    .WithDomain(DomainType.MechanicalRotational)
    .WithLibraryType(SimscapeModel.LibraryTypeEnum.Driveline)
    .WithSolver(SimscapeModel.SolverType.VariableStep)
    .WithSimulationTime(0, 30.0)
    .AddComponent("Engine", SimscapeComponent.ComponentType.Source, c =>
    {
        c.AddPort("shaft_out", PortDirection.Output);
        c.AddParameter("MaxTorque", "N*m", 250.0);
    })
    .AddComponent("Transmission", SimscapeComponent.ComponentType.Transformer, c =>
    {
        c.AddPort("shaft_in", PortDirection.Input);
        c.AddPort("shaft_out", PortDirection.Output);
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
#endregion

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

        #region Components

        /// <summary>
        /// Adds a component to the model with optional inline configuration.
        /// </summary>
        public SimscapeModelBuilder AddComponent(
            string name,
            SimscapeComponent.ComponentType type,
            Action<SimscapeComponent>? configure = null)
        {
            var component = new SimscapeComponent(name, type)
            {
                Domain = _model.Domain,
                CurrentSimscapeModel = _model
            };
            configure?.Invoke(component);
            _model.AddComponent(component);
            return this;
        }

        /// <summary>
        /// Adds a pre-built component to the model.
        /// </summary>
        public SimscapeModelBuilder AddComponent(SimscapeComponent component)
        {
            component.CurrentSimscapeModel = _model;
            _model.AddComponent(component);
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
        /// Connects two components by name. Connections are resolved at build time.
        /// </summary>
        public SimscapeModelBuilder Connect(string componentNameA, string componentNameB)
        {
            _deferredConnections.Add(new BuilderConnection(componentNameA, componentNameB));
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

        #region File Declarations

        /// <summary>
        /// Associates a <see cref="SimscapeModelFile"/> with the model. Creates one if not already set.
        /// </summary>
        public SimscapeModelBuilder WithModelFile(string fileName, string directoryPath)
        {
            _model.ModelFile = new SimscapeModelFile(fileName, directoryPath);
            return this;
        }

        /// <summary>
        /// Associates a pre-configured <see cref="SimscapeModelFile"/> with the model.
        /// </summary>
        public SimscapeModelBuilder WithModelFile(SimscapeModelFile modelFile)
        {
            _model.ModelFile = modelFile;
            return this;
        }

        /// <summary>
        /// Declares a domain in the model file with default Across/Through variables.
        /// Writes the domain into the file's Domains collection and assigns it to
        /// a reference component registered in the file.
        /// </summary>
        public SimscapeModelBuilder DeclareDomain(DomainType type, Action<SimscapeDomain>? configure = null)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var domain = new SimscapeDomain(type.ToString(), type);
            configure?.Invoke(domain);

            file.AddDomain(domain);

            var refComponent = new SimscapeComponent($"{type}_DomainRef", SimscapeComponent.ComponentType.Reference)
            {
                Domain = domain
            };
            file.AddComponent(refComponent);

            return this;
        }

        /// <summary>
        /// Declares a pre-configured domain in the model file.
        /// Writes the domain into the file's Domains collection and assigns it to
        /// a reference component registered in the file.
        /// </summary>
        public SimscapeModelBuilder DeclareDomain(SimscapeDomain domain)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            file.AddDomain(domain);

            var refComponent = new SimscapeComponent($"{domain.Name}_DomainRef", SimscapeComponent.ComponentType.Reference)
            {
                Domain = domain
            };
            file.AddComponent(refComponent);

            return this;
        }

        /// <summary>
        /// Declares a component in the model file with optional inline configuration.
        /// Writes the component into both the model's component list and the file's
        /// Components collection.
        /// </summary>
        public SimscapeModelBuilder DeclareComponent(
            string name,
            SimscapeComponent.ComponentType type,
            Action<SimscapeComponent>? configure = null)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = new SimscapeComponent(name, type)
            {
                Domain = _model.Domain,
                CurrentSimscapeModel = _model
            };
            configure?.Invoke(component);

            _model.AddComponent(component);
            file.AddComponent(component);

            // If the component's domain isn't yet in the file, add it
            if (component.Domain is not null && file.FindDomain(component.Domain.Name) is null)
                file.AddDomain(component.Domain);

            return this;
        }

        /// <summary>
        /// Declares a pre-built component in the model file.
        /// Writes the component into both the model's component list and the file's
        /// Components collection.
        /// </summary>
        public SimscapeModelBuilder DeclareComponent(SimscapeComponent component)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            component.CurrentSimscapeModel = _model;
            _model.AddComponent(component);
            file.AddComponent(component);

            if (component.Domain is not null && file.FindDomain(component.Domain.Name) is null)
                file.AddDomain(component.Domain);

            return this;
        }

        /// <summary>
        /// Declares a node on a named component in the model file.
        /// Writes the node into the file's Nodes collection and adds a matching
        /// conserving port on the target component.
        /// </summary>
        public SimscapeModelBuilder DeclareNode(
            string componentName,
            string nodeName,
            DomainType? domain = null)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareNode: component '{componentName}' not found in model file.");
                return this;
            }

            var nodeType = domain ?? component.Domain?.Type ?? DomainType.Electrical;
            var node = new SimscapeNode(nodeName, nodeType);

            component.AddPort(nodeName, PortDirection.Conserving);

            var port = component.FindPort(nodeName);
            if (port is not null)
                node.AddPort(port);

            file.AddNode(node);

            return this;
        }

        /// <summary>
        /// Declares a reference (ground) node on a named component in the model file.
        /// Writes the node into the file's Nodes collection with IsReference = true.
        /// </summary>
        public SimscapeModelBuilder DeclareReferenceNode(
            string componentName,
            string nodeName,
            DomainType? domain = null)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareReferenceNode: component '{componentName}' not found in model file.");
                return this;
            }

            var nodeType = domain ?? component.Domain?.Type ?? DomainType.Electrical;
            var node = SimscapeNode.CreateReference(nodeName, nodeType);

            component.AddPort(nodeName, PortDirection.Conserving);

            var port = component.FindPort(nodeName);
            if (port is not null)
                node.AddPort(port);

            file.AddNode(node);

            return this;
        }

        /// <summary>
        /// Declares an equation on a named component in the model file.
        /// Writes a <see cref="SimscapeEquation"/> into the file's Equations collection
        /// and adds the expression string to the component's equation list.
        /// </summary>
        public SimscapeModelBuilder DeclareEquation(
            string componentName,
            string equationName,
            string expression,
            EquationType type = EquationType.Algebraic)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareEquation: component '{componentName}' not found in model file.");
                return this;
            }

            var equation = new SimscapeEquation(equationName, expression, type)
            {
                OwningComponent = component
            };

            component.AddEquation(expression);
            file.AddEquation(equation);

            return this;
        }

        /// <summary>
        /// Declares a conditional equation on a named component in the model file.
        /// Writes a conditional <see cref="SimscapeEquation"/> into the file's Equations collection.
        /// </summary>
        public SimscapeModelBuilder DeclareConditionalEquation(
            string componentName,
            string equationName,
            string expression,
            string condition)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareConditionalEquation: component '{componentName}' not found in model file.");
                return this;
            }

            var equation = new SimscapeEquation(equationName, expression, condition)
            {
                OwningComponent = component
            };

            component.AddEquation($"if ({condition}) {{ {expression} }}");
            file.AddEquation(equation);

            return this;
        }

        /// <summary>
        /// Declares a variable on a named component in the model file.
        /// Writes a <see cref="SimscapeVariable"/> into the file's Variables collection
        /// and adds a matching <see cref="ComponentVariable"/> to the component.
        /// </summary>
        public SimscapeModelBuilder DeclareVariable(
            string componentName,
            string variableName,
            string unit,
            VariableKind kind = VariableKind.Internal,
            double initialValue = 0.0)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareVariable: component '{componentName}' not found in model file.");
                return this;
            }

            var variable = new SimscapeVariable(variableName, unit, kind, initialValue);

            component.AddVariable(variableName, unit, initialValue);
            file.AddVariable(variable);

            return this;
        }

        /// <summary>
        /// Declares a parameter on a named component in the model file.
        /// Writes a <see cref="SimscapeParameter"/> into the file's Parameters collection
        /// and adds a matching <see cref="ComponentParameter"/> to the component.
        /// </summary>
        public SimscapeModelBuilder DeclareParameter(
            string componentName,
            string parameterName,
            string unit,
            double defaultValue = 0.0)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareParameter: component '{componentName}' not found in model file.");
                return this;
            }

            var parameter = new SimscapeParameter(parameterName, unit, defaultValue);

            component.AddParameter(parameterName, unit, defaultValue);
            file.AddParameter(parameter);

            return this;
        }

        /// <summary>
        /// Declares a bounded parameter on a named component in the model file.
        /// Writes a <see cref="SimscapeParameter"/> with min/max bounds into the file's
        /// Parameters collection and adds a matching <see cref="ComponentParameter"/>
        /// to the component.
        /// </summary>
        public SimscapeModelBuilder DeclareParameter(
            string componentName,
            string parameterName,
            string unit,
            double defaultValue,
            double min,
            double max)
        {
            EnsureModelFile();
            var file = _model.ModelFile!;

            var component = FindFileComponent(componentName);
            if (component is null)
            {
                _errors.Add($"DeclareParameter: component '{componentName}' not found in model file.");
                return this;
            }

            var parameter = new SimscapeParameter(parameterName, unit, defaultValue, min, max);

            component.AddParameter(parameterName, unit, defaultValue);
            file.AddParameter(parameter);

            return this;
        }

        /// <summary>
        /// Declares a shared function in the model file.
        /// Writes the function into the file's SharedFunctions collection.
        /// </summary>
        public SimscapeModelBuilder DeclareSharedFunction(SimscapeFunction function)
        {
            EnsureModelFile();
            _model.ModelFile!.AddSharedFunction(function);
            return this;
        }

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
            SyncModelFileContents();
            ValidateModel();
            ValidateModelFile();
            ValidateAddins();

            if (_errors.Count > 0)
            {
                model = null;
                errors = _errors.AsReadOnly();
                return false;
            }

            // Persist declarations to disk if the file has unsaved changes
            if (_model.ModelFile is { IsDirty: true })
            {
                if (!_model.ModelFile.Save())
                    _errors.Add($"Failed to save model file to '{_model.ModelFile.FullPath}'.");
            }

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
            _model.Components.Clear();
            _model.Ports.Clear();
            _model.ModelFile = null;
            _model.ResetSimulationParameters();
            _addins.Clear();
            _deferredConnections.Clear();
            _errors.Clear();
            return this;
        }

        #endregion

        #region Validation (private)

        private void EnsureModelFile()
        {
            _model.ModelFile ??= new SimscapeModelFile(
                $"{(_model.Name.Length > 0 ? _model.Name : "Untitled")}.ssc",
                _model.Path.Length > 0 ? _model.Path : Environment.CurrentDirectory);
        }

        private SimscapeComponent? FindFileComponent(string name)
        {
            return _model.ModelFile?.FindComponent(name)
                ?? _model.FindComponent(name);
        }

        private void ResolveConnections()
        {
            foreach (var conn in _deferredConnections)
            {
                var a = _model.FindComponent(conn.ComponentNameA);
                var b = _model.FindComponent(conn.ComponentNameB);

                if (a is null)
                    _errors.Add($"Connection error: component '{conn.ComponentNameA}' not found.");
                if (b is null)
                    _errors.Add($"Connection error: component '{conn.ComponentNameB}' not found.");

                if (a is not null && b is not null)
                    a.Connect(b);
            }
        }

        private void SyncModelFileContents()
        {
            if (_model.ModelFile is null)
                return;

            var file = _model.ModelFile;

            // Ensure the model is registered in its own file
            if (!file.Models.Contains(_model))
                file.AddModel(_model);

            // Ensure all model components are also in the file
            foreach (var component in _model.Components)
            {
                if (file.FindComponent(component.Name) is null)
                    file.AddComponent(component);
            }

            // Ensure the model's domain is in the file
            if (_model.Domain is not null && file.FindDomain(_model.Domain.Name) is null)
                file.AddDomain(_model.Domain);
        }

        private void ValidateModel()
        {
            if (string.IsNullOrWhiteSpace(_model.Name))
                _errors.Add("Model name is required.");

            if (_model.Components.Count == 0)
                _errors.Add("Model must contain at least one component.");

            if (_model.Domain is not null && !_model.Domain.Validate())
                _errors.Add("Model domain is incomplete (missing Across/Through variable definitions).");

            if (_model.StopTime <= _model.StartTime)
                _errors.Add("StopTime must be greater than StartTime.");

            if (_model.MaxStepSize <= 0)
                _errors.Add("MaxStepSize must be greater than zero.");

            foreach (var component in _model.Components)
            {
                if (string.IsNullOrWhiteSpace(component.Name))
                    _errors.Add("A component is missing a name.");
            }
        }

        private void ValidateModelFile()
        {
            if (_model.ModelFile is null)
                return;

            var fileErrors = _model.ModelFile.GetValidationErrors();
            foreach (var error in fileErrors)
                _errors.Add($"Model file: {error}");
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
    /// Represents a deferred component-to-component connection resolved at build time.
    /// </summary>
    internal sealed class BuilderConnection(string componentNameA, string componentNameB)
    {
        public string ComponentNameA { get; } = componentNameA;
        public string ComponentNameB { get; } = componentNameB;
    }

    #endregion
}
