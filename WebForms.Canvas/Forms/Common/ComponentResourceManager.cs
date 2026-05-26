using System.Reflection;

namespace System.ComponentModel;

/// <summary>
/// Drop-in replacement for <c>System.ComponentModel.ComponentResourceManager</c>.
/// Designer-generated <c>InitializeComponent</c> methods use this to load localised
/// strings, images, and other resources embedded in the translated assembly.
///
/// This implementation delegates to the real <see cref="System.Resources.ResourceManager"/>
/// so that any embedded <c>.resources</c> files in the translated assembly are resolved
/// correctly at runtime via the <see cref="System.Runtime.Loader.AssemblyLoadContext"/>
/// that loaded the app.
/// </summary>
public class ComponentResourceManager : System.Resources.ResourceManager
{
    /// <summary>
    /// Initialises a new instance scoped to the resources embedded in the assembly
    /// that defines <paramref name="t"/>.
    /// </summary>
    public ComponentResourceManager(Type t) : base(t) { }

    /// <summary>
    /// Applies a resource value identified by <paramref name="objectName"/> to
    /// <paramref name="value"/> when the resource contains a string.
    /// Designer code calls this as:
    /// <code>resources.ApplyResources(this.button1, "button1");</code>
    /// </summary>
    public virtual void ApplyResources(object value, string objectName)
        => ApplyResources(value, objectName, culture: null);

    /// <summary>
    /// Applies localised resource values to <paramref name="value"/> using the
    /// specified <paramref name="culture"/> (null = current UI culture).
    ///
    /// Property values are matched by the naming convention used by the designer:
    ///   <c>{objectName}.{PropertyName}</c> — e.g. <c>"button1.Text"</c>.
    ///
    /// Only publicly settable properties are set; failures are silently ignored
    /// so that missing or incompatible resource entries never crash the app.
    /// </summary>
    public virtual void ApplyResources(object value, string objectName, System.Globalization.CultureInfo? culture)
    {
        if (value is null || string.IsNullOrEmpty(objectName)) return;

        var type = value.GetType();
        var resourceSet = GetResourceSet(culture ?? System.Globalization.CultureInfo.CurrentUICulture,
                                         createIfNotExists: true, tryParents: true);
        if (resourceSet is null) return;

        var prefix = objectName + ".";
        foreach (System.Collections.DictionaryEntry entry in resourceSet)
        {
            var key = entry.Key as string;
            if (key is null || !key.StartsWith(prefix, StringComparison.Ordinal)) continue;

            var propName = key[prefix.Length..];
            if (string.IsNullOrEmpty(propName)) continue;

            var prop = type.GetProperty(propName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop is null || !prop.CanWrite) continue;

            try
            {
                var resourceValue = entry.Value;
                if (resourceValue is not null &&
                    prop.PropertyType.IsAssignableFrom(resourceValue.GetType()))
                {
                    prop.SetValue(value, resourceValue);
                }
                else if (resourceValue is string strVal &&
                         prop.PropertyType == typeof(string))
                {
                    prop.SetValue(value, strVal);
                }
            }
            catch
            {
                // Silently ignore type mismatches or inaccessible setters.
            }
        }
    }
}
