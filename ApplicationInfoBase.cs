// ApplicationInfoBase.cs
using System;

namespace PredefinedControlAndInsertionAppProject
{
    // Abstraktná základná trieda pre všetky typy aplikácií
    // Definícia enumerácie ApplicationType
    // Základné spoločné vlastnosti a metódy

    /// <summary>
    /// Base class for application information.
    /// Atributes and methods common to all application types.
    /// </summary>
    public abstract class ApplicationInfoBase
    {
        public string Name { get; set; } = string.Empty;
        public abstract ApplicationType Type { get; }
        public abstract bool CanLaunch { get; }
    }

    public enum ApplicationType
    {
        Desktop,
        Uwp,
        Web
    }
}
