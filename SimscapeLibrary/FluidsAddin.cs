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

    /// <summary>
    /// All blocks available in the Simscape Fluids library.
    /// Covers isothermal liquid, thermal liquid, gas, two-phase fluid, and moist air domains.
    /// </summary>
    public enum FluidsBlockType
    {
        // =====================================================================
        // Isothermal Liquid
        // =====================================================================

        // --- Chambers and Reservoirs (Isothermal Liquid) ---

        /// <summary>Constant-pressure hydraulic reservoir (return line).</summary>
        IsothermalLiquidReservoir,
        /// <summary>Fixed-volume chamber with fluid compressibility.</summary>
        IsothermalLiquidConstantVolumeHydraulicChamber,
        /// <summary>Variable-volume chamber driven by piston motion.</summary>
        IsothermalLiquidVariableVolumeHydraulicChamber,
        /// <summary>Pressurized accumulator with gas precharge (bladder/piston type).</summary>
        IsothermalLiquidAccumulator,
        /// <summary>Fluid inertia element for mass acceleration effects in a line.</summary>
        IsothermalLiquidFluidInertia,

        // --- Orifices and Flow Restrictions (Isothermal Liquid) ---

        /// <summary>Sharp-edge or round orifice with turbulent/laminar flow.</summary>
        IsothermalLiquidOrifice,
        /// <summary>Orifice with opening area controlled by a physical signal.</summary>
        IsothermalLiquidVariableOrifice,
        /// <summary>Fixed local flow restriction (fitting, elbow, tee).</summary>
        IsothermalLiquidLocalRestriction,

        // --- Pipes (Isothermal Liquid) ---

        /// <summary>Rigid circular pipe with viscous friction and compressibility.</summary>
        IsothermalLiquidPipeRigid,
        /// <summary>Flexible pipe with wall compliance and viscous friction.</summary>
        IsothermalLiquidPipeFlexible,
        /// <summary>Segmented pipeline for distributed-parameter modeling.</summary>
        IsothermalLiquidPipeSegmented,

        // --- Valves (Isothermal Liquid) ---

        /// <summary>Check valve preventing reverse flow.</summary>
        IsothermalLiquidCheckValve,
        /// <summary>Pilot-operated check valve with external unlock.</summary>
        IsothermalLiquidPilotOperatedCheckValve,
        /// <summary>Pressure relief valve opening at set cracking pressure.</summary>
        IsothermalLiquidPressureReliefValve,
        /// <summary>Pressure-reducing valve maintaining downstream set pressure.</summary>
        IsothermalLiquidPressureReducingValve,
        /// <summary>Counterbalance valve for load-holding in hydraulic cylinders.</summary>
        IsothermalLiquidCounterbalanceValve,
        /// <summary>2-way directional valve with signal-controlled spool.</summary>
        IsothermalLiquidDirectionalValve2Way,
        /// <summary>3-way directional valve with signal-controlled spool.</summary>
        IsothermalLiquidDirectionalValve3Way,
        /// <summary>4-way directional valve with signal-controlled spool.</summary>
        IsothermalLiquidDirectionalValve4Way,
        /// <summary>Flow control valve maintaining set flow rate.</summary>
        IsothermalLiquidFlowControlValve,
        /// <summary>Shuttle valve selecting higher-pressure supply.</summary>
        IsothermalLiquidShuttleValve,
        /// <summary>Ball valve with quarter-turn actuation.</summary>
        IsothermalLiquidBallValve,
        /// <summary>Gate valve with linear lift actuation.</summary>
        IsothermalLiquidGateValve,
        /// <summary>Needle valve for fine flow metering.</summary>
        IsothermalLiquidNeedleValve,
        /// <summary>Poppet valve with conical seat.</summary>
        IsothermalLiquidPoppetValve,
        /// <summary>Spool valve with overlapped/underlapped metering lands.</summary>
        IsothermalLiquidSpoolValve,

        // --- Pumps and Motors (Isothermal Liquid) ---

        /// <summary>Fixed-displacement pump driven by mechanical shaft.</summary>
        IsothermalLiquidFixedDisplacementPump,
        /// <summary>Variable-displacement pump with signal-controlled swashplate.</summary>
        IsothermalLiquidVariableDisplacementPump,
        /// <summary>Fixed-displacement hydraulic motor.</summary>
        IsothermalLiquidFixedDisplacementMotor,
        /// <summary>Variable-displacement hydraulic motor.</summary>
        IsothermalLiquidVariableDisplacementMotor,
        /// <summary>Centrifugal pump with head-flow characteristic.</summary>
        IsothermalLiquidCentrifugalPump,
        /// <summary>Jet pump (ejector) driven by motive fluid stream.</summary>
        IsothermalLiquidJetPump,

        // --- Actuators (Isothermal Liquid) ---

        /// <summary>Single-acting hydraulic cylinder.</summary>
        IsothermalLiquidSingleActingCylinder,
        /// <summary>Double-acting hydraulic cylinder.</summary>
        IsothermalLiquidDoubleActingCylinder,
        /// <summary>Rotary actuator converting hydraulic pressure to torque.</summary>
        IsothermalLiquidRotaryActuator,

        // --- Sources and Sensors (Isothermal Liquid) ---

        /// <summary>Ideal pressure source (isothermal liquid).</summary>
        IsothermalLiquidPressureSource,
        /// <summary>Ideal flow rate source (isothermal liquid).</summary>
        IsothermalLiquidFlowRateSource,
        /// <summary>Pressure sensor (isothermal liquid).</summary>
        IsothermalLiquidPressureSensor,
        /// <summary>Flow rate sensor (isothermal liquid).</summary>
        IsothermalLiquidFlowRateSensor,

        // --- References (Isothermal Liquid) ---

        /// <summary>Isothermal liquid reference (ground node).</summary>
        IsothermalLiquidReference,

        // =====================================================================
        // Thermal Liquid
        // =====================================================================

        // --- Chambers and Reservoirs (Thermal Liquid) ---

        /// <summary>Reservoir at fixed pressure and temperature.</summary>
        ThermalLiquidReservoir,
        /// <summary>Constant-volume chamber with thermal effects.</summary>
        ThermalLiquidConstantVolumeChamber,
        /// <summary>Variable-volume chamber with thermal effects.</summary>
        ThermalLiquidVariableVolumeChamber,
        /// <summary>Gas-charged accumulator with heat exchange.</summary>
        ThermalLiquidAccumulator,

        // --- Orifices and Flow Restrictions (Thermal Liquid) ---

        /// <summary>Orifice with thermal energy transport.</summary>
        ThermalLiquidOrifice,
        /// <summary>Variable orifice with thermal energy transport.</summary>
        ThermalLiquidVariableOrifice,
        /// <summary>Local restriction with thermal energy transport.</summary>
        ThermalLiquidLocalRestriction,

        // --- Pipes (Thermal Liquid) ---

        /// <summary>Pipe with wall heat transfer and viscous friction.</summary>
        ThermalLiquidPipe,

        // --- Valves (Thermal Liquid) ---

        /// <summary>Check valve with thermal energy transport.</summary>
        ThermalLiquidCheckValve,
        /// <summary>Pressure relief valve with thermal energy transport.</summary>
        ThermalLiquidPressureReliefValve,
        /// <summary>2-way directional valve with thermal energy transport.</summary>
        ThermalLiquidDirectionalValve2Way,
        /// <summary>4-way directional valve with thermal energy transport.</summary>
        ThermalLiquidDirectionalValve4Way,

        // --- Pumps and Motors (Thermal Liquid) ---

        /// <summary>Fixed-displacement pump with thermal effects.</summary>
        ThermalLiquidFixedDisplacementPump,
        /// <summary>Variable-displacement pump with thermal effects.</summary>
        ThermalLiquidVariableDisplacementPump,
        /// <summary>Centrifugal pump with thermal effects.</summary>
        ThermalLiquidCentrifugalPump,

        // --- Heat Exchangers (Thermal Liquid) ---

        /// <summary>Shell-and-tube heat exchanger (E-NTU method).</summary>
        ThermalLiquidShellAndTubeHeatExchanger,
        /// <summary>Plate heat exchanger (E-NTU method).</summary>
        ThermalLiquidPlateHeatExchanger,
        /// <summary>Fin-and-tube heat exchanger (radiator/condenser).</summary>
        ThermalLiquidFinAndTubeHeatExchanger,
        /// <summary>Generic heat exchanger with configurable effectiveness.</summary>
        ThermalLiquidGenericHeatExchanger,

        // --- Sources and Sensors (Thermal Liquid) ---

        /// <summary>Ideal pressure and temperature source (thermal liquid).</summary>
        ThermalLiquidPressureTemperatureSource,
        /// <summary>Ideal flow rate source (thermal liquid).</summary>
        ThermalLiquidFlowRateSource,
        /// <summary>Pressure and temperature sensor (thermal liquid).</summary>
        ThermalLiquidPressureTemperatureSensor,
        /// <summary>Flow rate sensor (thermal liquid).</summary>
        ThermalLiquidFlowRateSensor,

        // --- References (Thermal Liquid) ---

        /// <summary>Thermal liquid reference (ground node).</summary>
        ThermalLiquidReference,

        // =====================================================================
        // Gas (Pneumatic)
        // =====================================================================

        // --- Chambers and Reservoirs (Gas) ---

        /// <summary>Constant-volume gas chamber (pneumatic receiver).</summary>
        GasConstantVolumeChamber,
        /// <summary>Variable-volume gas chamber (pneumatic cylinder bore).</summary>
        GasVariableVolumeChamber,
        /// <summary>Infinite reservoir at fixed gas conditions.</summary>
        GasReservoir,

        // --- Orifices and Flow Restrictions (Gas) ---

        /// <summary>Gas orifice with choked/unchoked flow regimes.</summary>
        GasOrifice,
        /// <summary>Variable gas orifice with signal-controlled area.</summary>
        GasVariableOrifice,
        /// <summary>Local gas restriction (fitting, elbow).</summary>
        GasLocalRestriction,

        // --- Pipes (Gas) ---

        /// <summary>Gas pipe with friction, heat transfer, and compressibility.</summary>
        GasPipe,

        // --- Valves (Gas) ---

        /// <summary>Gas check valve preventing reverse flow.</summary>
        GasCheckValve,
        /// <summary>Gas pressure relief valve.</summary>
        GasPressureReliefValve,
        /// <summary>2-way gas directional valve.</summary>
        GasDirectionalValve2Way,
        /// <summary>3-way gas directional valve.</summary>
        GasDirectionalValve3Way,
        /// <summary>4-way gas directional valve.</summary>
        GasDirectionalValve4Way,

        // --- Actuators (Gas) ---

        /// <summary>Single-acting pneumatic cylinder.</summary>
        GasSingleActingCylinder,
        /// <summary>Double-acting pneumatic cylinder.</summary>
        GasDoubleActingCylinder,
        /// <summary>Pneumatic rotary actuator.</summary>
        GasRotaryActuator,

        // --- Turbomachinery (Gas) ---

        /// <summary>Gas compressor with polytropic efficiency map.</summary>
        GasCompressor,
        /// <summary>Gas turbine with efficiency map.</summary>
        GasTurbine,

        // --- Sources and Sensors (Gas) ---

        /// <summary>Ideal gas pressure and temperature source.</summary>
        GasPressureTemperatureSource,
        /// <summary>Ideal gas mass flow rate source.</summary>
        GasMassFlowRateSource,
        /// <summary>Gas pressure and temperature sensor.</summary>
        GasPressureTemperatureSensor,
        /// <summary>Gas mass flow rate sensor.</summary>
        GasMassFlowRateSensor,

        // --- References (Gas) ---

        /// <summary>Gas reference (ground node).</summary>
        GasReference,

        // =====================================================================
        // Two-Phase Fluid
        // =====================================================================

        // --- Chambers and Reservoirs (Two-Phase) ---

        /// <summary>Receiver tank for two-phase refrigerant.</summary>
        TwoPhaseFluidReceiverTank,
        /// <summary>Constant-volume chamber for two-phase fluid.</summary>
        TwoPhaseFluidConstantVolumeChamber,

        // --- Orifices (Two-Phase) ---

        /// <summary>Orifice for two-phase fluid (expansion valve).</summary>
        TwoPhaseFluidOrifice,
        /// <summary>Variable orifice for two-phase fluid (EEV).</summary>
        TwoPhaseFluidVariableOrifice,
        /// <summary>Thermostatic expansion valve (TXV).</summary>
        TwoPhaseFluidThermostaticExpansionValve,

        // --- Pipes (Two-Phase) ---

        /// <summary>Pipe with two-phase flow and heat transfer.</summary>
        TwoPhaseFluidPipe,

        // --- Heat Exchangers (Two-Phase) ---

        /// <summary>Condenser for two-phase fluid (vapor → liquid).</summary>
        TwoPhaseFluidCondenser,
        /// <summary>Evaporator for two-phase fluid (liquid → vapor).</summary>
        TwoPhaseFluidEvaporator,

        // --- Compressors (Two-Phase) ---

        /// <summary>Compressor for two-phase refrigerant cycle.</summary>
        TwoPhaseFluidCompressor,

        // --- Sources and Sensors (Two-Phase) ---

        /// <summary>Two-phase fluid pressure and temperature source.</summary>
        TwoPhaseFluidPressureTemperatureSource,
        /// <summary>Two-phase fluid mass flow rate source.</summary>
        TwoPhaseFluidMassFlowRateSource,
        /// <summary>Two-phase fluid sensor (pressure, temperature, quality).</summary>
        TwoPhaseFluidSensor,

        // --- References (Two-Phase) ---

        /// <summary>Two-phase fluid reference (ground node).</summary>
        TwoPhaseFluidReference,

        // =====================================================================
        // Moist Air
        // =====================================================================

        // --- Chambers and Reservoirs (Moist Air) ---

        /// <summary>Constant-volume chamber for moist air.</summary>
        MoistAirConstantVolumeChamber,
        /// <summary>Infinite moist air reservoir at fixed conditions.</summary>
        MoistAirReservoir,

        // --- Orifices (Moist Air) ---

        /// <summary>Moist air orifice.</summary>
        MoistAirOrifice,
        /// <summary>Moist air variable orifice.</summary>
        MoistAirVariableOrifice,

        // --- Pipes (Moist Air) ---

        /// <summary>Moist air pipe with friction and heat transfer.</summary>
        MoistAirPipe,

        // --- Heat Exchangers (Moist Air) ---

        /// <summary>Evaporator coil for moist air dehumidification.</summary>
        MoistAirEvaporatorCoil,
        /// <summary>Condenser coil for moist air heating.</summary>
        MoistAirCondenserCoil,
        /// <summary>Generic moist air heat exchanger.</summary>
        MoistAirHeatExchanger,

        // --- Sources and Sensors (Moist Air) ---

        /// <summary>Moist air pressure, temperature, and humidity source.</summary>
        MoistAirSource,
        /// <summary>Moist air sensor (pressure, temperature, relative humidity).</summary>
        MoistAirSensor,
        /// <summary>Moist air mass flow rate sensor.</summary>
        MoistAirMassFlowRateSensor,

        // --- References (Moist Air) ---

        /// <summary>Moist air reference (ground node).</summary>
        MoistAirReference,

        // =====================================================================
        // Fluid Properties and Utilities
        // =====================================================================

        /// <summary>Fluid properties block defining thermodynamic tables.</summary>
        FluidPropertiesBlock,
        /// <summary>Solver configuration for fluid network.</summary>
        FluidsSolverConfiguration,
        /// <summary>Simscape connection port for fluid domain.</summary>
        FluidsConnectionPort
    }

    #endregion
}
