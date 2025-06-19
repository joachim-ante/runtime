// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis
{
    /// <summary>
    /// Represents a symbol that is defined externally but modelled as a type in the
    /// DependencyAnalysis infrastructure during compilation.
    /// </summary>
    public sealed class ExternEETypeSymbolNode : ExternSymbolNode, IEETypeNode
    {
        private TypeDesc _type;

        public ExternEETypeSymbolNode(NodeFactory factory, TypeDesc type)
            : base(
                // On Windows, we need to actually refer to the import symbol so the linker
                // does not generate a thunk, which would break data imports.
                (OperatingSystem.IsWindows() ? "__imp_" : "") +
                factory.NameMangler.NodeMangler.MethodTable(type),
                // this is an extern import, so we have to treat it as redirected
                true
            )
        {
            _type = type;

            factory.TypeSystemContext.EnsureLoadableType(type);
        }

        public TypeDesc Type => _type;
    }
}
