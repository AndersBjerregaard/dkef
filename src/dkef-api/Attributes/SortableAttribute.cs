namespace Dkef.Attributes;

/// <summary>
/// Indicates a property that's able to be queryed to be sorted by at repository level
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SortableAttribute : Attribute { }