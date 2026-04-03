using System;
using System.Collections.Generic;
using Simulation;

namespace CommunityMaker.Simscape
{
    /// <summary>
    /// Simscape Fluids add-in for modeling hydraulic and pneumatic fluid power systems.
    /// </summary>
    public sealed class FluidsAddin : SimscapeAddin
    {
        #region Properties

        // System components
        public List<FluidPipe> Pipes { get; set; } = [];
        public List<FluidPump> Pumps { get; set; } = [];
        public List<FluidValve> Valves { get; set; } = [];
        public List<FluidTank> Tanks { get; set; } = [];
        public List<FluidActuator> Actuators { get; set; } = [];

        // Fluid properties
        public FluidProperties Fluid { get; set; } = FluidProperties.DefaultHydraulicOil;

        // Environment
        public double AmbientPressure { get; set; } = 101325.0;  // Pa
        public double AmbientTemperature { get; set; } = 293.15; // K

        #endregion

        #region Constructors

        public FluidsAddin()
            : base(SimscapeAddinKind.Fluids, "Simscape Fluids",
                   ["Simscape", "Simscape Fluids"])
        {
            Tags = ["fluids", "hydraulic", "pneumatic", "pipe", "valve"];
            SupportedDomains = [DomainType.Hydraulic, DomainType.Pneumatic, DomainType.Thermal];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a pipe segment to the system.
        /// </summary>
        public void AddPipe(string name, double length, double innerDiameter)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Pipes.Add(new FluidPipe
            {
                Name = name,
                Length = length,
                InnerDiameter = innerDiameter
            });
        }

        /// <summary>
        /// Removes a pipe by name.
        /// </summary>
        public bool RemovePipe(string name) =>
            Pipes.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a pipe by name.
        /// </summary>
        public FluidPipe? FindPipe(string name) =>
            Pipes.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a pump to the system.
        /// </summary>
        public void AddPump(string name, PumpType type, double displacement)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Pumps.Add(new FluidPump
            {
                Name = name,
                Type = type,
                Displacement = displacement
            });
        }

        /// <summary>
        /// Removes a pump by name.
        /// </summary>
        public bool RemovePump(string name) =>
            Pumps.RemoveAll(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a valve to the system.
        /// </summary>
        public void AddValve(string name, ValveType type, double maxOpening = 1.0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Valves.Add(new FluidValve
            {
                Name = name,
                Type = type,
                MaxOpening = maxOpening
            });
        }

        /// <summary>
        /// Removes a valve by name.
        /// </summary>
        public bool RemoveValve(string name) =>
            Valves.RemoveAll(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a storage tank to the system.
        /// </summary>
        public void AddTank(string name, double volume, double initialLevel = 0.5)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Tanks.Add(new FluidTank
            {
                Name = name,
                Volume = volume,
                InitialFluidLevel = initialLevel
            });
        }

        /// <summary>
        /// Removes a tank by name.
        /// </summary>
        public bool RemoveTank(string name) =>
            Tanks.RemoveAll(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a hydraulic or pneumatic actuator.
        /// </summary>
        public void AddActuator(string name, ActuatorType type, double boreDiameter, double stroke)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Actuators.Add(new FluidActuator
            {
                Name = name,
                Type = type,
                BoreDiameter = boreDiameter,
                Stroke = stroke
            });
        }

        /// <summary>
        /// Removes an actuator by name.
        /// </summary>
        public bool RemoveActuator(string name) =>
            Actuators.RemoveAll(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Sets the working fluid properties.
        /// </summary>
        public void SetFluid(string name, double density, double viscosity, double bulkModulus)
        {
            Fluid = new FluidProperties
            {
                Name = name,
                Density = density,
                KinematicViscosity = viscosity,
                BulkModulus = bulkModulus
            };
        }

        /// <summary>
        /// Validates at least one flow component exists and base requirements are met.
        /// </summary>
        public override bool Validate() =>
            base.Validate() &&
            (Pipes.Count > 0 || Pumps.Count > 0 || Valves.Count > 0 || Actuators.Count > 0);

        #endregion
    }

    #region Fluids Supporting Types

    public class FluidPipe
    {
        public string Name { get; set; } = string.Empty;
        public double Length { get; set; }          // m
        public double InnerDiameter { get; set; }   // m
        public double WallThickness { get; set; }   // m
        public double Roughness { get; set; } = 1.5e-6; // m (surface roughness)
    }

    public class FluidPump
    {
        public string Name { get; set; } = string.Empty;
        public PumpType Type { get; set; }
        public double Displacement { get; set; }       // m³/rev
        public double MaxPressure { get; set; }        // Pa
        public double VolumetricEfficiency { get; set; } = 0.92;
    }

    public class FluidValve
    {
        public string Name { get; set; } = string.Empty;
        public ValveType Type { get; set; }
        public double MaxOpening { get; set; } = 1.0;  // 0..1 fraction
        public double CurrentOpening { get; set; }      // 0..1 fraction
        public double FlowCoefficient { get; set; }     // Cv
    }

    public class FluidTank
    {
        public string Name { get; set; } = string.Empty;
        public double Volume { get; set; }             // m³
        public double InitialFluidLevel { get; set; }  // 0..1 fraction
        public double InitialPressure { get; set; } = 101325.0; // Pa
    }

    public class FluidActuator
    {
        public string Name { get; set; } = string.Empty;
        public ActuatorType Type { get; set; }
        public double BoreDiameter { get; set; }  // m
        public double Stroke { get; set; }         // m
        public double RodDiameter { get; set; }    // m (double-acting only)
    }

    public class FluidProperties
    {
        public string Name { get; set; } = string.Empty;
        public double Density { get; set; }              // kg/m³
        public double KinematicViscosity { get; set; }   // m²/s
        public double BulkModulus { get; set; }          // Pa
        public double SpecificHeatCapacity { get; set; } // J/(kg·K)

        /// <summary>Standard ISO VG 32 hydraulic oil at 40 °C.</summary>
        public static FluidProperties DefaultHydraulicOil => new()
        {
            Name = "ISO VG 32",
            Density = 860.0,
            KinematicViscosity = 3.2e-5,
            BulkModulus = 1.4e9,
            SpecificHeatCapacity = 1900.0
        };
    }

    public enum PumpType
    {
        GearPump,
        VanePump,
        PistonPump,
        CentrifugalPump,
        ScrewPump
    }

    public enum ValveType
    {
        CheckValve,
        ReliefValve,
        DirectionalControl,
        ProportionalControl,
        ServoValve,
        FlowControl,
        PressureReducing,
        Throttle
    }

    public enum ActuatorType
    {
        SingleActingCylinder,
        DoubleActingCylinder,
        RotaryActuator,
        HydraulicMotor
    }

    #endregion
}
