using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Manages saving and loading of application configuration
    /// </summary>
    public class ConfigurationManager
    {
        // Configuration data class
        public class AutomationConfiguration
        {
            [JsonProperty(PropertyName = "ProcessName")]
            public string ProcessName { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "ProcessId")]
            public int ProcessId { get; set; } = -1;

            [JsonProperty(PropertyName = "WindowTitle")]
            public string WindowTitle { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "UIElements")]
            public List<AppUIElementConfig> UIElements { get; set; } = new List<AppUIElementConfig>();

            [JsonProperty(PropertyName = "CalculationRules")]
            public List<CalculationRuleConfig> CalculationRules { get; set; } = new List<CalculationRuleConfig>();

            [JsonProperty(PropertyName = "SequenceSteps")]
            public List<SequenceStepConfig> SequenceSteps { get; set; } = new List<SequenceStepConfig>();
        }

        // Data classes for serialization
        public class AppUIElementConfig
        {
            [JsonProperty(PropertyName = "Name")]
            public string Name { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "ElementType")]
            public string ElementType { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "AutomationId")]
            public string AutomationId { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "Value")]
            public string Value { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "IsTarget")]
            public bool IsTarget { get; set; }
        }

        public class CalculationRuleConfig
        {
            [JsonProperty(PropertyName = "TargetField")]
            public string TargetField { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "Operation")]
            public string Operation { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "Value1")]
            public string Value1 { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "Value2")]
            public string Value2 { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "Formula")]
            public string Formula { get; set; } = string.Empty;
        }

        public class SequenceStepConfig
        {
            [JsonProperty(PropertyName = "StepNumber")]
            public int StepNumber { get; set; }

            [JsonProperty(PropertyName = "Action")]
            public string Action { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "Target")]
            public string Target { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "IsLoopStart")]
            public bool IsLoopStart { get; set; } = false;

            [JsonProperty(PropertyName = "IsLoopEnd")]
            public bool IsLoopEnd { get; set; } = false;

            [JsonProperty(PropertyName = "IsInLoop")]
            public bool IsInLoop { get; set; } = false;

            // Vlastnosti pre klikaníe
            [JsonProperty(PropertyName = "ClickMode")]
            public MainWindow.ClickMode ClickMode { get; set; } = MainWindow.ClickMode.SingleClick;

            [JsonProperty(PropertyName = "ClickCount")]
            public int ClickCount { get; set; } = 1;

            [JsonProperty(PropertyName = "ClickConditionElement")]
            public string? ClickConditionElement { get; set; }

            [JsonProperty(PropertyName = "ClickConditionValue")]
            public string? ClickConditionValue { get; set; }

            [JsonProperty(PropertyName = "ClickInterval")]
            public int ClickInterval { get; set; } = 500;

            // Pre uloženie parametrov slučky
            [JsonProperty(PropertyName = "LoopParameters")]
            public LoopParametersConfig? LoopParameters { get; set; }
        }

        // Class for storing loop parameters
        // Trieda pre ukladanie parametrov slučky
        /// <summary>
        /// Represents the parameters for a loop in the automation sequence
        /// </summary>
        public class LoopParametersConfig
        {
            [JsonProperty(PropertyName = "StartStepIndex")]
            public int StartStepIndex { get; set; }

            [JsonProperty(PropertyName = "EndStepIndex")]
            public int EndStepIndex { get; set; }

            [JsonProperty(PropertyName = "IterationCount")]
            public int IterationCount { get; set; } = 1;

            [JsonProperty(PropertyName = "IsInfiniteLoop")]
            public bool IsInfiniteLoop { get; set; } = false;

            [JsonProperty(PropertyName = "ExitConditionElementName")]
            public string? ExitConditionElementName { get; set; }

            [JsonProperty(PropertyName = "ExitConditionValue")]
            public string? ExitConditionValue { get; set; }
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

                // Serialize to JSON using Newtonsoft.Json with enhanced settings
                string jsonString = JsonConvert.SerializeObject(
                    config,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Populate
                    }
                );

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

                // Deserialize from JSON with enhanced settings
                var config = JsonConvert.DeserializeObject<AutomationConfiguration>(
                    jsonString,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Populate
                    }
                );

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
            var config = new SequenceStepConfig
            {
                StepNumber = step.StepNumber,
                Action = step.Action,
                Target = step.Target,
                IsLoopStart = step.IsLoopStart,
                IsLoopEnd = step.IsLoopEnd,
                IsInLoop = step.IsInLoop,
                ClickMode = step.ClickMode,
                ClickCount = step.ClickCount,
                ClickConditionElement = step.ClickConditionElement,
                ClickConditionValue = step.ClickConditionValue,
                ClickInterval = step.ClickInterval
            };

            // If there are loop parameters, save them as well
            // Ak existujú parametre slučky, uložiť ich tiež
            if (step.LoopParameters != null)
            {
                config.LoopParameters = new LoopParametersConfig
                {
                    StartStepIndex = step.LoopParameters.StartStepIndex,
                    EndStepIndex = step.LoopParameters.EndStepIndex,
                    IterationCount = step.LoopParameters.IterationCount,
                    IsInfiniteLoop = step.LoopParameters.IsInfiniteLoop,
                    ExitConditionElementName = step.LoopParameters.ExitConditionElementName,
                    ExitConditionValue = step.LoopParameters.ExitConditionValue
                };
            }

            return config;
        }

        /// <summary>
        /// Convert SequenceStepConfig to MainWindow.SequenceStep
        /// </summary>
        public static MainWindow.SequenceStep ConvertSequenceStepConfig(SequenceStepConfig config)
        {
            var step = new MainWindow.SequenceStep
            {
                StepNumber = config.StepNumber,
                Action = config.Action,
                Target = config.Target,
                IsLoopStart = config.IsLoopStart,
                IsLoopEnd = config.IsLoopEnd,
                IsInLoop = config.IsInLoop,
                ClickMode = config.ClickMode,
                ClickCount = config.ClickCount,
                ClickConditionElement = config.ClickConditionElement,
                ClickConditionValue = config.ClickConditionValue,
                ClickInterval = config.ClickInterval
            };

            // If there are loop parameters, load them as well
            // Ak existujú parametre slučky, načítať ich tiež
            if (config.LoopParameters != null)
            {
                step.LoopParameters = new MainWindow.LoopControl
                {
                    StartStepIndex = config.LoopParameters.StartStepIndex,
                    EndStepIndex = config.LoopParameters.EndStepIndex,
                    IterationCount = config.LoopParameters.IterationCount,
                    IsInfiniteLoop = config.LoopParameters.IsInfiniteLoop,
                    ExitConditionElementName = config.LoopParameters.ExitConditionElementName,
                    ExitConditionValue = config.LoopParameters.ExitConditionValue
                };
            }

            return step;
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
