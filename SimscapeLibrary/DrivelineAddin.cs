using System;
using System.Collections.Generic;
using Simulation;

namespace CommunityMaker.Simscape
{
    /// <summary>
    /// Simscape Driveline add-in for modeling rotational and translational powertrain systems.
    /// </summary>
    public sealed class DrivelineAddin : SimscapeAddin
    {
        #region Properties

        // Powertrain components
        public List<DrivelineGear> Gears { get; set; } = [];
        public List<DrivelineClutch> Clutches { get; set; } = [];
        public List<DrivelineShaft> Shafts { get; set; } = [];

        // Engine and load
        public DrivelineEngine? Engine { get; set; }
        public double ExternalLoadTorque { get; set; }

        // Global settings
        public double DefaultFrictionCoefficient { get; set; } = 0.3;

        #endregion

        #region Constructors

        public DrivelineAddin()
            : base(SimscapeAddinKind.Driveline, "Simscape Driveline",
                   ["Simscape", "Simscape Driveline"])
        {
            Tags = ["driveline", "powertrain", "gear", "clutch"];
            SupportedDomains = [DomainType.MechanicalRotational, DomainType.MechanicalTranslational];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a gear to the driveline.
        /// </summary>
        public void AddGear(string name, GearType type, double ratio)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Gears.Add(new DrivelineGear { Name = name, Type = type, Ratio = ratio });
        }

        /// <summary>
        /// Removes a gear by name.
        /// </summary>
        public bool RemoveGear(string name) =>
            Gears.RemoveAll(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Finds a gear by name.
        /// </summary>
        public DrivelineGear? FindGear(string name) =>
            Gears.Find(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a clutch to the driveline.
        /// </summary>
        public void AddClutch(string name, ClutchType type, double maxTorqueCapacity)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Clutches.Add(new DrivelineClutch
            {
                Name = name,
                Type = type,
                MaxTorqueCapacity = maxTorqueCapacity
            });
        }

        /// <summary>
        /// Removes a clutch by name.
        /// </summary>
        public bool RemoveClutch(string name) =>
            Clutches.RemoveAll(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a shaft connecting two components.
        /// </summary>
        public void AddShaft(string name, double stiffness, double damping = 0.0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Shafts.Add(new DrivelineShaft
            {
                Name = name,
                TorsionalStiffness = stiffness,
                Damping = damping
            });
        }

        /// <summary>
        /// Removes a shaft by name.
        /// </summary>
        public bool RemoveShaft(string name) =>
            Shafts.RemoveAll(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Sets the engine for the driveline.
        /// </summary>
        public void SetEngine(string name, double maxTorque, double maxRpm)
        {
            Engine = new DrivelineEngine
            {
                Name = name,
                MaxTorque = maxTorque,
                MaxRpm = maxRpm
            };
        }

        /// <summary>
        /// Calculates the overall gear ratio from input to output.
        /// </summary>
        public double CalculateOverallGearRatio()
        {
            double ratio = 1.0;
            foreach (var gear in Gears)
                ratio *= gear.Ratio;
            return ratio;
        }

        /// <summary>
        /// Validates driveline has at least one gear or shaft and base requirements.
        /// </summary>
        public override bool Validate() =>
            base.Validate() && (Gears.Count > 0 || Shafts.Count > 0);

        #endregion
    }

    #region Driveline Supporting Types

    public class DrivelineGear
    {
        public string Name { get; set; } = string.Empty;
        public GearType Type { get; set; }
        public double Ratio { get; set; } = 1.0;
        public double Efficiency { get; set; } = 1.0;
        public double Inertia { get; set; }
    }

    public class DrivelineClutch
    {
        public string Name { get; set; } = string.Empty;
        public ClutchType Type { get; set; }
        public double MaxTorqueCapacity { get; set; }
        public bool IsEngaged { get; set; }
        public double FrictionCoefficient { get; set; } = 0.3;
    }

    public class DrivelineShaft
    {
        public string Name { get; set; } = string.Empty;
        public double TorsionalStiffness { get; set; } // N*m/rad
        public double Damping { get; set; }             // N*m*s/rad
        public double Length { get; set; }               // m
    }

    public class DrivelineEngine
    {
        public string Name { get; set; } = string.Empty;
        public double MaxTorque { get; set; }  // N*m
        public double MaxRpm { get; set; }
        public double IdleRpm { get; set; } = 800.0;
        public double Inertia { get; set; }    // kg*m²
    }

    public enum GearType
    {
        Simple,
        Planetary,
        Bevel,
        Worm,
        RackAndPinion,
        Differential
    }

    public enum ClutchType
    {
        Friction,
        DogClutch,
        Unidirectional,
        TorqueConverter,
        BandBrake,
        DiscBrake
    }

    /// <summary>
    /// All blocks available in the Simscape Driveline library.
    /// </summary>
    public enum DrivelineBlockType
    {
        // === Gears ===

        /// <summary>Simple gear with fixed ratio between two shafts.</summary>
        SimpleGear,
        /// <summary>Bevel gear with intersecting shaft axes.</summary>
        BevelGear,
        /// <summary>Worm gear with perpendicular non-intersecting axes and high reduction.</summary>
        WormGear,
        /// <summary>Rack and pinion converting rotational to translational motion.</summary>
        RackAndPinion,
        /// <summary>Planetary gear set with sun, planet, and ring members.</summary>
        PlanetaryGear,
        /// <summary>Ravigneaux dual-planetary gear set for automatic transmissions.</summary>
        RavigneauxGear,
        /// <summary>Simpson planetary gear set commonly used in automatic transmissions.</summary>
        SimpsonGear,
        /// <summary>Lepelletier gear set combining Ravigneaux with a simple planetary.</summary>
        LepelletierGear,
        /// <summary>Compound planetary gear set with dual planet gears.</summary>
        CompoundPlanetaryGear,
        /// <summary>Ring-planet subassembly of a planetary gear set.</summary>
        RingPlanetGear,
        /// <summary>Sun-planet subassembly of a planetary gear set.</summary>
        SunPlanetGear,
        /// <summary>Sun-planet bevel gear subassembly.</summary>
        SunPlanetBevel,

        // === Couplings and Differentials ===

        /// <summary>Open differential splitting torque between two output shafts.</summary>
        OpenDifferential,
        /// <summary>Limited-slip differential with torque-biasing capability.</summary>
        LimitedSlipDifferential,
        /// <summary>Torsen-type torque-sensing differential.</summary>
        TorsenDifferential,
        /// <summary>Viscous coupling transferring torque via fluid shear.</summary>
        ViscousCoupling,
        /// <summary>Flexible shaft coupling with torsional compliance.</summary>
        FlexibleShaftCoupling,
        /// <summary>Universal joint (Hooke joint) coupling two shafts at an angle.</summary>
        UniversalJoint,
        /// <summary>Constant-velocity joint for angled shaft coupling without speed variation.</summary>
        ConstantVelocityJoint,

        // === Clutches and Brakes ===

        /// <summary>Friction clutch with controllable normal force.</summary>
        FrictionClutch,
        /// <summary>Dog clutch for positive engagement without slip.</summary>
        DogClutch,
        /// <summary>Cone clutch using conical friction surfaces.</summary>
        ConeClutch,
        /// <summary>Unidirectional clutch (one-way / overrunning clutch).</summary>
        UnidirectionalClutch,
        /// <summary>Band brake applying friction via a band wrapped around a drum.</summary>
        BandBrake,
        /// <summary>Disc brake with caliper-applied friction pads.</summary>
        DiscBrake,
        /// <summary>Drum brake with internally expanding shoes.</summary>
        DrumBrake,
        /// <summary>Double-sided friction clutch.</summary>
        DoubleSidedFrictionClutch,
        /// <summary>Loaded-contact rotational friction between surfaces.</summary>
        LoadedContactRotationalFriction,
        /// <summary>Loaded-contact translational friction between surfaces.</summary>
        LoadedContactTranslationalFriction,

        // === Torque Converters ===

        /// <summary>Torque converter with impeller, turbine, and stator.</summary>
        TorqueConverter,
        /// <summary>Torque converter with lock-up clutch for direct coupling.</summary>
        TorqueConverterWithLockup,

        // === Engines and Motors ===

        /// <summary>Generic engine with torque-speed lookup table.</summary>
        GenericEngine,
        /// <summary>Spark-ignition (gasoline/petrol) engine model.</summary>
        SparkIgnitionEngine,
        /// <summary>Diesel (compression-ignition) engine model.</summary>
        DieselEngine,
        /// <summary>Mapped motor model using efficiency maps.</summary>
        MappedMotor,

        // === Tires and Road ===

        /// <summary>Tire with longitudinal slip dynamics (Magic Formula).</summary>
        TireMagicFormula,
        /// <summary>Tire with simple longitudinal force model.</summary>
        TireSimple,
        /// <summary>Tire-road interaction block (Fiala model).</summary>
        TireFiala,
        /// <summary>Longitudinal wheel with tire compliance.</summary>
        LongitudinalWheel,
        /// <summary>Tire with combined slip (lateral + longitudinal).</summary>
        TireCombinedSlip,

        // === Vehicle Components ===

        /// <summary>Vehicle body with longitudinal dynamics (mass, drag, grade).</summary>
        VehicleBody,
        /// <summary>Two-axle vehicle model for longitudinal dynamics studies.</summary>
        VehicleTwoAxle,

        // === Inertias and Compliances ===

        /// <summary>Rotational inertia element.</summary>
        RotationalInertia,
        /// <summary>Translational mass element.</summary>
        TranslationalMass,
        /// <summary>Rotational spring (torsional compliance).</summary>
        RotationalSpring,
        /// <summary>Rotational damper (torsional viscous friction).</summary>
        RotationalDamper,
        /// <summary>Rotational spring-damper combined element.</summary>
        RotationalSpringDamper,
        /// <summary>Translational spring element.</summary>
        TranslationalSpring,
        /// <summary>Translational damper element.</summary>
        TranslationalDamper,
        /// <summary>Translational spring-damper combined element.</summary>
        TranslationalSpringDamper,

        // === Rotational Friction and Stops ===

        /// <summary>Rotational friction with Coulomb, viscous, and Stribeck effects.</summary>
        RotationalFriction,
        /// <summary>Translational friction with Coulomb, viscous, and Stribeck effects.</summary>
        TranslationalFriction,
        /// <summary>Hard stop limiting rotational travel between two angular bounds.</summary>
        RotationalHardStop,
        /// <summary>Hard stop limiting translational travel between two linear bounds.</summary>
        TranslationalHardStop,

        // === Sources and Sensors ===

        /// <summary>Ideal torque source (rotational).</summary>
        IdealTorqueSource,
        /// <summary>Ideal angular velocity source (rotational).</summary>
        IdealAngularVelocitySource,
        /// <summary>Ideal force source (translational).</summary>
        IdealForceSource,
        /// <summary>Ideal translational velocity source.</summary>
        IdealTranslationalVelocitySource,
        /// <summary>Torque sensor measuring rotational torque.</summary>
        TorqueSensor,
        /// <summary>Angular velocity sensor.</summary>
        AngularVelocitySensor,
        /// <summary>Force sensor measuring translational force.</summary>
        ForceSensor,
        /// <summary>Translational velocity sensor.</summary>
        TranslationalVelocitySensor,
        /// <summary>Rotational motion sensor (angle + velocity + acceleration).</summary>
        RotationalMotionSensor,
        /// <summary>Translational motion sensor (position + velocity + acceleration).</summary>
        TranslationalMotionSensor,
        /// <summary>Rotational power sensor measuring torque × angular velocity.</summary>
        RotationalPowerSensor,
        /// <summary>Translational power sensor measuring force × velocity.</summary>
        TranslationalPowerSensor,

        // === References ===

        /// <summary>Rotational reference (ground / mechanical frame).</summary>
        RotationalReference,
        /// <summary>Translational reference (ground / mechanical frame).</summary>
        TranslationalReference,

        // === Transmission Templates ===

        /// <summary>Automated manual transmission model.</summary>
        AutomatedManualTransmission,
        /// <summary>Continuously variable transmission (CVT) model.</summary>
        ContinuouslyVariableTransmission,
        /// <summary>Dual-clutch transmission model.</summary>
        DualClutchTransmission,

        // === Couplings (Mechanical-to-Mechanical) ===

        /// <summary>Rotational-to-translational converter (lead screw, belt, etc.).</summary>
        RotationalToTranslationalConverter,
        /// <summary>Wheel and axle converting rotational to translational motion.</summary>
        WheelAndAxle,

        // === Utilities ===

        /// <summary>Solver configuration for driveline mechanisms.</summary>
        SolverConfiguration,
        /// <summary>Simscape bus for bundling physical connections.</summary>
        ConnectionBus
    }

    #endregion
}
