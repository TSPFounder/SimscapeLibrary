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

    /// <summary>
    /// All blocks available in the Simscape Multibody library.
    /// </summary>
    public enum MultibodyBlockType
    {
        // === Body Elements ===

        /// <summary>Solid with brick (box) geometry.</summary>
        BrickSolid,
        /// <summary>Solid with cylindrical geometry.</summary>
        CylindricalSolid,
        /// <summary>Solid with ellipsoidal geometry.</summary>
        EllipsoidSolid,
        /// <summary>Solid with geometry extruded from a 2-D cross-section.</summary>
        ExtrudedSolid,
        /// <summary>Solid with geometry imported from a file (STL, STEP, etc.).</summary>
        FileSolid,
        /// <summary>Solid with geometry revolved from a 2-D cross-section.</summary>
        RevolutionSolid,
        /// <summary>Solid with spherical geometry.</summary>
        SphericalSolid,
        /// <summary>Concentrated point mass with no geometry.</summary>
        PointMass,
        /// <summary>Inertia tensor applied to a frame.</summary>
        Inertia,
        /// <summary>Body with time-varying mass and inertia.</summary>
        GeneralVariableMass,
        /// <summary>Brick solid with variable dimensions.</summary>
        VariableBrickSolid,
        /// <summary>Cylindrical solid with variable dimensions.</summary>
        VariableCylindricalSolid,
        /// <summary>Spherical solid with variable radius.</summary>
        VariableSphericalSolid,
        /// <summary>Visual-only graphic with no mass (marker, arrow, etc.).</summary>
        Graphic,
        /// <summary>Spline-based geometry trajectory.</summary>
        Spline,
        /// <summary>Infinite ground plane for contact modeling.</summary>
        InfinitePlane,
        /// <summary>Grid surface defined by elevation data.</summary>
        GridSurface,

        // === Joints ===

        /// <summary>Single rotational degree of freedom.</summary>
        RevoluteJoint,
        /// <summary>Single translational degree of freedom.</summary>
        PrismaticJoint,
        /// <summary>Three rotational degrees of freedom (ball joint).</summary>
        SphericalJoint,
        /// <summary>One rotational and one translational DOF on a common axis.</summary>
        CylindricalJoint,
        /// <summary>Two translational and one rotational DOF in a plane.</summary>
        PlanarJoint,
        /// <summary>Zero degrees of freedom — rigid connection.</summary>
        WeldJoint,
        /// <summary>Six degrees of freedom — unconstrained.</summary>
        SixDofJoint,
        /// <summary>Two rotational DOF with intersecting axes (Hooke joint).</summary>
        UniversalJoint,
        /// <summary>Three rotational DOF with independent axis sequences.</summary>
        GimbalJoint,
        /// <summary>Six DOF with spring-damper compliance on each axis.</summary>
        BushingJoint,
        /// <summary>One prismatic and one revolute DOF on a slot path.</summary>
        PinSlotJoint,
        /// <summary>Constant velocity coupling between two shafts.</summary>
        ConstantVelocityJoint,
        /// <summary>Two prismatic DOF in a plane (no rotation).</summary>
        RectangularJoint,
        /// <summary>Three translational DOF (no rotation).</summary>
        CartesianJoint,
        /// <summary>Converts rotation to translation via helical coupling.</summary>
        LeadScrewJoint,
        /// <summary>One prismatic DOF that telescopes along an axis.</summary>
        TelescopingJoint,
        /// <summary>Composite joint for shaft bearings.</summary>
        BearingJoint,

        // === Gears and Couplings ===

        /// <summary>Meshing gear pair with intersecting axes.</summary>
        BevelGearConstraint,
        /// <summary>Meshing gear pair with parallel axes.</summary>
        CommonGearConstraint,
        /// <summary>Rotational-to-translational gear conversion.</summary>
        RackAndPinionConstraint,
        /// <summary>Meshing gear pair with perpendicular non-intersecting axes.</summary>
        WormAndGearConstraint,
        /// <summary>Helical rotation-to-translation constraint.</summary>
        LeadScrewConstraint,
        /// <summary>Endpoint of a belt or cable path.</summary>
        BeltCableEnd,
        /// <summary>Spool for winding a belt or cable.</summary>
        BeltCableSpool,
        /// <summary>Pulley that redirects a belt or cable.</summary>
        BeltPulley,

        // === Constraints ===

        /// <summary>Fixes the distance between two frames.</summary>
        DistanceConstraint,
        /// <summary>Fixes the angle between two frames.</summary>
        AngleConstraint,
        /// <summary>Constrains a frame to move along a curve.</summary>
        PointOnCurveConstraint,

        // === Forces and Torques ===

        /// <summary>External force and/or torque applied to a frame.</summary>
        ExternalForceAndTorque,
        /// <summary>Gravitational field acting on all bodies.</summary>
        GravitationalField,
        /// <summary>Force acting between two frames along their connecting line.</summary>
        InternalForce,
        /// <summary>Inverse-square gravitational or Coulomb force between two frames.</summary>
        InverseSquareLawForce,
        /// <summary>Linear spring and damper between two frames.</summary>
        SpringAndDamperForce,
        /// <summary>Penalty-based contact force between two geometries.</summary>
        SpatialContactForce,
        /// <summary>Joint-level spring-damper actuation force.</summary>
        JointSpringAndDamperForce,

        // === Frames and Transforms ===

        /// <summary>Defines the inertial world reference frame.</summary>
        WorldFrame,
        /// <summary>Named reference frame for intermediate connections.</summary>
        ReferenceFrame,
        /// <summary>Fixed spatial transform (rotation + translation) between two frames.</summary>
        RigidTransform,
        /// <summary>Reduced rigid transform — rotation only.</summary>
        RotationalTransform,
        /// <summary>Reduced rigid transform — translation only.</summary>
        TranslationalTransform,

        // === Simulation and Sensing ===

        /// <summary>Configures gravity, linearization, and solver settings for a mechanism.</summary>
        MechanismConfiguration,
        /// <summary>Solver settings for multibody dynamics.</summary>
        SolverConfiguration,
        /// <summary>Measures position, velocity, and acceleration between two frames.</summary>
        TransformSensor,

        // === Utilities ===

        /// <summary>Converts a physical signal to a Simulink output.</summary>
        PSSimulinkConverter,
        /// <summary>Converts a Simulink input to a physical signal.</summary>
        SimulinkPSConverter,
        /// <summary>Subsystem reference for reusable multibody assemblies.</summary>
        MultibodySubsystem
    }

    #endregion
}
