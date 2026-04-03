// SimscapeAddin.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Simulation;

namespace CommunityMaker.Simscape
{
    /// <summary>
    /// Base class for Simscape add-in libraries that extend simulation capabilities.
    /// </summary>
    public abstract class SimscapeAddin
    {
        #region Properties

        // Identification
        public SimscapeAddinKind Kind { get; }
        public string DisplayName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Requirements
        public List<string> RequiredMatlabProducts { get; set; } = [];
        public string MinimumMatlabVersion { get; set; } = string.Empty;

        // Classification
        public List<string> Tags { get; set; } = [];
        public List<DomainType> SupportedDomains { get; set; } = [];
        public List<string> ComponentLibraries { get; set; } = [];

        // State
        public bool IsInstalled { get; set; }
        public bool IsLicensed { get; set; }

        #endregion

        #region Constructors

        protected SimscapeAddin(SimscapeAddinKind kind) => Kind = kind;

        protected SimscapeAddin(SimscapeAddinKind kind, string displayName, List<string> requiredProducts)
        {
            Kind = kind;
            DisplayName = displayName;
            RequiredMatlabProducts = requiredProducts ?? [];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks whether all required products are available.
        /// </summary>
        public virtual bool CheckAvailability() => IsInstalled && IsLicensed;

        /// <summary>
        /// Registers a component library for this add-in.
        /// </summary>
        public void AddComponentLibrary(string libraryPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(libraryPath);
            if (!ComponentLibraries.Contains(libraryPath))
                ComponentLibraries.Add(libraryPath);
        }

        /// <summary>
        /// Removes a component library from this add-in.
        /// </summary>
        public bool RemoveComponentLibrary(string libraryPath) =>
            ComponentLibraries.Remove(libraryPath);

        /// <summary>
        /// Adds a supported physical domain to this add-in.
        /// </summary>
        public void AddSupportedDomain(DomainType domain)
        {
            if (!SupportedDomains.Contains(domain))
                SupportedDomains.Add(domain);
        }

        /// <summary>
        /// Checks whether this add-in supports the given domain.
        /// </summary>
        public bool SupportsDomain(DomainType domain) =>
            SupportedDomains.Contains(domain);

        /// <summary>
        /// Generates a MATLAB script that checks required product availability.
        /// </summary>
        public virtual string ToMatlabLicenseCheckScript(string resultVar = "isAvailable")
        {
            var lines = new List<string> { $"{resultVar} = true;", "v = ver;" };

            foreach (var product in RequiredMatlabProducts)
            {
                var escaped = product.Replace("'", "''");
                lines.Add($"{resultVar} = {resultVar} && any(strcmp({{v.Name}}, '{escaped}'));");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Validates that the add-in has a name and at least one required product.
        /// </summary>
        public virtual bool Validate() =>
            !string.IsNullOrWhiteSpace(DisplayName) &&
            RequiredMatlabProducts.Count > 0;

        #endregion
    }

    #region Supporting Types

    public enum SimscapeAddinKind
    {
        Simscape = 0,
        Multibody,
        Electrical,
        Fluids,
        Driveline,
        Battery,
        Thermal,
        PowerSystems
    }

    #endregion
}
