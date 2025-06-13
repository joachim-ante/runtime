// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

namespace ILCompiler
{
    public class ExportsFileWriter
    {
        private readonly string _exportsFile;
        private readonly string[] _exportSymbols;
        private readonly List<EcmaMethod> _methods;
        private readonly TypeSystemContext _context;

        public ExportsFileWriter(TypeSystemContext context, string exportsFile, string[] exportSymbols)
        {
            _exportsFile = exportsFile;
            _exportSymbols = exportSymbols;
            _context = context;
            _methods = new List<EcmaMethod>();
        }

        public void AddExportedMethods(IEnumerable<EcmaMethod> methods)
            => _methods.AddRange(methods);

        public void EmitExportedMethods()
        {
            FileStream fileStream = new FileStream(_exportsFile, FileMode.Create);
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                if (_context.Target.IsWindows)
                {
                    streamWriter.WriteLine("EXPORTS");
                    // @TODO: This is a hack. Cf. WindowsNodeMangler
                    // We need to explicitly export these as DATA so that the linker knows not to produce import thunks for them.
                    // Once external vtables are fixed, the vtable part is needs to be uncommented.
                    // static bool IsVTable(string symbol) => symbol.StartsWith("??_7") && symbol.EndsWith("@@6B@");
                    static bool IsNonGCStatics(string symbol) => symbol.StartsWith("?__NONGCSTATICS") && symbol.EndsWith("@@");
                    static bool IsData(string symbol) => IsNonGCStatics(symbol) /* || IsVTable(symbol) */;
                    foreach (string symbol in _exportSymbols)
                    {
                        if (IsData(symbol))
                            streamWriter.WriteLine($"   {symbol.Replace(',', ' ')} DATA");
                        else
                            streamWriter.WriteLine($"   {symbol.Replace(',', ' ')}");
                    }
                    foreach (var method in _methods)
                        streamWriter.WriteLine($"   {method.GetUnmanagedCallersOnlyExportName()}");
                }
                else if (_context.Target.IsApplePlatform)
                {
                    //@TODO: Check if these string.IsNullOrEmpty changes are really required
                    foreach (string symbol in _exportSymbols)
                    {
                        if (!string.IsNullOrEmpty(symbol))
                            streamWriter.WriteLine($"_{symbol}");
                    }

                    foreach (var method in _methods)
                    {
                        var symbol = method.GetUnmanagedCallersOnlyExportName();
                        if (!string.IsNullOrEmpty(symbol))
                            streamWriter.WriteLine($"_{symbol}");
                    }
                }
                else
                {
                    streamWriter.WriteLine("V1.0 {");
                    if (_exportSymbols.Length != 0 || _methods.Count != 0)
                    {
                        streamWriter.WriteLine("    global:");
                        foreach (string symbol in _exportSymbols)
                            streamWriter.WriteLine($"        {symbol};");
                        foreach (var method in _methods)
                            streamWriter.WriteLine($"        {method.GetUnmanagedCallersOnlyExportName()};");
                    }
                    streamWriter.WriteLine("    local: *;");
                    streamWriter.WriteLine("};");
                }
            }
        }
    }
}
