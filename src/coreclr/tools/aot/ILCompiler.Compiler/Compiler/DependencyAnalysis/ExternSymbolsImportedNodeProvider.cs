// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis
{
    public class ExternSymbolsImportedNodeProvider : ImportedNodeProvider
    {
        public override IEETypeNode ImportedEETypeNode(NodeFactory factory, TypeDesc type)
        {
            return new ExternEETypeSymbolNode(factory, type);
        }

        public override ISortableSymbolNode ImportedGCStaticNode(NodeFactory factory, MetadataType type)
        {
            return new ExternSymbolNode(GCStaticsNode.GetMangledName(type, factory.NameMangler), true);
        }

        public override ISortableSymbolNode ImportedNonGCStaticNode(NodeFactory factory, MetadataType type)
        {
            string importPrefix = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                // On Windows, we need to explicitly refer to the exported data. We also need to explicitly mark the
                // export as a DATA export, and in those cases you have to directly refer to the linked symbol, because
                // the linker does not generate a thunk.
                importPrefix = "__imp_";
            }
            return new ExternSymbolNode(importPrefix + NonGCStaticsNode.GetMangledName(type, factory.NameMangler), true);
        }

        public override ISortableSymbolNode ImportedMethodDictionaryNode(NodeFactory factory, MethodDesc method)
        {
            throw new NotImplementedException();
        }

        public override IMethodNode ImportedMethodCodeNode(NodeFactory factory, MethodDesc method, bool unboxingStub)
        {
            return new ExternMethodSymbolNode(factory, method, unboxingStub);
        }
    }
}
