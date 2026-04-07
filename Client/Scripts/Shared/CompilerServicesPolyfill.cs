// This file defines stub classes needed to use new features like init and required in .NET Framework

// Parameter is unread
#pragma warning disable CS9113

namespace System.Runtime.CompilerServices
{
    public sealed class IsExternalInit;
    
    public sealed class RequiredMemberAttribute : Attribute;
    
    public sealed class CompilerFeatureRequiredAttribute(string name) : Attribute;
}

namespace System.Diagnostics.CodeAnalysis
{
    public sealed class SetsRequiredMembersAttribute : Attribute;
}