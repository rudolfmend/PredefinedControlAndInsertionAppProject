using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Runtime.Versioning;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]

    /// <summary>
    /// Manages saving and loading of application configuration
    /// </summary>
    public class ConfigurationManager
    {
        // Configuration data class
        public class AutomationConfiguration
        {
            public string ProcessName { get; set; } = string.Empty; 
            public int ProcessId { get; set; } = -1; 
            public string WindowTitle { get; set; } = string.Empty;
            public List<AppUIElementConfig> UIElements { get; set; } = new List<AppUIElementConfig>();
            public List<CalculationRuleConfig> CalculationRules { get; set; } = new List<CalculationRuleConfig>();
            public List<SequenceStepConfig> SequenceSteps { get; set; } = new List<SequenceStepConfig>();
        }

        // Data classes for serialization
        public class AppUIElementConfig
        {
            public string Name { get; set; } = string.Empty;
            public string ElementType { get; set; } = string.Empty;
            public string AutomationId { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public bool IsTarget { get; set; }
        }

        public class CalculationRuleConfig
        {
            public string TargetField { get; set; } = string.Empty;
            public string Operation { get; set; } = string.Empty;
            public string Value1 { get; set; } = string.Empty;
            public string Value2 { get; set; } = string.Empty;
            public string Formula { get; set; } = string.Empty;
        }

        public class SequenceStepConfig
        {
            public int StepNumber { get; set; }
            public string Action { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
        }

        // Default directory for configuration files
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PredefinedControlApp");

        /// <summary>
        /// Save configuration to a file
        /// </summary>
        public static bool SaveConfiguration(string fileName, AutomationConfiguration config)
        {
            try
            {
                // Make sure the directory exists
                EnsureDirectoryExists();

                // Full path to the configuration file
                string filePath = GetConfigFilePath(fileName);

                // Serialize to JSON
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(config, options);

                // Write to file
                File.WriteAllText(filePath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error saving configuration: {ex.Message}",
                                "Save Error", 5000);
                return false;
            }
        }

        /// <summary>
        /// Load configuration from a file
        /// </summary>
        public static AutomationConfiguration? LoadConfiguration(string fileName)
        {
            try
            {
                // Full path to the configuration file
                string filePath = GetConfigFilePath(fileName);

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    TimedMessageBox.Show($"Configuration file '{fileName}' not found.",
                                    "Load Error", 5000);
                    return null;
                }

                // Read JSON from file
                string jsonString = File.ReadAllText(filePath);

                // Deserialize from JSON
                var config = JsonSerializer.Deserialize<AutomationConfiguration>(jsonString);

                return config;
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error loading configuration: {ex.Message}",
                                "Load Error", 5000);
                return null;
            }
        }

        /// <summary>
        /// Get list of available configuration files
        /// </summary>
        public static List<string> GetAvailableConfigurations()
        {
            try
            {
                // Make sure the directory exists
                EnsureDirectoryExists();

                // Get all JSON files in the configuration directory
                string[] files = Directory.GetFiles(ConfigDirectory, "*.json");

                // Extract just the file names without the .json extension
                var fileNames = new List<string>();
                foreach (string file in files)
                {
                    fileNames.Add(Path.GetFileNameWithoutExtension(file));
                }

                return fileNames;
            }
            catch (Exception)
            {
                // Return empty list on error
                return new List<string>();
            }
        }

        /// <summary>
        /// Convert UIElement to UIElementConfig
        /// </summary>
        public static AppUIElementConfig ConvertAppUIElement(AppUIElement element)
        {
            return new AppUIElementConfig
            {
                Name = element.Name,
                ElementType = element.ElementType,
                AutomationId = element.AutomationId,
                Value = element.Value,
                IsTarget = element.IsTarget
            };
        }

        /// <summary>
        /// Convert UIElementConfig to UIElement
        /// </summary>
        public static AppUIElement ConvertAppUIElementConfig(AppUIElementConfig config)
        {
            return new AppUIElement
            {
                Name = config.Name,
                ElementType = config.ElementType,
                AutomationId = config.AutomationId,
                Value = config.Value,
                IsTarget = config.IsTarget
            };
        }

        /// <summary>
        /// Convert MainWindow.CalculationRule to CalculationRuleConfig
        /// </summary>
        public static CalculationRuleConfig ConvertCalculationRule(MainWindow.CalculationRule rule)
        {
            return new CalculationRuleConfig
            {
                TargetField = rule.TargetField,
                Operation = rule.Operation,
                Value1 = rule.Value1,
                Value2 = rule.Value2,
                Formula = rule.Formula
            };
        }

        /// <summary>
        /// Convert CalculationRuleConfig to MainWindow.CalculationRule
        /// </summary>
        public static MainWindow.CalculationRule ConvertCalculationRuleConfig(CalculationRuleConfig config)
        {
            return new MainWindow.CalculationRule
            {
                TargetField = config.TargetField,
                Operation = config.Operation,
                Value1 = config.Value1,
                Value2 = config.Value2,
                Formula = config.Formula
            };
        }

        /// <summary>
        /// Convert MainWindow.SequenceStep to SequenceStepConfig
        /// </summary>
        public static SequenceStepConfig ConvertSequenceStep(MainWindow.SequenceStep step)
        {
            return new SequenceStepConfig
            {
                StepNumber = step.StepNumber,
                Action = step.Action,
                Target = step.Target
            };
        }

        /// <summary>
        /// Convert SequenceStepConfig to MainWindow.SequenceStep
        /// </summary>
        public static MainWindow.SequenceStep ConvertSequenceStepConfig(SequenceStepConfig config)
        {
            return new MainWindow.SequenceStep
            {
                StepNumber = config.StepNumber,
                Action = config.Action,
                Target = config.Target
            };
        }

        // Helper methods
        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        private static string GetConfigFilePath(string fileName)
        {
            // Make sure the file name has a .json extension
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            return Path.Combine(ConfigDirectory, fileName);
        }
    }
}
