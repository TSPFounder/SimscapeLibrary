using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a connection between two nodes carrying a Through variable (e.g., current, force).
    /// The Across value is the potential difference between the terminal nodes.
    /// </summary>
    public class SimscapeBranch
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Domain
        public DomainType Domain { get; set; }

        // Terminal nodes
        public SimscapeNode? FromNode { get; set; }
        public SimscapeNode? ToNode { get; set; }

        // Through variable (flow through the branch, e.g., current in A, force in N)
        public double ThroughValue { get; set; }

        // Owning component
        public SimscapeComponent? OwningComponent { get; set; }

        // Branch parameters
        public List<SimscapeParameter> Parameters { get; set; } = [];

        // Variables tracked on this branch
        public List<SimscapeVariable> Variables { get; set; } = [];

        #endregion

        #region Constructors

        public SimscapeBranch() { }

        public SimscapeBranch(string name, DomainType domain)
        {
            Name = name;
            Domain = domain;
        }

        public SimscapeBranch(string name, DomainType domain, SimscapeNode from, SimscapeNode to)
        {
            Name = name;
            Domain = domain;
            FromNode = from;
            ToNode = to;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Computes the Across value (potential difference) from FromNode to ToNode.
        /// </summary>
        public double ComputeAcrossValue()
        {
            if (FromNode is null || ToNode is null)
                return 0.0;
            return FromNode.AcrossValue - ToNode.AcrossValue;
        }

        /// <summary>
        /// Connects this branch between two nodes and registers itself on both.
        /// </summary>
        public void Connect(SimscapeNode from, SimscapeNode to)
        {
            ArgumentNullException.ThrowIfNull(from);
            ArgumentNullException.ThrowIfNull(to);

            // Detach from previous nodes
            Disconnect();

            FromNode = from;
            ToNode = to;
            from.AddBranch(this);
            to.AddBranch(this);
        }

        /// <summary>
        /// Disconnects this branch from both terminal nodes.
        /// </summary>
        public void Disconnect()
        {
            FromNode?.RemoveBranch(this);
            ToNode?.RemoveBranch(this);
            FromNode = null;
            ToNode = null;
        }

        /// <summary>
        /// Reverses the branch direction by swapping FromNode and ToNode.
        /// The Through value sign is inverted to maintain sign convention.
        /// </summary>
        public void Reverse()
        {
            (FromNode, ToNode) = (ToNode, FromNode);
            ThroughValue = -ThroughValue;
        }

        /// <summary>
        /// Adds a parameter to this branch.
        /// </summary>
        public void AddParameter(string name, string unit, double defaultValue)
        {
            Parameters.Add(new SimscapeParameter(name, unit, defaultValue));
        }

        /// <summary>
        /// Adds a variable tracked on this branch.
        /// </summary>
        public void AddVariable(string name, string unit, VariableKind kind, double initialValue = 0.0)
        {
            Variables.Add(new SimscapeVariable(name, unit, kind, initialValue));
        }

        /// <summary>
        /// Finds a parameter by name.
        /// </summary>
        public SimscapeParameter? FindParameter(string name) =>
            Parameters.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Resets the Through value and all variable values to their defaults.
        /// </summary>
        public void Reset()
        {
            ThroughValue = 0.0;
            foreach (var v in Variables)
                v.Reset();
        }

        /// <summary>
        /// Validates the branch has a name and both terminal nodes assigned.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            FromNode is not null &&
            ToNode is not null &&
            FromNode != ToNode;

        public override string ToString() =>
            $"{Name} ({Domain}: {FromNode?.Name ?? "?"} → {ToNode?.Name ?? "?"}, Through={ThroughValue})";

        #endregion
    }
}
