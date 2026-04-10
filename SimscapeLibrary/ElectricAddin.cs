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

    /// <summary>
    /// All blocks available in the Simscape Electrical library.
    /// Covers passive elements, sources, semiconductors, power converters,
    /// electric machines, sensors, and power system components.
    /// </summary>
    public enum ElectricalBlockType
    {
        // =====================================================================
        // Passive Elements
        // =====================================================================

        /// <summary>Ideal resistor (R).</summary>
        Resistor,
        /// <summary>Ideal capacitor (C).</summary>
        Capacitor,
        /// <summary>Ideal inductor (L).</summary>
        Inductor,
        /// <summary>Variable resistor controlled by a physical signal.</summary>
        VariableResistor,
        /// <summary>Variable capacitor controlled by a physical signal.</summary>
        VariableCapacitor,
        /// <summary>Variable inductor controlled by a physical signal.</summary>
        VariableInductor,
        /// <summary>Mutual inductor (magnetically coupled coils).</summary>
        MutualInductor,
        /// <summary>Two-winding ideal transformer.</summary>
        IdealTransformer,
        /// <summary>Nonlinear inductor with saturable core.</summary>
        SaturableTransformer,
        /// <summary>Three-winding transformer.</summary>
        ThreeWindingTransformer,
        /// <summary>Zigzag-connected three-phase transformer.</summary>
        ZigzagTransformer,
        /// <summary>Memristor (memory resistor) element.</summary>
        Memristor,
        /// <summary>Gyrator — converts voltage to current and vice versa.</summary>
        Gyrator,
        /// <summary>Fuse element that opens at rated current.</summary>
        Fuse,

        // =====================================================================
        // Voltage and Current Sources
        // =====================================================================

        /// <summary>DC voltage source.</summary>
        DCVoltageSource,
        /// <summary>AC voltage source (single-phase sinusoidal).</summary>
        ACVoltageSource,
        /// <summary>DC current source.</summary>
        DCCurrentSource,
        /// <summary>AC current source (single-phase sinusoidal).</summary>
        ACCurrentSource,
        /// <summary>Controlled voltage source (signal-driven).</summary>
        ControlledVoltageSource,
        /// <summary>Controlled current source (signal-driven).</summary>
        ControlledCurrentSource,
        /// <summary>Voltage-controlled voltage source (VCVS).</summary>
        VoltageControlledVoltageSource,
        /// <summary>Voltage-controlled current source (VCCS).</summary>
        VoltageControlledCurrentSource,
        /// <summary>Current-controlled voltage source (CCVS).</summary>
        CurrentControlledVoltageSource,
        /// <summary>Current-controlled current source (CCCS).</summary>
        CurrentControlledCurrentSource,
        /// <summary>Three-phase voltage source (balanced).</summary>
        ThreePhaseVoltageSource,
        /// <summary>Three-phase current source (balanced).</summary>
        ThreePhaseCurrentSource,
        /// <summary>Programmable voltage source with waveform table.</summary>
        ProgrammableVoltageSource,
        /// <summary>Programmable current source with waveform table.</summary>
        ProgrammableCurrentSource,
        /// <summary>Pulse-width modulated (PWM) voltage source.</summary>
        PwmVoltageSource,

        // =====================================================================
        // Batteries and Fuel Cells
        // =====================================================================

        /// <summary>Battery cell (generic equivalent circuit model).</summary>
        Battery,
        /// <summary>Battery cell with Thevenin equivalent RC branches.</summary>
        BatteryThevenin,
        /// <summary>Lithium-ion battery cell (table-based SOC model).</summary>
        BatteryLithiumIon,
        /// <summary>Lead-acid battery cell model.</summary>
        BatteryLeadAcid,
        /// <summary>Nickel-metal hydride battery cell model.</summary>
        BatteryNiMH,
        /// <summary>Battery pack (series/parallel cell assembly).</summary>
        BatteryPack,
        /// <summary>Battery management system (charge balancing and SOC).</summary>
        BatteryManagementSystem,
        /// <summary>PEM fuel cell stack model.</summary>
        FuelCell,
        /// <summary>Supercapacitor (electric double-layer capacitor).</summary>
        Supercapacitor,

        // =====================================================================
        // Semiconductors — Diodes
        // =====================================================================

        /// <summary>Ideal diode (no forward voltage drop).</summary>
        IdealDiode,
        /// <summary>Piecewise-linear diode with forward voltage.</summary>
        PiecewiseLinearDiode,
        /// <summary>Exponential diode (Shockley model).</summary>
        ExponentialDiode,
        /// <summary>Zener diode with reverse breakdown.</summary>
        ZenerDiode,
        /// <summary>Schottky diode (low forward drop).</summary>
        SchottkyDiode,
        /// <summary>Light-emitting diode (LED).</summary>
        LED,
        /// <summary>Tunnel diode with negative resistance region.</summary>
        TunnelDiode,
        /// <summary>Varactor diode (voltage-controlled capacitance).</summary>
        VaractorDiode,
        /// <summary>TVS (transient voltage suppressor) diode.</summary>
        TVSDiode,

        // =====================================================================
        // Semiconductors — Switches
        // =====================================================================

        /// <summary>Ideal switch (no losses).</summary>
        IdealSwitch,
        /// <summary>N-channel MOSFET with on-resistance model.</summary>
        NChannelMOSFET,
        /// <summary>P-channel MOSFET with on-resistance model.</summary>
        PChannelMOSFET,
        /// <summary>IGBT with collector-emitter saturation model.</summary>
        IGBT,
        /// <summary>GTO thyristor with turn-off capability.</summary>
        GTO,
        /// <summary>Thyristor (SCR) with gate trigger.</summary>
        Thyristor,
        /// <summary>TRIAC bidirectional thyristor.</summary>
        TRIAC,
        /// <summary>NPN bipolar junction transistor.</summary>
        NPN_BJT,
        /// <summary>PNP bipolar junction transistor.</summary>
        PNP_BJT,
        /// <summary>N-channel JFET.</summary>
        NChannelJFET,
        /// <summary>P-channel JFET.</summary>
        PChannelJFET,
        /// <summary>SiC (silicon carbide) MOSFET.</summary>
        SiCMOSFET,
        /// <summary>GaN (gallium nitride) HEMT.</summary>
        GaNHEMT,

        // =====================================================================
        // Op-Amps and Analog ICs
        // =====================================================================

        /// <summary>Ideal operational amplifier (infinite gain, zero output impedance).</summary>
        IdealOpAmp,
        /// <summary>Finite-gain operational amplifier with bandwidth model.</summary>
        FiniteGainOpAmp,
        /// <summary>Comparator (voltage threshold output).</summary>
        Comparator,
        /// <summary>555 timer IC model.</summary>
        Timer555,
        /// <summary>Sample-and-hold circuit.</summary>
        SampleAndHold,

        // =====================================================================
        // Power Converters
        // =====================================================================

        // --- DC-DC Converters ---

        /// <summary>Buck (step-down) converter.</summary>
        BuckConverter,
        /// <summary>Boost (step-up) converter.</summary>
        BoostConverter,
        /// <summary>Buck-boost converter.</summary>
        BuckBoostConverter,
        /// <summary>Flyback converter (isolated).</summary>
        FlybackConverter,
        /// <summary>Forward converter (isolated).</summary>
        ForwardConverter,
        /// <summary>Push-pull converter (isolated).</summary>
        PushPullConverter,
        /// <summary>Half-bridge DC-DC converter.</summary>
        HalfBridgeDCDCConverter,
        /// <summary>Full-bridge DC-DC converter.</summary>
        FullBridgeDCDCConverter,
        /// <summary>Interleaved boost converter.</summary>
        InterleavedBoostConverter,

        // --- AC-DC Converters (Rectifiers) ---

        /// <summary>Single-phase diode bridge rectifier.</summary>
        SinglePhaseDiodeBridgeRectifier,
        /// <summary>Three-phase diode bridge rectifier.</summary>
        ThreePhaseDiodeBridgeRectifier,
        /// <summary>Single-phase thyristor bridge rectifier.</summary>
        SinglePhaseThyristorRectifier,
        /// <summary>Three-phase thyristor bridge rectifier.</summary>
        ThreePhaseThyristorRectifier,
        /// <summary>Vienna rectifier (three-level boost PFC).</summary>
        ViennaRectifier,
        /// <summary>Active front end rectifier (PWM controlled).</summary>
        ActiveFrontEndRectifier,

        // --- DC-AC Converters (Inverters) ---

        /// <summary>Single-phase full-bridge inverter.</summary>
        SinglePhaseInverter,
        /// <summary>Three-phase two-level inverter.</summary>
        ThreePhaseInverterTwoLevel,
        /// <summary>Three-phase three-level NPC inverter.</summary>
        ThreePhaseInverterThreeLevelNPC,
        /// <summary>Three-phase T-type inverter.</summary>
        ThreePhaseInverterTType,
        /// <summary>Cascaded H-bridge multilevel inverter.</summary>
        CascadedHBridgeInverter,

        // --- AC-AC Converters ---

        /// <summary>Single-phase AC voltage controller (phase control).</summary>
        SinglePhaseACVoltageController,
        /// <summary>Three-phase AC voltage controller.</summary>
        ThreePhaseACVoltageController,
        /// <summary>Cycloconverter (direct frequency converter).</summary>
        Cycloconverter,
        /// <summary>Matrix converter (direct AC-AC).</summary>
        MatrixConverter,

        // =====================================================================
        // Electric Machines — DC
        // =====================================================================

        /// <summary>Separately excited DC motor.</summary>
        SeparatelyExcitedDCMotor,
        /// <summary>Series-wound DC motor.</summary>
        SeriesDCMotor,
        /// <summary>Shunt-wound DC motor.</summary>
        ShuntDCMotor,
        /// <summary>Compound DC motor (cumulative/differential).</summary>
        CompoundDCMotor,
        /// <summary>Permanent-magnet DC motor (brushed).</summary>
        PermanentMagnetDCMotor,
        /// <summary>Universal (series AC/DC) motor.</summary>
        UniversalMotor,

        // =====================================================================
        // Electric Machines — AC Induction
        // =====================================================================

        /// <summary>Three-phase squirrel-cage induction motor.</summary>
        SquirrelCageInductionMotor,
        /// <summary>Three-phase wound-rotor induction motor.</summary>
        WoundRotorInductionMotor,
        /// <summary>Single-phase induction motor (split-phase).</summary>
        SinglePhaseInductionMotor,
        /// <summary>Doubly-fed induction generator (DFIG).</summary>
        DoublyFedInductionGenerator,

        // =====================================================================
        // Electric Machines — Synchronous
        // =====================================================================

        /// <summary>Interior permanent-magnet synchronous motor (IPMSM).</summary>
        InteriorPMSM,
        /// <summary>Surface-mounted permanent-magnet synchronous motor (SPMSM).</summary>
        SurfacePMSM,
        /// <summary>Wound-field synchronous machine.</summary>
        WoundFieldSynchronousMachine,
        /// <summary>Salient-pole synchronous generator.</summary>
        SalientPoleSynchronousGenerator,
        /// <summary>Brushless DC (BLDC) motor — trapezoidal back-EMF.</summary>
        BrushlessDCMotor,

        // =====================================================================
        // Electric Machines — Reluctance and Stepper
        // =====================================================================

        /// <summary>Switched reluctance motor (SRM).</summary>
        SwitchedReluctanceMotor,
        /// <summary>Synchronous reluctance motor (SynRM).</summary>
        SynchronousReluctanceMotor,
        /// <summary>Hybrid stepper motor.</summary>
        HybridStepperMotor,
        /// <summary>Variable-reluctance stepper motor.</summary>
        VariableReluctanceStepperMotor,
        /// <summary>Permanent-magnet stepper motor.</summary>
        PermanentMagnetStepperMotor,

        // =====================================================================
        // Electric Machines — Linear
        // =====================================================================

        /// <summary>Linear permanent-magnet synchronous motor.</summary>
        LinearPMSM,
        /// <summary>Linear induction motor.</summary>
        LinearInductionMotor,
        /// <summary>Voice coil actuator.</summary>
        VoiceCoilActuator,

        // =====================================================================
        // Electric Machine Peripherals
        // =====================================================================

        /// <summary>Resolver sensor for rotor position measurement.</summary>
        Resolver,
        /// <summary>Incremental encoder for speed/position measurement.</summary>
        IncrementalEncoder,
        /// <summary>Hall-effect position sensor.</summary>
        HallEffectSensor,
        /// <summary>PWM gate driver for power switches.</summary>
        GateDriver,
        /// <summary>Space-vector PWM modulator (SVPWM).</summary>
        SpaceVectorModulator,
        /// <summary>Sinusoidal PWM modulator.</summary>
        SinusoidalPWMModulator,
        /// <summary>Clarke transform (abc → αβ).</summary>
        ClarkeTransform,
        /// <summary>Park transform (αβ → dq).</summary>
        ParkTransform,
        /// <summary>Inverse Park transform (dq → αβ).</summary>
        InverseParkTransform,
        /// <summary>Inverse Clarke transform (αβ → abc).</summary>
        InverseClarkeTransform,

        // =====================================================================
        // Magnetic Elements
        // =====================================================================

        /// <summary>Reluctance element (magnetic resistance).</summary>
        Reluctance,
        /// <summary>Permanent magnet (MMF source).</summary>
        PermanentMagnet,
        /// <summary>Electromagnetic converter (electromechanical coupling).</summary>
        ElectromagneticConverter,
        /// <summary>Flux concentrator / core with saturation.</summary>
        SaturableMagneticCore,
        /// <summary>Magnetic air gap.</summary>
        MagneticAirGap,
        /// <summary>Magnetic reference (ground).</summary>
        MagneticReference,

        // =====================================================================
        // Power Systems — Transmission and Distribution
        // =====================================================================

        /// <summary>Pi-section transmission line model.</summary>
        PiSectionTransmissionLine,
        /// <summary>Distributed-parameter transmission line.</summary>
        DistributedParameterLine,
        /// <summary>Three-phase pi-section line.</summary>
        ThreePhasePiSectionLine,
        /// <summary>Circuit breaker (single-phase).</summary>
        CircuitBreaker,
        /// <summary>Three-phase circuit breaker.</summary>
        ThreePhaseCircuitBreaker,
        /// <summary>Surge arrester (lightning/overvoltage protection).</summary>
        SurgeArrester,
        /// <summary>Fault block for simulating short-circuit conditions.</summary>
        FaultBlock,
        /// <summary>Three-phase fault block.</summary>
        ThreePhaseFaultBlock,
        /// <summary>Current transformer (CT).</summary>
        CurrentTransformer,
        /// <summary>Potential transformer (PT/VT).</summary>
        PotentialTransformer,

        // =====================================================================
        // Power Systems — Renewable Energy
        // =====================================================================

        /// <summary>Solar cell (photovoltaic) single-diode model.</summary>
        SolarCell,
        /// <summary>PV array (series/parallel solar cell assembly).</summary>
        PVArray,
        /// <summary>PV module with MPPT interface.</summary>
        PVModule,
        /// <summary>Wind turbine generator (DFIG-based).</summary>
        WindTurbineDFIG,
        /// <summary>Wind turbine generator (PMSG-based, direct-drive).</summary>
        WindTurbinePMSG,

        // =====================================================================
        // Sensors and Measurement
        // =====================================================================

        /// <summary>Voltage sensor (across measurement).</summary>
        VoltageSensor,
        /// <summary>Current sensor (through measurement).</summary>
        CurrentSensor,
        /// <summary>Power sensor (instantaneous P and Q).</summary>
        PowerSensor,
        /// <summary>Impedance sensor.</summary>
        ImpedanceSensor,
        /// <summary>RMS voltage sensor.</summary>
        RMSVoltageSensor,
        /// <summary>RMS current sensor.</summary>
        RMSCurrentSensor,
        /// <summary>Three-phase voltage sensor.</summary>
        ThreePhaseVoltageSensor,
        /// <summary>Three-phase current sensor.</summary>
        ThreePhaseCurrentSensor,
        /// <summary>Three-phase instantaneous power sensor.</summary>
        ThreePhasePowerSensor,
        /// <summary>PLL (phase-locked loop) for grid synchronization.</summary>
        PhaseLocked_Loop,
        /// <summary>THD analyzer (total harmonic distortion measurement).</summary>
        THDAnalyzer,

        // =====================================================================
        // References and Grounding
        // =====================================================================

        /// <summary>Electrical reference (ground node).</summary>
        ElectricalReference,
        /// <summary>Neutral point connection.</summary>
        NeutralPoint,
        /// <summary>Open circuit terminator.</summary>
        OpenCircuit,

        // =====================================================================
        // Protection Devices
        // =====================================================================

        /// <summary>Overcurrent relay (inverse-time characteristic).</summary>
        OvercurrentRelay,
        /// <summary>Undervoltage relay.</summary>
        UndervoltageRelay,
        /// <summary>Overvoltage relay.</summary>
        OvervoltageRelay,
        /// <summary>Differential relay (transformer/generator protection).</summary>
        DifferentialRelay,
        /// <summary>Ground fault circuit interrupter (GFCI/RCD).</summary>
        GroundFaultInterrupter,

        // =====================================================================
        // Connectors and Utilities
        // =====================================================================

        /// <summary>Solver configuration for electrical networks.</summary>
        ElectricalSolverConfiguration,
        /// <summary>PS-Simulink converter.</summary>
        PSSimulinkConverter,
        /// <summary>Simulink-PS converter.</summary>
        SimulinkPSConverter,
        /// <summary>Three-phase connection port.</summary>
        ThreePhaseConnectionPort,
        /// <summary>Composite electrical connection port.</summary>
        ElectricalConnectionPort
    }

    #endregion
}
