using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape model file on disk containing components, models, and library metadata.
    /// Handles serialization, loading, and file-level validation.
    /// </summary>
    public class SimscapeModelFile
    {
        #region Properties

        // File identity
        public string FileName { get; set; } = string.Empty;
        public string DirectoryPath { get; set; } = string.Empty;
        public string FullPath => Path.Combine(DirectoryPath, FileName);
        public ModelFileFormat Format { get; set; } = ModelFileFormat.Ssc;

        // Metadata
        public string Author { get; set; } = string.Empty;
        public string FileVersion { get; set; } = "1.0";
        public string Description { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Contents
        public List<SimscapeComponent> Components { get; set; } = [];
        public List<SimscapeModel> Models { get; set; } = [];
        public List<SimscapeFunction> SharedFunctions { get; set; } = [];

        // Declarations
        public List<SimscapeDomain> Domains { get; set; } = [];
        public List<SimscapeNode> Nodes { get; set; } = [];
        public List<SimscapeEquation> Equations { get; set; } = [];
        public List<SimscapeVariable> Variables { get; set; } = [];
        public List<SimscapeParameter> Parameters { get; set; } = [];

        // State
        public bool IsDirty { get; private set; }
        public bool IsReadOnly { get; set; }

        #endregion

        #region Constructors

        public SimscapeModelFile() { }

        public SimscapeModelFile(string fileName, string directoryPath)
        {
            FileName = fileName;
            DirectoryPath = directoryPath;
        }

        public SimscapeModelFile(string fullPath)
        {
            FileName = Path.GetFileName(fullPath);
            DirectoryPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
        }

        #endregion

        #region Component Methods

        /// <summary>
        /// Adds a component to the file.
        /// </summary>
        public void AddComponent(SimscapeComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (!Components.Contains(component))
            {
                Components.Add(component);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a component by name.
        /// </summary>
        public bool RemoveComponent(string name)
        {
            int removed = Components.RemoveAll(
                c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a component by name.
        /// </summary>
        public SimscapeComponent? FindComponent(string name) =>
            Components.Find(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Model Methods

        /// <summary>
        /// Adds a model to the file.
        /// </summary>
        public void AddModel(SimscapeModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (!Models.Contains(model))
            {
                Models.Add(model);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a model by name.
        /// </summary>
        public bool RemoveModel(string name)
        {
            int removed = Models.RemoveAll(
                m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a model by name.
        /// </summary>
        public SimscapeModel? FindModel(string name) =>
            Models.Find(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Shared Function Methods

        /// <summary>
        /// Adds a shared function available to all components in this file.
        /// </summary>
        public void AddSharedFunction(SimscapeFunction function)
        {
            ArgumentNullException.ThrowIfNull(function);
            if (!SharedFunctions.Contains(function))
            {
                SharedFunctions.Add(function);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a shared function by name.
        /// </summary>
        public bool RemoveSharedFunction(string name)
        {
            int removed = SharedFunctions.RemoveAll(
                f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        #endregion

        #region Domain Methods

        /// <summary>
        /// Adds a domain declaration to the file.
        /// </summary>
        public void AddDomain(SimscapeDomain domain)
        {
            ArgumentNullException.ThrowIfNull(domain);
            if (!Domains.Any(d => string.Equals(d.Name, domain.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Domains.Add(domain);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a domain by name.
        /// </summary>
        public bool RemoveDomain(string name)
        {
            int removed = Domains.RemoveAll(
                d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a domain by name.
        /// </summary>
        public SimscapeDomain? FindDomain(string name) =>
            Domains.Find(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Node Methods

        /// <summary>
        /// Adds a node declaration to the file.
        /// </summary>
        public void AddNode(SimscapeNode node)
        {
            ArgumentNullException.ThrowIfNull(node);
            if (!Nodes.Any(n => string.Equals(n.Name, node.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Nodes.Add(node);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a node by name.
        /// </summary>
        public bool RemoveNode(string name)
        {
            int removed = Nodes.RemoveAll(
                n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a node by name.
        /// </summary>
        public SimscapeNode? FindNode(string name) =>
            Nodes.Find(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Equation Methods

        /// <summary>
        /// Adds an equation declaration to the file.
        /// </summary>
        public void AddEquation(SimscapeEquation equation)
        {
            ArgumentNullException.ThrowIfNull(equation);
            if (!Equations.Any(e => string.Equals(e.Name, equation.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Equations.Add(equation);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes an equation by name.
        /// </summary>
        public bool RemoveEquation(string name)
        {
            int removed = Equations.RemoveAll(
                e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds an equation by name.
        /// </summary>
        public SimscapeEquation? FindEquation(string name) =>
            Equations.Find(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Variable Methods

        /// <summary>
        /// Adds a variable declaration to the file.
        /// </summary>
        public void AddVariable(SimscapeVariable variable)
        {
            ArgumentNullException.ThrowIfNull(variable);
            if (!Variables.Any(v => string.Equals(v.Name, variable.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Variables.Add(variable);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a variable by name.
        /// </summary>
        public bool RemoveVariable(string name)
        {
            int removed = Variables.RemoveAll(
                v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a variable by name.
        /// </summary>
        public SimscapeVariable? FindVariable(string name) =>
            Variables.Find(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Parameter Methods

        /// <summary>
        /// Adds a parameter declaration to the file.
        /// </summary>
        public void AddParameter(SimscapeParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            if (!Parameters.Any(p => string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Parameters.Add(parameter);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a parameter by name.
        /// </summary>
        public bool RemoveParameter(string name)
        {
            int removed = Parameters.RemoveAll(
                p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a parameter by name.
        /// </summary>
        public SimscapeParameter? FindParameter(string name) =>
            Parameters.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes the file contents according to the current <see cref="Format"/>.
        /// </summary>
        public string Serialize() => Format switch
        {
            ModelFileFormat.Ssc => SerializeToSsc(),
            ModelFileFormat.Json => SerializeToJson(),
            _ => SerializeToSsc()
        };

        /// <summary>
        /// Serializes the file contents to Simscape Language (.ssc) syntax.
        /// </summary>
        public string SerializeToSsc()
        {
            var sb = new StringBuilder();

            // File header comment
            WriteComment(sb, $"Simscape Model File: {FileName}");
            if (!string.IsNullOrWhiteSpace(Description))
                WriteComment(sb, Description);
            if (!string.IsNullOrWhiteSpace(Author))
                WriteComment(sb, $"Author: {Author}");
            WriteComment(sb, $"Version: {FileVersion}");
            WriteComment(sb, $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            if (!string.IsNullOrWhiteSpace(License))
                WriteComment(sb, $"License: {License}");
            sb.AppendLine();

            // Domain definitions
            foreach (var domain in Domains)
                WriteDomainBlock(sb, domain);

            // Component definitions
            foreach (var component in Components)
                WriteComponentBlock(sb, component);

            return sb.ToString();
        }

        /// <summary>
        /// Serializes the file contents to a JSON string.
        /// </summary>
        public string SerializeToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var payload = new ModelFilePayload
            {
                FileName = FileName,
                Author = Author,
                FileVersion = FileVersion,
                Description = Description,
                License = License,
                CreatedDate = CreatedDate,
                ModifiedDate = DateTime.UtcNow,
                ComponentNames = Components.Select(c => c.Name).ToList(),
                ModelNames = Models.Select(m => m.Name).ToList(),
                SharedFunctionNames = SharedFunctions.Select(f => f.Name).ToList(),
                DomainNames = Domains.Select(d => d.Name).ToList(),
                NodeNames = Nodes.Select(n => n.Name).ToList(),
                EquationNames = Equations.Select(e => e.Name).ToList(),
                VariableNames = Variables.Select(v => v.Name).ToList(),
                ParameterNames = Parameters.Select(p => p.Name).ToList()
            };
            return JsonSerializer.Serialize(payload, options);
        }

        /// <summary>
        /// Saves the serialized content to <see cref="FullPath"/>.
        /// </summary>
        public bool Save()
        {
            if (IsReadOnly)
                return false;

            try
            {
                var directory = Path.GetDirectoryName(FullPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                var content = Serialize();
                File.WriteAllText(FullPath, content, Encoding.UTF8);
                ModifiedDate = DateTime.UtcNow;
                IsDirty = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the serialized content to a specified path.
        /// </summary>
        public bool SaveAs(string fullPath)
        {
            FileName = Path.GetFileName(fullPath);
            DirectoryPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
            IsReadOnly = false;
            return Save();
        }

        /// <summary>
        /// Loads file metadata from <see cref="FullPath"/> if the file exists.
        /// Only JSON format is supported for round-trip loading.
        /// </summary>
        public bool Load()
        {
            if (!File.Exists(FullPath))
                return false;

            try
            {
                var content = File.ReadAllText(FullPath, Encoding.UTF8);
                var payload = JsonSerializer.Deserialize<ModelFilePayload>(content);
                if (payload is null) return false;

                Author = payload.Author;
                FileVersion = payload.FileVersion;
                Description = payload.Description;
                License = payload.License;
                CreatedDate = payload.CreatedDate;
                ModifiedDate = payload.ModifiedDate;
                IsDirty = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Simscape Language Writers (private)

        private static void WriteDomainBlock(StringBuilder sb, SimscapeDomain domain)
        {
            sb.AppendLine($"domain {domain.Name}");

            if (!string.IsNullOrWhiteSpace(domain.Description))
                WriteComment(sb, domain.Description, indent: 1);

            // Across variables
            sb.AppendLine("  variables");
            sb.AppendLine($"    {domain.AcrossVariableName} = {{ 0, '{domain.AcrossVariableUnit}' }};");
            sb.AppendLine("  end");

            // Through (balancing) variables
            sb.AppendLine("  variables(Balancing = true)");
            sb.AppendLine($"    {domain.ThroughVariableName} = {{ 0, '{domain.ThroughVariableUnit}' }};");
            sb.AppendLine("  end");

            // Domain parameters
            if (domain.Parameters.Count > 0)
            {
                sb.AppendLine("  parameters");
                foreach (var p in domain.Parameters)
                    sb.AppendLine($"    {p.Name} = {{ {FormatValue(p.DefaultValue)}, '{p.Unit}' }};");
                sb.AppendLine("  end");
            }

            sb.AppendLine("end");
            sb.AppendLine();
        }

        private void WriteComponentBlock(StringBuilder sb, SimscapeComponent component)
        {
            sb.AppendLine($"component {component.Name}");

            if (!string.IsNullOrWhiteSpace(component.Description))
                WriteComment(sb, component.Description, indent: 1);

            // Nodes — conserving ports tied to a physical domain
            var conservingPorts = component.Ports
                .Where(p => p.Kind == PortKind.Conserving && p.Domain.HasValue)
                .ToList();
            if (conservingPorts.Count > 0)
            {
                sb.AppendLine("  nodes");
                foreach (var port in conservingPorts)
                {
                    var domainPath = GetFoundationDomainPath(port.Domain!.Value);
                    var comment = !string.IsNullOrWhiteSpace(port.Description) ? $" % {port.Description}" : "";
                    sb.AppendLine($"    {port.Name} = {domainPath};{comment}");
                }
                sb.AppendLine("  end");
            }

            // Inputs — physical signal input ports
            var inputPorts = component.Ports
                .Where(p => p.Kind == PortKind.PhysicalSignal && p.Direction == PortDirection.Input)
                .ToList();
            if (inputPorts.Count > 0)
            {
                sb.AppendLine("  inputs");
                foreach (var port in inputPorts)
                {
                    var comment = !string.IsNullOrWhiteSpace(port.Description) ? $" % {port.Description}" : "";
                    sb.AppendLine($"    {port.Name} = {{ 0, '1' }};{comment}");
                }
                sb.AppendLine("  end");
            }

            // Outputs — physical signal output ports
            var outputPorts = component.Ports
                .Where(p => p.Kind == PortKind.PhysicalSignal && p.Direction == PortDirection.Output)
                .ToList();
            if (outputPorts.Count > 0)
            {
                sb.AppendLine("  outputs");
                foreach (var port in outputPorts)
                {
                    var comment = !string.IsNullOrWhiteSpace(port.Description) ? $" % {port.Description}" : "";
                    sb.AppendLine($"    {port.Name} = {{ 0, '1' }};{comment}");
                }
                sb.AppendLine("  end");
            }

            // Parameters
            WriteParameterSections(sb, component);

            // Variables
            WriteVariableSections(sb, component);

            // Functions
            WriteFunctionSections(sb, component);

            // Branches
            WriteBranchSection(sb, component);

            // Equations
            WriteEquationSections(sb, component);

            sb.AppendLine("end");
            sb.AppendLine();
        }

        private void WriteParameterSections(StringBuilder sb, SimscapeComponent component)
        {
            // Collect file-level parameters that belong to this component
            var fileParams = Parameters
                .Where(p => component.Parameters.Any(
                    cp => string.Equals(cp.Name, p.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var publicParams = fileParams.Where(p => p.Access == ParameterAccess.Public).ToList();
            var privateParams = fileParams.Where(p => p.Access == ParameterAccess.Private).ToList();

            // Also include component-only parameters not in the file-level list
            var componentOnly = component.Parameters
                .Where(cp => !fileParams.Any(
                    fp => string.Equals(fp.Name, cp.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (publicParams.Count > 0 || componentOnly.Count > 0)
            {
                sb.AppendLine("  parameters");
                foreach (var p in publicParams)
                    WriteParameterDeclaration(sb, p);
                foreach (var cp in componentOnly)
                    sb.AppendLine($"    {cp.Name} = {{ {FormatValue(cp.DefaultValue)}, '{cp.Unit}' }};");
                sb.AppendLine("  end");
            }

            if (privateParams.Count > 0)
            {
                sb.AppendLine("  parameters(Access = private)");
                foreach (var p in privateParams)
                    WriteParameterDeclaration(sb, p);
                sb.AppendLine("  end");
            }
        }

        private static void WriteParameterDeclaration(StringBuilder sb, SimscapeParameter p)
        {
            var comment = !string.IsNullOrWhiteSpace(p.Description) ? $" % {p.Description}" : "";
            sb.AppendLine($"    {p.Name} = {{ {FormatValue(p.DefaultValue)}, '{p.Unit}' }};{comment}");
        }

        private void WriteVariableSections(StringBuilder sb, SimscapeComponent component)
        {
            // Collect file-level variables that belong to this component
            var fileVars = Variables
                .Where(v => component.Variables.Any(
                    cv => string.Equals(cv.Name, v.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Also include component-only variables not in the file-level list
            var componentOnly = component.Variables
                .Where(cv => !fileVars.Any(
                    fv => string.Equals(fv.Name, cv.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (fileVars.Count > 0 || componentOnly.Count > 0)
            {
                sb.AppendLine("  variables");
                foreach (var v in fileVars)
                {
                    var priorityAttr = v.Priority != InitializationPriority.None
                        ? $"  % priority = {v.Priority.ToString().ToLowerInvariant()}"
                        : "";
                    var comment = !string.IsNullOrWhiteSpace(v.Description) ? $" % {v.Description}" : "";
                    sb.AppendLine($"    {v.Name} = {{ {FormatValue(v.InitialValue)}, '{v.Unit}' }};{comment}{priorityAttr}");
                }
                foreach (var cv in componentOnly)
                    sb.AppendLine($"    {cv.Name} = {{ {FormatValue(cv.InitialValue)}, '{cv.Unit}' }};");
                sb.AppendLine("  end");
            }
        }

        private void WriteFunctionSections(StringBuilder sb, SimscapeComponent component)
        {
            // Write shared functions referenced by this component's equations
            var relevantFunctions = SharedFunctions
                .Where(f => component.Equations.Any(
                    eq => eq.Contains(f.Name, StringComparison.Ordinal)))
                .ToList();

            foreach (var fn in relevantFunctions)
            {
                var inputs = string.Join(", ", fn.Inputs.Select(p => p.Name));
                var outputs = string.Join(", ", fn.Outputs.Select(p => p.Name));

                sb.AppendLine($"  function [{outputs}] = {fn.Name}({inputs})");

                if (!string.IsNullOrWhiteSpace(fn.Description))
                    WriteComment(sb, fn.Description, indent: 2);

                switch (fn.Type)
                {
                    case FunctionType.Inline:
                        sb.AppendLine($"    {outputs} = {fn.Expression};");
                        break;

                    case FunctionType.LookupTable when fn.LookupTable is not null:
                        var bp = string.Join(", ", fn.LookupTable.Breakpoints1.Select(FormatValue));
                        var vals = string.Join(", ", fn.LookupTable.Values.Select(FormatValue));
                        sb.AppendLine($"    bp = [{bp}];");
                        sb.AppendLine($"    tbl = [{vals}];");
                        sb.AppendLine($"    {outputs} = tablelookup(bp, tbl, {inputs}, interpolation = {fn.LookupTable.Interpolation.ToString().ToLowerInvariant()});");
                        break;

                    case FunctionType.Piecewise:
                        for (int i = 0; i < fn.PiecewiseSegments.Count; i++)
                        {
                            var seg = fn.PiecewiseSegments[i];
                            if (string.IsNullOrWhiteSpace(seg.Condition))
                            {
                                sb.AppendLine($"    else");
                            }
                            else
                            {
                                var keyword = i == 0 ? "if" : "elseif";
                                sb.AppendLine($"    {keyword} {seg.Condition}");
                            }
                            sb.AppendLine($"      {outputs} = {seg.Expression};");
                        }
                        sb.AppendLine("    end");
                        break;
                }

                sb.AppendLine("  end");
                sb.AppendLine();
            }
        }

        private void WriteBranchSection(StringBuilder sb, SimscapeComponent component)
        {
            // Collect equations that reference branch through-variables
            var componentEquations = GetComponentEquations(component);
            var branchVars = Variables
                .Where(v => v.Kind == VariableKind.Through &&
                            component.Variables.Any(
                                cv => string.Equals(cv.Name, v.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Find nodes for this component to build branch declarations
            var componentNodes = Nodes
                .Where(n => component.Ports.Any(
                    p => string.Equals(p.Name, n.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (branchVars.Count > 0 && componentNodes.Count >= 2)
            {
                sb.AppendLine("  branches");
                foreach (var throughVar in branchVars)
                {
                    // Convention: first node is +, second is -
                    var from = componentNodes[0];
                    var to = componentNodes[1];
                    sb.AppendLine($"    {throughVar.Name} : {from.Name}.{throughVar.Name} -> {to.Name}.{throughVar.Name};");
                }
                sb.AppendLine("  end");
            }
        }

        private void WriteEquationSections(StringBuilder sb, SimscapeComponent component)
        {
            var componentEquations = GetComponentEquations(component);
            var initialEquations = componentEquations
                .Where(e => e.Type == EquationType.InitialCondition).ToList();
            var standardEquations = componentEquations
                .Where(e => e.Type != EquationType.InitialCondition).ToList();

            // Also include string-only equations from the component itself
            var stringOnlyEquations = component.Equations
                .Where(eq => !componentEquations.Any(
                    ce => string.Equals(ce.Expression, eq, StringComparison.Ordinal)))
                .ToList();

            if (standardEquations.Count > 0 || stringOnlyEquations.Count > 0)
            {
                sb.AppendLine("  equations");
                foreach (var eq in standardEquations)
                    WriteEquation(sb, eq);
                foreach (var expr in stringOnlyEquations)
                    sb.AppendLine($"    {expr};");
                sb.AppendLine("  end");
            }

            if (initialEquations.Count > 0)
            {
                sb.AppendLine("  equations (Initial = true)");
                foreach (var eq in initialEquations)
                    WriteEquation(sb, eq);
                sb.AppendLine("  end");
            }
        }

        private static void WriteEquation(StringBuilder sb, SimscapeEquation eq)
        {
            if (eq.IsConditional && !string.IsNullOrWhiteSpace(eq.Condition))
            {
                sb.AppendLine($"    if {eq.Condition}");
                sb.AppendLine($"      {eq.Expression};");
                sb.AppendLine("    end");
            }
            else
            {
                sb.AppendLine($"    {eq.Expression};");
            }
        }

        private List<SimscapeEquation> GetComponentEquations(SimscapeComponent component)
        {
            return Equations
                .Where(e => e.OwningComponent == component ||
                            (e.OwningComponent is not null &&
                             string.Equals(e.OwningComponent.Name, component.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private static void WriteComment(StringBuilder sb, string text, int indent = 0)
        {
            var prefix = new string(' ', indent * 2);
            foreach (var line in text.Split('\n'))
                sb.AppendLine($"{prefix}% {line.TrimEnd()}");
        }

        private static string FormatValue(double value) =>
            value.ToString("G", CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns the Simscape foundation domain path for a given domain type.
        /// </summary>
        private static string GetFoundationDomainPath(DomainType type) => type switch
        {
            DomainType.Electrical => "foundation.electrical.electrical",
            DomainType.MechanicalTranslational => "foundation.mechanical.translational.translational",
            DomainType.MechanicalRotational => "foundation.mechanical.rotational.rotational",
            DomainType.Hydraulic => "foundation.hydraulic.hydraulic",
            DomainType.Thermal => "foundation.thermal.thermal",
            DomainType.Magnetic => "foundation.magnetic.magnetic",
            DomainType.Pneumatic => "foundation.gas.gas",
            _ => "foundation.electrical.electrical"
        };

        #endregion

        #region Validation & State

        /// <summary>
        /// Checks whether the file exists on disk.
        /// </summary>
        public bool ExistsOnDisk() => File.Exists(FullPath);

        /// <summary>
        /// Returns the file extension based on the format.
        /// </summary>
        public string GetExtension() => Format switch
        {
            ModelFileFormat.Ssc => ".ssc",
            ModelFileFormat.Slx => ".slx",
            ModelFileFormat.Json => ".json",
            ModelFileFormat.Xml => ".xml",
            _ => ".ssc"
        };

        /// <summary>
        /// Marks the file as having unsaved changes.
        /// </summary>
        public void MarkDirty()
        {
            IsDirty = true;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the file has a name, valid path, and all contents are valid.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(FileName) &&
            !string.IsNullOrWhiteSpace(DirectoryPath) &&
            (Components.Count > 0 || Models.Count > 0) &&
            Components.All(c => c.Validate()) &&
            Models.All(m => m.Validate()) &&
            SharedFunctions.All(f => f.Validate()) &&
            Domains.All(d => d.Validate()) &&
            Equations.All(e => e.Validate()) &&
            Variables.All(v => v.Validate()) &&
            Parameters.All(p => p.Validate());

        /// <summary>
        /// Collects validation errors across all contents.
        /// </summary>
        public IReadOnlyList<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FileName))
                errors.Add("File name is required.");
            if (string.IsNullOrWhiteSpace(DirectoryPath))
                errors.Add("Directory path is required.");
            if (Components.Count == 0 && Models.Count == 0)
                errors.Add("File must contain at least one component or model.");

            for (int i = 0; i < Components.Count; i++)
            {
                if (!Components[i].Validate())
                    errors.Add($"Component '{Components[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < Models.Count; i++)
            {
                if (!Models[i].Validate())
                    errors.Add($"Model '{Models[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < SharedFunctions.Count; i++)
            {
                if (!SharedFunctions[i].Validate())
                    errors.Add($"Shared function '{SharedFunctions[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < Domains.Count; i++)
            {
                if (!Domains[i].Validate())
                    errors.Add($"Domain '{Domains[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < Equations.Count; i++)
            {
                if (!Equations[i].Validate())
                    errors.Add($"Equation '{Equations[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < Variables.Count; i++)
            {
                if (!Variables[i].Validate())
                    errors.Add($"Variable '{Variables[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (!Parameters[i].Validate())
                    errors.Add($"Parameter '{Parameters[i].Name}' at index {i} failed validation.");
            }

            return errors.AsReadOnly();
        }

        public override string ToString() =>
            $"{FileName} ({Components.Count} components, {Models.Count} models, " +
            $"{Domains.Count} domains, {Nodes.Count} nodes, {Equations.Count} equations, " +
            $"{Variables.Count} variables, {Parameters.Count} parameters" +
            $"{(IsDirty ? ", *modified*" : "")})";

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Simscape file format types.
    /// </summary>
    public enum ModelFileFormat
    {
        Ssc,
        Slx,
        Json,
        Xml
    }

    /// <summary>
    /// Serialization payload for JSON model file metadata.
    /// </summary>
    internal class ModelFilePayload
    {
        public string FileName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string FileVersion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<string> ComponentNames { get; set; } = [];
        public List<string> ModelNames { get; set; } = [];
        public List<string> SharedFunctionNames { get; set; } = [];
        public List<string> DomainNames { get; set; } = [];
        public List<string> NodeNames { get; set; } = [];
        public List<string> EquationNames { get; set; } = [];
        public List<string> VariableNames { get; set; } = [];
        public List<string> ParameterNames { get; set; } = [];
    }

    #endregion
}
