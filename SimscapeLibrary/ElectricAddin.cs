using System;
using System.Collections.Generic;
using Simulation;

namespace CommunityMaker.Simscape
{
    /// <summary>
    /// Simscape Electrical add-in for modeling circuits, power electronics, and electric machines.
    /// </summary>
    public sealed class ElectricAddin : SimscapeAddin
    {
        #region Properties

        // Circuit components
        public List<CircuitElement> CircuitElements { get; set; } = [];
        public List<PowerSource> PowerSources { get; set; } = [];
        public List<Semiconductor> Semiconductors { get; set; } = [];
        public List<ElectricMachine> Machines { get; set; } = [];

        // Simulation settings
        public CircuitAnalysisType AnalysisType { get; set; } = CircuitAnalysisType.Transient;
        public double SwitchingFrequency { get; set; } // Hz

        #endregion

        #region Constructors

        public ElectricAddin()
            : base(SimscapeAddinKind.Electrical, "Simscape Electrical",
                   ["Simscape", "Simscape Electrical"])
        {
            Tags = ["electrical", "circuit", "power", "electronics"];
            SupportedDomains = [DomainType.Electrical, DomainType.Magnetic, DomainType.Thermal];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a passive circuit element (resistor, capacitor, inductor, etc.).
        /// </summary>
        public void AddCircuitElement(string name, CircuitElementType type, double value, string unit)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            CircuitElements.Add(new CircuitElement
            {
                Name = name,
                Type = type,
                Value = value,
                Unit = unit
            });
        }

        /// <summary>
        /// Removes a circuit element by name.
        /// </summary>
        public bool RemoveCircuitElement(string name) =>
            CircuitElements.RemoveAll(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a circuit element by name.
        /// </summary>
        public CircuitElement? FindCircuitElement(string name) =>
            CircuitElements.Find(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a power source (DC, AC, or controlled).
        /// </summary>
        public void AddPowerSource(string name, PowerSourceType type, double voltage, double frequency = 0.0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            PowerSources.Add(new PowerSource
            {
                Name = name,
                Type = type,
                Voltage = voltage,
                Frequency = frequency
            });
        }

        /// <summary>
        /// Removes a power source by name.
        /// </summary>
        public bool RemovePowerSource(string name) =>
            PowerSources.RemoveAll(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a semiconductor switching device.
        /// </summary>
        public void AddSemiconductor(string name, SemiconductorType type, double ratedVoltage, double ratedCurrent)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Semiconductors.Add(new Semiconductor
            {
                Name = name,
                Type = type,
                RatedVoltage = ratedVoltage,
                RatedCurrent = ratedCurrent
            });
        }

        /// <summary>
        /// Removes a semiconductor by name.
        /// </summary>
        public bool RemoveSemiconductor(string name) =>
            Semiconductors.RemoveAll(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds an electric machine (motor/generator).
        /// </summary>
        public void AddMachine(string name, MachineType type, double ratedPower, double ratedVoltage)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Machines.Add(new ElectricMachine
            {
                Name = name,
                Type = type,
                RatedPower = ratedPower,
                RatedVoltage = ratedVoltage
            });
        }

        /// <summary>
        /// Removes a machine by name.
        /// </summary>
        public bool RemoveMachine(string name) =>
            Machines.RemoveAll(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Validates at least one component exists and base requirements are met.
        /// </summary>
        public override bool Validate() =>
            base.Validate() &&
            (CircuitElements.Count > 0 || PowerSources.Count > 0 ||
             Semiconductors.Count > 0 || Machines.Count > 0);

        #endregion
    }

    #region Electrical Supporting Types

    public class CircuitElement
    {
        public string Name { get; set; } = string.Empty;
        public CircuitElementType Type { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double InitialCondition { get; set; }
    }

    public class PowerSource
    {
        public string Name { get; set; } = string.Empty;
        public PowerSourceType Type { get; set; }
        public double Voltage { get; set; }     // V
        public double Frequency { get; set; }   // Hz (AC only)
        public double PhaseAngle { get; set; }  // deg (AC only)
        public double InternalResistance { get; set; }
    }

    public class Semiconductor
    {
        public string Name { get; set; } = string.Empty;
        public SemiconductorType Type { get; set; }
        public double RatedVoltage { get; set; }  // V
        public double RatedCurrent { get; set; }  // A
        public double OnResistance { get; set; }  // Ω
        public double SwitchingLoss { get; set; } // W
    }

    public class ElectricMachine
    {
        public string Name { get; set; } = string.Empty;
        public MachineType Type { get; set; }
        public double RatedPower { get; set; }    // W
        public double RatedVoltage { get; set; }  // V
        public double RatedSpeed { get; set; }    // rpm
        public double Efficiency { get; set; } = 0.95;
    }

    public enum CircuitElementType
    {
        Resistor,
        Capacitor,
        Inductor,
        Transformer,
        MutualInductor,
        VariableResistor
    }

    public enum PowerSourceType
    {
        DCVoltage,
        ACVoltage,
        DCCurrent,
        ACCurrent,
        ControlledVoltage,
        ControlledCurrent,
        Battery
    }

    public enum SemiconductorType
    {
        Diode,
        MOSFET,
        IGBT,
        Thyristor,
        GTO,
        BJT
    }

    public enum MachineType
    {
        DCMotor,
        InductionMotor,
        PermanentMagnetSynchronous,
        SwitchedReluctance,
        StepperMotor,
        UniversalMotor
    }

    public enum CircuitAnalysisType
    {
        Transient,
        SteadyState,
        FrequencyDomain
    }

    #endregion
}
