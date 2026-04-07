using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Simulation
{
    /// <summary>
    /// Represents a Simscape model file on disk containing components, models, and library metadata.
    /// Handles serialization, loading, and file-level validation.
    /// </summary>
    public class SimscapeModelFile
    {
        #region Properties

        // File identity
        public string FileName { get; set; } = string.Empty;
        public string DirectoryPath { get; set; } = string.Empty;
        public string FullPath => Path.Combine(DirectoryPath, FileName);
        public ModelFileFormat Format { get; set; } = ModelFileFormat.Ssc;

        // Metadata
        public string Author { get; set; } = string.Empty;
        public string FileVersion { get; set; } = "1.0";
        public string Description { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Contents
        public List<SimscapeComponent> Components { get; set; } = [];
        public List<SimscapeModel> Models { get; set; } = [];
        public List<SimscapeFunction> SharedFunctions { get; set; } = [];

        // State
        public bool IsDirty { get; private set; }
        public bool IsReadOnly { get; set; }

        #endregion

        #region Constructors

        public SimscapeModelFile() { }

        public SimscapeModelFile(string fileName, string directoryPath)
        {
            FileName = fileName;
            DirectoryPath = directoryPath;
        }

        public SimscapeModelFile(string fullPath)
        {
            FileName = Path.GetFileName(fullPath);
            DirectoryPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
        }

        #endregion

        #region Component Methods

        /// <summary>
        /// Adds a component to the file.
        /// </summary>
        public void AddComponent(SimscapeComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (!Components.Contains(component))
            {
                Components.Add(component);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a component by name.
        /// </summary>
        public bool RemoveComponent(string name)
        {
            int removed = Components.RemoveAll(
                c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a component by name.
        /// </summary>
        public SimscapeComponent? FindComponent(string name) =>
            Components.Find(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Model Methods

        /// <summary>
        /// Adds a model to the file.
        /// </summary>
        public void AddModel(SimscapeModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (!Models.Contains(model))
            {
                Models.Add(model);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a model by name.
        /// </summary>
        public bool RemoveModel(string name)
        {
            int removed = Models.RemoveAll(
                m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        /// <summary>
        /// Finds a model by name.
        /// </summary>
        public SimscapeModel? FindModel(string name) =>
            Models.Find(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Shared Function Methods

        /// <summary>
        /// Adds a shared function available to all components in this file.
        /// </summary>
        public void AddSharedFunction(SimscapeFunction function)
        {
            ArgumentNullException.ThrowIfNull(function);
            if (!SharedFunctions.Contains(function))
            {
                SharedFunctions.Add(function);
                MarkDirty();
            }
        }

        /// <summary>
        /// Removes a shared function by name.
        /// </summary>
        public bool RemoveSharedFunction(string name)
        {
            int removed = SharedFunctions.RemoveAll(
                f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) MarkDirty();
            return removed > 0;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes the file contents to a JSON string.
        /// </summary>
        public string Serialize()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var payload = new ModelFilePayload
            {
                FileName = FileName,
                Author = Author,
                FileVersion = FileVersion,
                Description = Description,
                License = License,
                CreatedDate = CreatedDate,
                ModifiedDate = DateTime.UtcNow,
                ComponentNames = Components.Select(c => c.Name).ToList(),
                ModelNames = Models.Select(m => m.Name).ToList(),
                SharedFunctionNames = SharedFunctions.Select(f => f.Name).ToList()
            };
            return JsonSerializer.Serialize(payload, options);
        }

        /// <summary>
        /// Saves the serialized content to <see cref="FullPath"/>.
        /// </summary>
        public bool Save()
        {
            if (IsReadOnly)
                return false;

            try
            {
                var directory = Path.GetDirectoryName(FullPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                var content = Serialize();
                File.WriteAllText(FullPath, content, Encoding.UTF8);
                ModifiedDate = DateTime.UtcNow;
                IsDirty = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the serialized content to a specified path.
        /// </summary>
        public bool SaveAs(string fullPath)
        {
            FileName = Path.GetFileName(fullPath);
            DirectoryPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
            IsReadOnly = false;
            return Save();
        }

        /// <summary>
        /// Loads file metadata from <see cref="FullPath"/> if the file exists.
        /// </summary>
        public bool Load()
        {
            if (!File.Exists(FullPath))
                return false;

            try
            {
                var content = File.ReadAllText(FullPath, Encoding.UTF8);
                var payload = JsonSerializer.Deserialize<ModelFilePayload>(content);
                if (payload is null) return false;

                Author = payload.Author;
                FileVersion = payload.FileVersion;
                Description = payload.Description;
                License = payload.License;
                CreatedDate = payload.CreatedDate;
                ModifiedDate = payload.ModifiedDate;
                IsDirty = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Validation & State

        /// <summary>
        /// Checks whether the file exists on disk.
        /// </summary>
        public bool ExistsOnDisk() => File.Exists(FullPath);

        /// <summary>
        /// Returns the file extension based on the format.
        /// </summary>
        public string GetExtension() => Format switch
        {
            ModelFileFormat.Ssc => ".ssc",
            ModelFileFormat.Slx => ".slx",
            ModelFileFormat.Json => ".json",
            ModelFileFormat.Xml => ".xml",
            _ => ".ssc"
        };

        /// <summary>
        /// Marks the file as having unsaved changes.
        /// </summary>
        public void MarkDirty()
        {
            IsDirty = true;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the file has a name, valid path, and all contents are valid.
        /// </summary>
        public bool Validate() =>
            !string.IsNullOrWhiteSpace(FileName) &&
            !string.IsNullOrWhiteSpace(DirectoryPath) &&
            (Components.Count > 0 || Models.Count > 0) &&
            Components.All(c => c.Validate()) &&
            Models.All(m => m.Validate()) &&
            SharedFunctions.All(f => f.Validate());

        /// <summary>
        /// Collects validation errors across all contents.
        /// </summary>
        public IReadOnlyList<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FileName))
                errors.Add("File name is required.");
            if (string.IsNullOrWhiteSpace(DirectoryPath))
                errors.Add("Directory path is required.");
            if (Components.Count == 0 && Models.Count == 0)
                errors.Add("File must contain at least one component or model.");

            for (int i = 0; i < Components.Count; i++)
            {
                if (!Components[i].Validate())
                    errors.Add($"Component '{Components[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < Models.Count; i++)
            {
                if (!Models[i].Validate())
                    errors.Add($"Model '{Models[i].Name}' at index {i} failed validation.");
            }
            for (int i = 0; i < SharedFunctions.Count; i++)
            {
                if (!SharedFunctions[i].Validate())
                    errors.Add($"Shared function '{SharedFunctions[i].Name}' at index {i} failed validation.");
            }

            return errors.AsReadOnly();
        }

        public override string ToString() =>
            $"{FileName} ({Components.Count} components, {Models.Count} models{(IsDirty ? ", *modified*" : "")})";

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Simscape file format types.
    /// </summary>
    public enum ModelFileFormat
    {
        Ssc,
        Slx,
        Json,
        Xml
    }

    /// <summary>
    /// Serialization payload for model file metadata.
    /// </summary>
    internal class ModelFilePayload
    {
        public string FileName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string FileVersion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<string> ComponentNames { get; set; } = [];
        public List<string> ModelNames { get; set; } = [];
        public List<string> SharedFunctionNames { get; set; } = [];
    }

    #endregion
}
