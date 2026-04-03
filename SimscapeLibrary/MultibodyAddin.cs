using System;
using System.Collections.Generic;
using Simulation;

namespace CommunityMaker.Simscape
{
    /// <summary>
    /// Simscape Multibody add-in for 3D mechanical simulation of rigid and flexible bodies.
    /// </summary>
    public sealed class MultibodyAddin : SimscapeAddin
    {
        #region Properties

        // World settings
        public GravityVector Gravity { get; set; } = GravityVector.EarthDefault;

        // Body and joint definitions
        public List<RigidBody> Bodies { get; set; } = [];
        public List<MultibodyJoint> Joints { get; set; } = [];
        public List<MultibodyConstraint> Constraints { get; set; } = [];

        // Visualization
        public bool EnableVisualization { get; set; } = true;
        public double FrameScale { get; set; } = 1.0;

        #endregion

        #region Constructors

        public MultibodyAddin()
            : base(SimscapeAddinKind.Multibody, "Simscape Multibody",
                   ["Simscape", "Simscape Multibody"])
        {
            Tags = ["mechanical", "multibody", "3d"];
            SupportedDomains = [DomainType.MechanicalTranslational, DomainType.MechanicalRotational];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a rigid body to the multibody model.
        /// </summary>
        public void AddBody(string name, double mass, double[] inertia)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Bodies.Add(new RigidBody
            {
                Name = name,
                Mass = mass,
                Inertia = inertia ?? [0, 0, 0, 0, 0, 0]
            });
        }

        /// <summary>
        /// Removes a rigid body by name.
        /// </summary>
        public bool RemoveBody(string name) =>
            Bodies.RemoveAll(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a joint connecting two bodies.
        /// </summary>
        public void AddJoint(string name, JointType type, string baseBodyName, string followerBodyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Joints.Add(new MultibodyJoint
            {
                Name = name,
                Type = type,
                BaseBodyName = baseBodyName,
                FollowerBodyName = followerBodyName
            });
        }

        /// <summary>
        /// Removes a joint by name.
        /// </summary>
        public bool RemoveJoint(string name) =>
            Joints.RemoveAll(j => string.Equals(j.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;

        /// <summary>
        /// Adds a constraint between two bodies.
        /// </summary>
        public void AddConstraint(string name, ConstraintType type, string bodyA, string bodyB)
        {
            Constraints.Add(new MultibodyConstraint
            {
                Name = name,
                Type = type,
                BodyAName = bodyA,
                BodyBName = bodyB
            });
        }

        /// <summary>
        /// Finds a body by name.
        /// </summary>
        public RigidBody? FindBody(string name) =>
            Bodies.Find(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Sets the gravity vector for the simulation world.
        /// </summary>
        public void SetGravity(double x, double y, double z) =>
            Gravity = new GravityVector { X = x, Y = y, Z = z };

        /// <summary>
        /// Imports geometry from a CAD file path.
        /// </summary>
        public RigidBody ImportCADGeometry(string name, string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            var body = new RigidBody
            {
                Name = name,
                GeometryFilePath = filePath
            };
            Bodies.Add(body);
            return body;
        }

        /// <summary>
        /// Validates bodies, joints, and base add-in requirements.
        /// </summary>
        public override bool Validate() =>
            base.Validate() && Bodies.Count > 0;

        #endregion
    }

    #region Supporting Types

    public class GravityVector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>Standard Earth gravity (9.81 m/s² downward).</summary>
        public static GravityVector EarthDefault => new() { X = 0, Y = 0, Z = -9.81 };
    }

    public class RigidBody
    {
        public string Name { get; set; } = string.Empty;
        public double Mass { get; set; }
        public double[] Inertia { get; set; } = [0, 0, 0, 0, 0, 0]; // [Ixx, Iyy, Izz, Ixy, Ixz, Iyz]
        public double[] Position { get; set; } = [0, 0, 0];
        public double[] Orientation { get; set; } = [0, 0, 0]; // Euler angles (deg)
        public string GeometryFilePath { get; set; } = string.Empty;
    }

    public class MultibodyJoint
    {
        public string Name { get; set; } = string.Empty;
        public JointType Type { get; set; }
        public string BaseBodyName { get; set; } = string.Empty;
        public string FollowerBodyName { get; set; } = string.Empty;
    }

    public class MultibodyConstraint
    {
        public string Name { get; set; } = string.Empty;
        public ConstraintType Type { get; set; }
        public string BodyAName { get; set; } = string.Empty;
        public string BodyBName { get; set; } = string.Empty;
    }

    public enum JointType
    {
        Revolute,
        Prismatic,
        Spherical,
        Cylindrical,
        Planar,
        Weld,
        SixDof,
        Universal,
        BushingJoint
    }

    public enum ConstraintType
    {
        Distance,
        Angle,
        GearPair,
        BevelGear,
        PointOnCurve
    }

    #endregion
}
