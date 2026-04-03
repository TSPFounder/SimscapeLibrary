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

    #endregion
}
