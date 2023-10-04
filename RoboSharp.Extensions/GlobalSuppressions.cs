// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "IDE0090:Use 'new(...)'", Justification = "Required by .NetStandard Target")]
[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Uses Appropriate Constructor", Scope = "member", Target = "~M:RoboSharp.Extensions.RoboMover.RunAsRoboMover(System.String,System.String,System.String)~System.Threading.Tasks.Task{RoboSharp.Results.RoboCopyResults}")]
[assembly: SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "intential property check", Scope = "member", Target = "~M:RoboSharp.Extensions.IFilePairExtensions.GetFileLength(RoboSharp.Extensions.IFilePair)~System.Int64")]
