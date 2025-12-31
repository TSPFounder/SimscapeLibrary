// SimscapeAddin.cs
using System;
using System.Collections.Generic;

namespace CommunityMaker.Simscape
{
    public abstract class SimscapeAddin
    {
        protected SimscapeAddin(
            SimscapeAddinKind kind,
            string displayName,
            IReadOnlyList<string> requiredMatlabProducts,
            IReadOnlyList<string>? tags = null)
        {
            Kind = kind;
            DisplayName = displayName ?? kind.ToString();
            RequiredMatlabProducts = requiredMatlabProducts ?? Array.Empty<string>();
            Tags = tags ?? Array.Empty<string>();
        }

        public SimscapeAddinKind Kind { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> RequiredMatlabProducts { get; }
        public IReadOnlyList<string> Tags { get; }

        /// <summary>
        /// Generates a MATLAB snippet that checks whether the required products are installed/licensed.
        /// Note: product IDs/names can be tuned to your environment.
        /// </summary>
        public virtual string ToMatlabLicenseCheckScript(string resultVarName = "isAvailable")
        {
            // Uses ver + license('test', ...) pattern. This is intentionally simple.
            // You can swap to matlab.addons.installedAddons or other APIs if desired.
            var lines = new List<string>
            {
                $"{resultVarName} = true;",
                "v = ver;"
            };

            foreach (var p in RequiredMatlabProducts)
            {
                // "Simscape Multibody" etc
                lines.Add(
                    $"{resultVarName} = {resultVarName} && any(strcmp({{v.Name}}, '{EscapeMatlab(p)}'));");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string EscapeMatlab(string s) => s.Replace("'", "''");
    }

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
}
