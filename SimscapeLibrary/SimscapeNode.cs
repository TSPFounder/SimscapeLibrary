using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a connection point in a Simscape network where branches meet.
    /// Each node holds the Across variable value for its domain (e.g., voltage, velocity).
    /// </summary>
    public class SimscapeNode
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Domain
        public DomainType Domain { get; set; }

        // Across variable value at this node
        public double AcrossValue { get; set; }

        // Topology
        public List<SimscapePort> ConnectedPorts { get; set; } = [];
        public List<SimscapeBranch> ConnectedBranches { get; set; } = [];

        // Reference node flag (e.g., ground / mechanical reference)
        public bool IsReference { get; set; }

        #endregion

        #region Constructors

        public SimscapeNode() { }

        public SimscapeNode(string name, DomainType domain)
        {
            Name = name;
            Domain = domain;
        }

        /// <summary>
        /// Creates a reference (ground) node for the given domain with Across value fixed at zero.
        /// </summary>
        public static SimscapeNode CreateReference(string name, DomainType domain) =>
            new(name, domain) { IsReference = true, AcrossValue = 0.0 };

        #endregion

        #region Methods

        /// <summary>
        /// Attaches a port to this node. The port must share the same domain.
        /// </summary>
        public bool AddPort(SimscapePort port)
        {
            ArgumentNullException.ThrowIfNull(port);
            if (port.Domain is not null && port.Domain != Domain)
                return false;
            if (!ConnectedPorts.Contains(port))
                ConnectedPorts.Add(port);
            return true;
        }

        /// <summary>
        /// Removes a port from this node.
        /// </summary>
        public bool RemovePort(SimscapePort port) =>
            ConnectedPorts.Remove(port);

        /// <summary>
        /// Attaches a branch to this node.
        /// </summary>
        public void AddBranch(SimscapeBranch branch)
        {
            ArgumentNullException.ThrowIfNull(branch);
            if (!ConnectedBranches.Contains(branch))
                ConnectedBranches.Add(branch);
        }

        /// <summary>
        /// Removes a branch from this node.
        /// </summary>
        public bool RemoveBranch(SimscapeBranch branch) =>
            ConnectedBranches.Remove(branch);

        /// <summary>
        /// Computes the net Through variable at this node (Kirchhoff's law: sum should equal zero).
        /// </summary>
        public double GetNetThroughValue()
        {
            double sum = 0.0;
            foreach (var branch in ConnectedBranches)
            {
                // Through value is positive into the node at the "from" end, negative at the "to" end.
                if (branch.FromNode == this)
                    sum += branch.ThroughValue;
                else if (branch.ToNode == this)
                    sum -= branch.ThroughValue;
            }
            return sum;
        }

        /// <summary>
        /// Checks whether Kirchhoff's law is satisfied within the given tolerance.
        /// </summary>
        public bool IsThroughBalanced(double tolerance = 1e-9) =>
            Math.Abs(GetNetThroughValue()) <= tolerance;

        /// <summary>
        /// Resets the node's Across value to zero.
        /// </summary>
        public void Reset() => AcrossValue = 0.0;

        /// <summary>
        /// Validates the node has a name and at least one connection.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            (ConnectedPorts.Count > 0 || ConnectedBranches.Count > 0);

        public override string ToString() =>
            $"{Name} ({Domain}, Across={AcrossValue}{(IsReference ? ", REF" : "")})";

        #endregion
    }
}
