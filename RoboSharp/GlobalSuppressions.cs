// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Required by .NetStandard Target")]
[assembly: SuppressMessage("Style", "IDE0066:Use 'switch expression(...)'", Justification = "Not compatible with C# 7.3 - NetStandard")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Required by IList interface", Scope = "member", Target = "~M:RoboSharp.ImmutableList`1.Clear~RoboSharp.ImmutableList`1")]
