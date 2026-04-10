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

    /// <summary>
    /// All blocks available in the base Simscape (Foundation) library.
    /// Covers foundation domain elements, physical signal operations, sources, sensors, and utilities.
    /// </summary>
    public enum SimscapeBlockType
    {
        // =====================================================================
        // Electrical Domain (Foundation)
        // =====================================================================

        /// <summary>Ideal resistor.</summary>
        ElectricalResistor,
        /// <summary>Ideal capacitor.</summary>
        ElectricalCapacitor,
        /// <summary>Ideal inductor.</summary>
        ElectricalInductor,
        /// <summary>Variable resistor controlled by a physical signal.</summary>
        ElectricalVariableResistor,
        /// <summary>Variable capacitor controlled by a physical signal.</summary>
        ElectricalVariableCapacitor,
        /// <summary>Variable inductor controlled by a physical signal.</summary>
        ElectricalVariableInductor,
        /// <summary>Mutual inductor (magnetically coupled windings).</summary>
        ElectricalMutualInductor,
        /// <summary>Ideal transformer with turns ratio.</summary>
        ElectricalIdealTransformer,
        /// <summary>Ideal operational amplifier.</summary>
        ElectricalOpAmp,
        /// <summary>Piecewise-linear diode.</summary>
        ElectricalDiode,
        /// <summary>Ideal switch (open/closed).</summary>
        ElectricalSwitch,
        /// <summary>DC voltage source.</summary>
        ElectricalDCVoltageSource,
        /// <summary>DC current source.</summary>
        ElectricalDCCurrentSource,
        /// <summary>AC voltage source (single-phase sinusoidal).</summary>
        ElectricalACVoltageSource,
        /// <summary>AC current source (single-phase sinusoidal).</summary>
        ElectricalACCurrentSource,
        /// <summary>Controlled voltage source driven by a physical signal.</summary>
        ElectricalControlledVoltageSource,
        /// <summary>Controlled current source driven by a physical signal.</summary>
        ElectricalControlledCurrentSource,
        /// <summary>Voltage sensor (across measurement).</summary>
        ElectricalVoltageSensor,
        /// <summary>Current sensor (through measurement).</summary>
        ElectricalCurrentSensor,
        /// <summary>Electrical reference (ground node).</summary>
        ElectricalReference,

        // =====================================================================
        // Mechanical Rotational Domain (Foundation)
        // =====================================================================

        /// <summary>Rotational inertia element.</summary>
        RotationalInertia,
        /// <summary>Ideal rotational spring (torsional compliance).</summary>
        RotationalSpring,
        /// <summary>Ideal rotational damper (torsional viscous friction).</summary>
        RotationalDamper,
        /// <summary>Rotational spring-damper combined element.</summary>
        RotationalSpringDamper,
        /// <summary>Rotational friction with Coulomb, viscous, and Stribeck effects.</summary>
        RotationalFriction,
        /// <summary>Hard stop limiting rotational travel between two angular bounds.</summary>
        RotationalHardStop,
        /// <summary>Rotational free end (unconnected terminator).</summary>
        RotationalFreeEnd,
        /// <summary>Ideal torque source controlled by a physical signal.</summary>
        IdealTorqueSource,
        /// <summary>Ideal angular velocity source controlled by a physical signal.</summary>
        IdealAngularVelocitySource,
        /// <summary>Torque sensor measuring through-variable.</summary>
        TorqueSensor,
        /// <summary>Angular velocity sensor measuring across-variable.</summary>
        AngularVelocitySensor,
        /// <summary>Rotational motion sensor (angle, velocity, acceleration).</summary>
        RotationalMotionSensor,
        /// <summary>Simple gear pair with fixed ratio.</summary>
        SimpleGear,
        /// <summary>Lever converting rotational to translational motion.</summary>
        WheelAndAxle,
        /// <summary>Rotational coupling with backlash.</summary>
        RotationalBacklash,
        /// <summary>Mechanical rotational reference (ground frame).</summary>
        MechanicalRotationalReference,

        // =====================================================================
        // Mechanical Translational Domain (Foundation)
        // =====================================================================

        /// <summary>Translational mass element.</summary>
        TranslationalMass,
        /// <summary>Ideal translational spring.</summary>
        TranslationalSpring,
        /// <summary>Ideal translational damper.</summary>
        TranslationalDamper,
        /// <summary>Translational spring-damper combined element.</summary>
        TranslationalSpringDamper,
        /// <summary>Translational friction with Coulomb, viscous, and Stribeck effects.</summary>
        TranslationalFriction,
        /// <summary>Hard stop limiting translational travel between two linear bounds.</summary>
        TranslationalHardStop,
        /// <summary>Translational free end (unconnected terminator).</summary>
        TranslationalFreeEnd,
        /// <summary>Ideal force source controlled by a physical signal.</summary>
        IdealForceSource,
        /// <summary>Ideal translational velocity source controlled by a physical signal.</summary>
        IdealTranslationalVelocitySource,
        /// <summary>Force sensor measuring through-variable.</summary>
        ForceSensor,
        /// <summary>Translational velocity sensor measuring across-variable.</summary>
        TranslationalVelocitySensor,
        /// <summary>Translational motion sensor (position, velocity, acceleration).</summary>
        TranslationalMotionSensor,
        /// <summary>Translational coupling with backlash.</summary>
        TranslationalBacklash,
        /// <summary>Mechanical translational reference (ground frame).</summary>
        MechanicalTranslationalReference,

        // =====================================================================
        // Hydraulic Domain (Foundation)
        // =====================================================================

        /// <summary>Constant-volume hydraulic chamber with fluid compressibility.</summary>
        HydraulicConstantVolumeChamber,
        /// <summary>Variable-volume hydraulic chamber driven by piston motion.</summary>
        HydraulicVariableVolumeChamber,
        /// <summary>Hydraulic resistive tube with viscous friction.</summary>
        HydraulicResistiveTube,
        /// <summary>Fixed hydraulic orifice (turbulent/laminar).</summary>
        HydraulicOrifice,
        /// <summary>Variable hydraulic orifice controlled by a physical signal.</summary>
        HydraulicVariableOrifice,
        /// <summary>Double-acting hydraulic cylinder.</summary>
        HydraulicDoubleActingCylinder,
        /// <summary>Single-acting hydraulic cylinder.</summary>
        HydraulicSingleActingCylinder,
        /// <summary>Hydraulic piston chamber (pressure-to-force conversion).</summary>
        HydraulicPistonChamber,
        /// <summary>Hydraulic check valve preventing reverse flow.</summary>
        HydraulicCheckValve,
        /// <summary>Hydraulic pressure relief valve.</summary>
        HydraulicPressureReliefValve,
        /// <summary>Hydraulic 4-way directional valve.</summary>
        HydraulicDirectionalValve4Way,
        /// <summary>Hydraulic pressure-compensated flow control valve.</summary>
        HydraulicFlowControlValve,
        /// <summary>Hydraulic fluid inertia element.</summary>
        HydraulicFluidInertia,
        /// <summary>Ideal hydraulic pressure source.</summary>
        HydraulicIdealPressureSource,
        /// <summary>Ideal hydraulic flow rate source.</summary>
        HydraulicIdealFlowRateSource,
        /// <summary>Hydraulic pressure sensor.</summary>
        HydraulicPressureSensor,
        /// <summary>Hydraulic flow rate sensor.</summary>
        HydraulicFlowRateSensor,
        /// <summary>Hydraulic reference (ground node).</summary>
        HydraulicReference,

        // =====================================================================
        // Thermal Domain (Foundation)
        // =====================================================================

        /// <summary>Conductive heat transfer element (Fourier's law).</summary>
        ThermalConductiveHeatTransfer,
        /// <summary>Convective heat transfer element (Newton's law of cooling).</summary>
        ThermalConvectiveHeatTransfer,
        /// <summary>Radiative heat transfer element (Stefan-Boltzmann law).</summary>
        ThermalRadiativeHeatTransfer,
        /// <summary>Thermal mass (heat capacity) element.</summary>
        ThermalMass,
        /// <summary>Perfect insulator (zero heat flow).</summary>
        ThermalPerfectInsulator,
        /// <summary>Variable thermal conductance controlled by a physical signal.</summary>
        ThermalVariableConductor,
        /// <summary>Ideal temperature source.</summary>
        ThermalIdealTemperatureSource,
        /// <summary>Ideal heat flow source.</summary>
        ThermalIdealHeatFlowSource,
        /// <summary>Controlled temperature source driven by a physical signal.</summary>
        ThermalControlledTemperatureSource,
        /// <summary>Controlled heat flow source driven by a physical signal.</summary>
        ThermalControlledHeatFlowSource,
        /// <summary>Temperature sensor.</summary>
        ThermalTemperatureSensor,
        /// <summary>Heat flow sensor.</summary>
        ThermalHeatFlowSensor,
        /// <summary>Absolute temperature sensor (single-port).</summary>
        ThermalAbsoluteTemperatureSensor,
        /// <summary>Thermal reference (absolute zero).</summary>
        ThermalReference,

        // =====================================================================
        // Magnetic Domain (Foundation)
        // =====================================================================

        /// <summary>Reluctance element (magnetic resistance).</summary>
        MagneticReluctance,
        /// <summary>Fundamental reluctance with saturation and hysteresis.</summary>
        MagneticFundamentalReluctance,
        /// <summary>Permanent magnet (MMF source).</summary>
        MagneticPermanentMagnet,
        /// <summary>Electromagnetic converter (electromechanical coupling).</summary>
        MagneticElectromagneticConverter,
        /// <summary>Magnetic flux sensor.</summary>
        MagneticFluxSensor,
        /// <summary>MMF sensor (magnetomotive force).</summary>
        MagneticMMFSensor,
        /// <summary>Ideal MMF source.</summary>
        MagneticIdealMMFSource,
        /// <summary>Ideal magnetic flux source.</summary>
        MagneticIdealFluxSource,
        /// <summary>Magnetic reference (ground node).</summary>
        MagneticReference,

        // =====================================================================
        // Physical Signal Operations — Math
        // =====================================================================

        /// <summary>Adds two physical signals.</summary>
        PSAdd,
        /// <summary>Subtracts one physical signal from another.</summary>
        PSSubtract,
        /// <summary>Multiplies two physical signals.</summary>
        PSMultiply,
        /// <summary>Divides one physical signal by another.</summary>
        PSDivide,
        /// <summary>Applies a constant gain to a physical signal.</summary>
        PSGain,
        /// <summary>Computes the absolute value of a physical signal.</summary>
        PSAbs,
        /// <summary>Outputs the sign (−1, 0, +1) of a physical signal.</summary>
        PSSign,
        /// <summary>Outputs the minimum of two physical signals.</summary>
        PSMin,
        /// <summary>Outputs the maximum of two physical signals.</summary>
        PSMax,
        /// <summary>Computes the square root of a physical signal.</summary>
        PSSqrt,
        /// <summary>Raises a physical signal to a power.</summary>
        PSPower,
        /// <summary>Applies a mathematical function (exp, log, log10, etc.).</summary>
        PSMathFunction,
        /// <summary>Applies a trigonometric function (sin, cos, tan, etc.).</summary>
        PSTrigonometricFunction,
        /// <summary>Computes the modulo (remainder) of two physical signals.</summary>
        PSModulo,

        // =====================================================================
        // Physical Signal Operations — Rounding
        // =====================================================================

        /// <summary>Rounds a physical signal toward positive infinity.</summary>
        PSCeil,
        /// <summary>Rounds a physical signal toward negative infinity.</summary>
        PSFloor,
        /// <summary>Rounds a physical signal to the nearest integer.</summary>
        PSRound,
        /// <summary>Rounds a physical signal toward zero.</summary>
        PSFix,

        // =====================================================================
        // Physical Signal Operations — Logic and Comparison
        // =====================================================================

        /// <summary>Outputs 1 if input A > input B, else 0.</summary>
        PSGreaterThan,
        /// <summary>Outputs 1 if input A >= input B, else 0.</summary>
        PSGreaterThanOrEqual,
        /// <summary>Outputs 1 if input A &lt; input B, else 0.</summary>
        PSLessThan,
        /// <summary>Outputs 1 if input A &lt;= input B, else 0.</summary>
        PSLessThanOrEqual,
        /// <summary>Outputs 1 if input A equals input B within tolerance, else 0.</summary>
        PSEqual,
        /// <summary>Logical AND of two physical signals.</summary>
        PSLogicalAND,
        /// <summary>Logical OR of two physical signals.</summary>
        PSLogicalOR,
        /// <summary>Logical NOT of a physical signal.</summary>
        PSLogicalNOT,

        // =====================================================================
        // Physical Signal Operations — Dynamic
        // =====================================================================

        /// <summary>Integrates a physical signal over time.</summary>
        PSIntegrator,
        /// <summary>Computes the time derivative (rate of change) of a physical signal.</summary>
        PSDerivative,
        /// <summary>Transfer function in the Laplace domain.</summary>
        PSTransferFunction,
        /// <summary>First-order filter (low-pass).</summary>
        PSFirstOrderFilter,
        /// <summary>Second-order filter (bandpass, low-pass, etc.).</summary>
        PSSecondOrderFilter,
        /// <summary>Time delay element for a physical signal.</summary>
        PSDelay,
        /// <summary>Unit delay (sample-and-hold for one time step).</summary>
        PSUnitDelay,
        /// <summary>Zero-order hold (continuous-to-discrete).</summary>
        PSZeroOrderHold,
        /// <summary>State-space model for physical signals.</summary>
        PSStateSpace,
        /// <summary>PID controller in the physical signal domain.</summary>
        PSPID,

        // =====================================================================
        // Physical Signal Operations — Lookup and Nonlinear
        // =====================================================================

        /// <summary>1-D lookup table (interpolation from breakpoint data).</summary>
        PSLookupTable1D,
        /// <summary>2-D lookup table (interpolation from two-axis breakpoint data).</summary>
        PSLookupTable2D,
        /// <summary>Saturates a physical signal between upper and lower bounds.</summary>
        PSSaturation,
        /// <summary>Dead zone that outputs zero within a band around zero.</summary>
        PSDeadZone,
        /// <summary>Rate limiter constraining the derivative of a physical signal.</summary>
        PSRateLimiter,
        /// <summary>Hysteresis relay for physical signals.</summary>
        PSHysteresis,

        // =====================================================================
        // Physical Signal Operations — Routing
        // =====================================================================

        /// <summary>Switch selecting between two physical signals based on a threshold.</summary>
        PSSwitch,
        /// <summary>Multiport switch selecting from N physical signals by index.</summary>
        PSMultiportSwitch,
        /// <summary>Selects a single element from a vector physical signal.</summary>
        PSSelector,
        /// <summary>Multiplexes scalar physical signals into a vector.</summary>
        PSMux,
        /// <summary>Demultiplexes a vector physical signal into scalars.</summary>
        PSDemux,
        /// <summary>Terminates an unconnected physical signal.</summary>
        PSTerminator,
        /// <summary>Goto tag for physical signal routing.</summary>
        PSGoto,
        /// <summary>From tag receiving a physical signal from a Goto block.</summary>
        PSFrom,

        // =====================================================================
        // Physical Signal Sources
        // =====================================================================

        /// <summary>Constant physical signal output.</summary>
        PSConstant,
        /// <summary>Ramp signal (linearly increasing/decreasing).</summary>
        PSRamp,
        /// <summary>Step signal (instantaneous level change).</summary>
        PSStep,
        /// <summary>Sine wave physical signal.</summary>
        PSSineWave,
        /// <summary>Square wave physical signal.</summary>
        PSSquareWave,
        /// <summary>Periodic pulse physical signal.</summary>
        PSPulse,
        /// <summary>Sawtooth wave physical signal.</summary>
        PSSawtooth,
        /// <summary>Random number physical signal (uniform or Gaussian).</summary>
        PSRandomNumber,
        /// <summary>Chirp signal (swept-frequency sine).</summary>
        PSChirp,
        /// <summary>Signal defined by a MATLAB timeseries or workspace data.</summary>
        PSSignalBuilder,
        /// <summary>Repeating sequence interpolated from a table of time-value pairs.</summary>
        PSRepeatingSequence,

        // =====================================================================
        // Utilities and Simulation Configuration
        // =====================================================================

        /// <summary>Solver configuration for Simscape physical networks.</summary>
        SolverConfiguration,
        /// <summary>Converts a physical signal to a Simulink output signal.</summary>
        PSSimulinkConverter,
        /// <summary>Converts a Simulink input signal to a physical signal.</summary>
        SimulinkPSConverter,
        /// <summary>Connection port for Simscape subsystem boundaries.</summary>
        ConnectionPort,
        /// <summary>Physical signal connection port for subsystem boundaries.</summary>
        PSConnectionPort,
        /// <summary>Probe block for logging physical signals without affecting simulation.</summary>
        Probe,
        /// <summary>Spectrum analyzer for frequency-domain visualization.</summary>
        SpectrumAnalyzer,
        /// <summary>Scope for time-domain physical signal visualization.</summary>
        SimscapeScope,
        /// <summary>Component reference block for reusable Simscape components.</summary>
        ComponentReference,
        /// <summary>Simscape subsystem containing a physical network.</summary>
        SimscapeSubsystem,
        /// <summary>Domain-specific through/across variable display.</summary>
        PhysicalSignalProbe,
        /// <summary>Unit conversion block for physical signals.</summary>
        PSUnitConversion
    }

    #endregion
}
