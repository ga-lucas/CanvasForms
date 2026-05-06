using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine("Canvas.Windows.Forms.ILTranslator starting...");
        Console.WriteLine($"Args: {string.Join(" | ", args.Select(a => $"'{a}'"))}");

        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: Canvas.Windows.Forms.ILTranslator <input-assembly-path> <output-assembly-path>");
            return 2;
        }

        var inputPath = Path.GetFullPath(args[0]);
        var outputPath = Path.GetFullPath(args[1]);

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input assembly not found: {inputPath}");
            return 3;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Environment.CurrentDirectory);

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(inputPath)!);
        resolver.AddSearchDirectory(AppContext.BaseDirectory);

        var readerParameters = new ReaderParameters
        {
            ReadWrite = false,
            ReadSymbols = File.Exists(Path.ChangeExtension(inputPath, ".pdb")),
            AssemblyResolver = resolver
        };

        try
        {
            using var module = ModuleDefinition.ReadModule(inputPath, readerParameters);

            // ── Pass 1: retarget assembly references ──────────────────────────────
            var updated = RetargetAssemblyReferences(module);

            // ── Pass 2: IL call-site rewrites ─────────────────────────────────────
            var rewritten = RewriteCallSites(module);

            var writerParameters = new WriterParameters
            {
                WriteSymbols = readerParameters.ReadSymbols
            };

            Console.WriteLine($"Writing translated module to: {outputPath}");
            module.Write(outputPath, writerParameters);
            Console.WriteLine($"Wrote: {outputPath} (exists={File.Exists(outputPath)})");
            Console.WriteLine($"Translated '{Path.GetFileName(inputPath)}' -> '{Path.GetFileName(outputPath)}' " +
                              $"(refs updated: {updated}, call-sites rewritten: {rewritten})");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    // ── Pass 1 ────────────────────────────────────────────────────────────────

    private static int RetargetAssemblyReferences(ModuleDefinition module)
    {
        var updated = 0;
        foreach (var reference in module.AssemblyReferences)
        {
            if (reference.Name is "System.Windows.Forms"
                                or "System.Windows.Forms.Primitives"
                                or "WebForms.Canvas")
            {
                // All WinForms surface area lives in Canvas.Windows.Forms.
                reference.Name = "Canvas.Windows.Forms";
                updated++;
            }
        }
        return updated;
    }

    // ── Pass 2 ────────────────────────────────────────────────────────────────
    // Rewrites IL call-sites that are broken by WASM constraints.

    private static int RewriteCallSites(ModuleDefinition module)
    {
        var rewrites = 0;
        rewrites += RewriteDoDragDrop(module);
        return rewrites;
    }

    // ── DoDragDrop rewrite ────────────────────────────────────────────────────
    //
    // Problem: in real WinForms, Control.DoDragDrop(data, effects) blocks until
    // the drag ends and returns the resulting DragDropEffects.  In WASM there is
    // only one thread, so blocking is impossible — DoDragDrop returns None
    // immediately and the actual result is stored in DragDropManager.LastResult
    // once FormRenderer.HandleDrop fires.
    //
    // Translated apps that use the return value of DoDragDrop need a shim:
    //
    //   Original IL (from compiled WinForms app):
    //     ldarg.X / ldarg.Y  (push receiver + args)
    //     call     System.Windows.Forms.Control::DoDragDrop(object, DragDropEffects)
    //     stloc.Z             (store DragDropEffects result)
    //
    //   Rewritten IL:
    //     ldarg.X / ldarg.Y
    //     call     System.Windows.Forms.Control::DoDragDrop(object, DragDropEffects)
    //     pop                                                 ; discard immediate None
    //     call     System.Windows.Forms.DragDropManager::get_LastResult
    //     stloc.Z             ; same local — now holds the real result
    //
    // Only call-sites where the return value is consumed (i.e. followed by a
    // store or use) are patched.  Void-context calls (result discarded by a pop)
    // are left alone — they already work correctly.

    private static int RewriteDoDragDrop(ModuleDefinition module)
    {
        var count = 0;

        // Lazily resolve DragDropManager.LastResult getter reference once it's needed.
        MethodReference? lastResultGetter = null;

        foreach (var type in module.GetTypes())
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;

                var il = method.Body.Instructions;
                for (var i = 0; i < il.Count - 1; i++)
                {
                    var instr = il[i];

                    // Look for: call/callvirt to Control.DoDragDrop(object, DragDropEffects)
                    if (instr.OpCode != OpCodes.Call && instr.OpCode != OpCodes.Callvirt)
                        continue;

                    if (instr.Operand is not MethodReference callee)
                        continue;

                    if (!IsDoDragDropCall(callee))
                        continue;

                    // Check the *next* instruction — if it's `pop` the return value
                    // is already discarded and the call works fine as-is.
                    var next = il[i + 1];
                    if (next.OpCode == OpCodes.Pop)
                        continue;

                    // The next instruction uses the DragDropEffects return value.
                    // Insert:  pop  +  call DragDropManager::get_LastResult
                    // between the DoDragDrop call and the next consumer instruction.

                    lastResultGetter ??= ResolveLastResultGetter(module);
                    if (lastResultGetter == null)
                        break; // Canvas.Windows.Forms not in references — skip

                    var processor = method.Body.GetILProcessor();

                    // Insert `pop` right after the DoDragDrop call
                    var popInstr = processor.Create(OpCodes.Pop);
                    processor.InsertAfter(instr, popInstr);

                    // Insert `call DragDropManager::get_LastResult` after the pop
                    var getLastResult = processor.Create(OpCodes.Call, lastResultGetter);
                    processor.InsertAfter(popInstr, getLastResult);

                    // Advance past the two inserted instructions
                    i += 2;
                    count++;
                }
            }
        }

        return count;
    }

    private static bool IsDoDragDropCall(MethodReference m)
    {
        if (m.Name != "DoDragDrop") return false;
        if (m.Parameters.Count != 2) return false;

        // Parameter types: object, DragDropEffects
        var p0 = m.Parameters[0].ParameterType.FullName;
        var p1 = m.Parameters[1].ParameterType.FullName;
        return (p0 == "System.Object" || p0 == "System.Windows.Forms.IDataObject")
            && p1 == "System.Windows.Forms.DragDropEffects";
    }

    /// <summary>
    /// Finds or creates a MethodReference for DragDropManager.LastResult getter
    /// in the translated module's Canvas.Windows.Forms assembly reference.
    /// Returns null if the reference cannot be resolved (e.g. non-WinForms assembly).
    /// </summary>
    private static MethodReference? ResolveLastResultGetter(ModuleDefinition module)
    {
        // Find the Canvas.Windows.Forms assembly reference (already retargeted by Pass 1).
        var canvasRef = module.AssemblyReferences
            .FirstOrDefault(r => r.Name == "Canvas.Windows.Forms");
        if (canvasRef == null) return null;

        // Build a TypeReference for System.Windows.Forms.DragDropManager
        var managerType = new TypeReference(
            "System.Windows.Forms",
            "DragDropManager",
            module,
            canvasRef);

        // Build a MethodReference for the LastResult property getter
        var getter = new MethodReference("get_LastResult", module.ImportReference(
            typeof(int)), // DragDropEffects is an int-backed enum; use the enum type below
            managerType)
        {
            HasThis = false
        };

        // Fix return type to DragDropEffects enum
        var effectsType = new TypeReference(
            "System.Windows.Forms",
            "DragDropEffects",
            module,
            canvasRef);
        getter.ReturnType = effectsType;

        return module.ImportReference(getter);
    }
}
