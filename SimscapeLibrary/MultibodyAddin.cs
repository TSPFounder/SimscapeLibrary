// MultibodyAddin.cs
namespace CommunityMaker.Simscape
{
    public sealed class MultibodyAddin : SimscapeAddin
    {
        public MultibodyAddin()
            : base(
                SimscapeAddinKind.Multibody,
                "Simscape Multibody",
                requiredMatlabProducts: new[] { "Simscape", "Simscape Multibody" },
                tags: new[] { "mechanical", "multibody", "3d" })
        { }
    }
}
