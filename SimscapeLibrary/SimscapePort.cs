using System;
using System.Collections.Generic;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape port that connects elements via physical signal or conserving connections.
    /// </summary>
    public class SimscapePort
    {
        #region Properties

        // Identification
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Classification
        public PortKind Kind { get; set; }
        public PortDirection Direction { get; set; }

        // Domain association (required for conserving ports)
        public DomainType? Domain { get; set; }

        // Across and Through variable values at this port
        public double AcrossValue { get; set; }
        public double ThroughValue { get; set; }

        // Connection state
        public SimscapePort? ConnectedPort { get; set; }
        public bool IsConnected => ConnectedPort is not null;

        #endregion

        #region Constructors

        public SimscapePort() { }

        /// <summary>
        /// Creates a physical signal port (input or output).
        /// </summary>
        public SimscapePort(string name, PortDirection direction)
        {
            Name = name;
            Direction = direction;
            Kind = PortKind.PhysicalSignal;
        }

        /// <summary>
        /// Creates a conserving port associated with a physical domain.
        /// </summary>
        public SimscapePort(string name, DomainType domain)
        {
            Name = name;
            Domain = domain;
            Direction = PortDirection.Conserving;
            Kind = PortKind.Conserving;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects this port to another port. Conserving ports must share the same domain.
        /// </summary>
        public bool Connect(SimscapePort other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (other == this)
                return false;

            if (Kind == PortKind.Conserving && other.Kind == PortKind.Conserving && Domain != other.Domain)
                return false;

            Disconnect();
            other.Disconnect();

            ConnectedPort = other;
            other.ConnectedPort = this;
            return true;
        }

        /// <summary>
        /// Disconnects this port from its connected port.
        /// </summary>
        public void Disconnect()
        {
            if (ConnectedPort is not null)
            {
                ConnectedPort.ConnectedPort = null;
                ConnectedPort = null;
            }
        }

        /// <summary>
        /// Sets the across and through variable values at this port.
        /// </summary>
        public void SetValues(double across, double through)
        {
            AcrossValue = across;
            ThroughValue = through;
        }

        /// <summary>
        /// Resets variable values to zero.
        /// </summary>
        public void ResetValues()
        {
            AcrossValue = 0.0;
            ThroughValue = 0.0;
        }

        /// <summary>
        /// Validates the port has a name and conserving ports have a domain assigned.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            (Kind != PortKind.Conserving || Domain is not null);

        public override string ToString() =>
            $"{Name} ({Kind}, {Direction}{(Domain.HasValue ? $", {Domain}" : "")})";

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Distinguishes physical signal ports from conserving (domain-based) ports.
    /// </summary>
    public enum PortKind
    {
        PhysicalSignal,
        Conserving
    }

    /// <summary>
    /// Direction of signal flow through a port.
    /// </summary>
    public enum PortDirection
    {
        Input,
        Output,
        Conserving
    }

    #endregion
}
