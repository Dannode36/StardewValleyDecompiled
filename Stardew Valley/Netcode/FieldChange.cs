namespace Netcode;

/// <summary>A delegate which handles a field value changing.</summary>
/// <param name="field">The field instance.</param>
/// <param name="oldValue">The previous field value.</param>
/// <param name="newValue">The new field value.</param>
public delegate void FieldChange<in TSelf, in TValue>(TSelf field, TValue oldValue, TValue newValue);
