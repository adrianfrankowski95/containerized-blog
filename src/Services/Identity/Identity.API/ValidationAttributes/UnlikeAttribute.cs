namespace Blog.Services.Identity.API.ValidationAttributes;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;


public class UnlikeAttribute : ValidationAttribute
{

    public UnlikeAttribute(string otherProperty)
        : base("'{0}' and '{1}' must be different.")
    {
        if (otherProperty is null)
        {
            throw new ArgumentNullException(nameof(otherProperty));
        }
        OtherProperty = otherProperty;
    }

    public string OtherProperty { get; private set; }

    public string? OtherPropertyDisplayName { get; internal set; }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, OtherPropertyDisplayName ?? OtherProperty);
    }

    public override bool RequiresValidationContext => true;

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        PropertyInfo? otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
        if (otherPropertyInfo is null)
        {
            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Could not find a property named {0}.", OtherProperty));
        }

        object? otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
        if (Equals(value, otherPropertyValue))
        {
            if (string.IsNullOrEmpty(OtherPropertyDisplayName))
            {
                OtherPropertyDisplayName = GetDisplayNameForProperty(validationContext.ObjectType, OtherProperty);
            }
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }
        return null!;
    }

    private static string? GetDisplayNameForProperty(Type containerType, string propertyName)
    {
        ICustomTypeDescriptor? typeDescriptor = GetTypeDescriptor(containerType);
        PropertyDescriptor? property = typeDescriptor?.GetProperties().Find(propertyName, true);
        if (property is null)
        {
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                "The property {0}.{1} could not be found.", containerType.FullName, propertyName));
        }
        IEnumerable<Attribute> attributes = property.Attributes.Cast<Attribute>();
        DisplayAttribute? display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (display != null)
        {
            return display?.GetName();
        }
        DisplayNameAttribute? displayName = attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
        if (displayName != null)
        {
            return displayName.DisplayName;
        }
        return propertyName;
    }

    private static ICustomTypeDescriptor? GetTypeDescriptor(Type type)
    {
        return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
    }

}