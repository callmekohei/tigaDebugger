// ===========================================================================
//  FILE    : sdbfs.fsx
//  AUTHOR  : callmekohei <callmekohei at gmail.com>
//  License : MIT license
// ===========================================================================

namespace Mono.Debugger.Client.Commands

#r "/usr/local/lib/sdb/sdb.exe"
#r "/usr/local/lib/sdb/Mono.Debugging.dll"
#r "/usr/local/lib/sdb/Mono.Debugging.Soft.dll"

open Mono.Debugger.Client
open Mono.Debugging.Client

open System
open System.IO
open System.Text

module Foo =

    let localVariables () = async {
        try
            let f = Debugger.ActiveFrame

            if (f = null) then
                if (Debugger.State <> State.Exited) then
                    Log.Info("Backtrace for this thread is unavailable")
                else
                    Log.Error("No active stack frame")
            else


                let vals = f.GetLocalVariables()

                if (vals.Length = 0) then
                    Log.Info("No locals")
                else
                    for v in vals do
                        let strErr = Utilities.StringizeValue(v)

                        if (snd strErr) then
                            Log.Error("{0}<error>{1} {2} = {3}", Color.DarkRed, Color.Reset, v.Name, fst strErr)
                        else
                            Log.Info("{0}{1}{2} {3} = {4}", Color.DarkGreen, v.TypeName, Color.Reset, v.Name, fst strErr)

        with e -> Log.Info(e.Message)
    }


    let stack() = async {
        try
            let f = Debugger.ActiveFrame

            if (f = null) then
                if (Debugger.State <> State.Exited) then
                    Log.Info("Backtrace for this thread is unavailable")
                else
                    Log.Error("No active stack frame")
            else
                Log.Emphasis(Utilities.StringizeFrame(f, true))

        with e -> Log.Info(e.Message)
    }


    let backTrace() = async{

        try
            let p = Debugger.ActiveProcess
            let t = Debugger.ActiveThread

            if (p = null) then
                Log.Error("No active inferior process")
            elif t = null then
                Log.Error("No active thread")
            else
                let threads = p.GetThreads()

                for i in [0..(threads.Length - 1)] do
                    let t = threads.[i]
                    let str = Utilities.StringizeThread(t, false)

                    if (t = Debugger.ActiveThread) then
                        Log.Emphasis(str)
                    else
                        Log.Info(str)

                    let bt = t.Backtrace

                    if (bt.FrameCount <> 0) then
                        for j in [0..(bt.FrameCount - 1)] do
                            let f = bt.GetFrame(j);
                            let fstr = Utilities.StringizeFrame(f, true)

                            if (f = Debugger.ActiveFrame) then
                                Log.Emphasis(fstr)
                            else
                                Log.Info(fstr)
                    else
                        Log.Info("Backtrace for this thread is unavailable")

                    if (i < threads.Length - 1) then
                        Log.Info(String.Empty)

        with e -> Log.Info(e.Message)
    }




    let threadList() = async {

        try

            let p = Debugger.ActiveProcess
            if (p = null) then
                Log.Error("No active inferior process")
            else
                let t = Debugger.ActiveThread
                if t = null then
                    Log.Error("No active thread")
                else
                    let threads = p.GetThreads()

                    let mutable i = 0
                    // for (var i = 0; i < threads.Length; i++)
                    for i in [0..(threads.Length - 1)] do
                        let t = threads.[i]
                        let str = Utilities.StringizeThread(t, true);

                        if (t = Debugger.ActiveThread) then
                            Log.Emphasis(str)
                        else
                            Log.Info(str)

                        if (i < (threads.Length - 1)) then
                            Log.Info(String.Empty)

        with e -> Log.Info(e.Message)
    }


    let thread() =
        try

            let t = Debugger.ActiveThread

            if (t = null) then
                Log.Error("No active thread")
            else
                let str = Utilities.StringizeThread(t, true)

                if (t = Debugger.ActiveThread) then
                    Log.Emphasis(str)
                else
                    Log.Info(str)

        with e -> Log.Info(e.Message)


    let watches() = async{
        try
            for pair in Debugger.Watches do
                let f            = Debugger.ActiveFrame
                let prefix       = pair.Key.ToString()
                let variableName = pair.Value
                let typeName     = f.GetExpressionValue(pair.Value, Debugger.Options.EvaluationOptions).TypeName
                let value        = f.GetExpressionValue(pair.Value, Debugger.Options.EvaluationOptions).Value

                Log.Info("#{0} '{1}':{2}{3}{4} it = {5}", prefix,variableName, Color.DarkGreen, typeName, Color.Reset, value);

        with e -> Log.Info(e.Message)
    }


    let Assembly() = async {
        try
            let f = Debugger.ActiveFrame

            if (f = null) then
                Log.Error("No active stack frame")
            else
                let lower = -5
                let upper = 10

                let asm = f.Disassemble(lower, upper)

                for line in asm do
                    if not line.IsOutOfRange then
                        let str = String.Format("0x{0:X8}    {1}", line.Address, line.Code)
                        if (line.Address = f.Address) then
                            Log.Emphasis(str)
                        else
                            Log.Info(str)

        with e -> Log.Info(e.Message)
    }


    let gatherOutputImpl(ms:MemoryStream, time_ms:int) =

        let sr = new System.IO.StreamReader(ms)
        let mutable tmp = int64 0
        let mutable flg = true

        while flg = true do

            System.Threading.Thread.Sleep time_ms

            if tmp = int64 0 then
                tmp <- ms.Position
            else
                if tmp = ms.Position then
                    flg <- false
                else
                    tmp <- ms.Position

        ms.Position <- int64 0
        sr.ReadToEnd()


    let gatherOutput f args = async {

        try
            // Switch from StandardOut to MemoryStream
            let ms = new MemoryStream()
            let sw = new StreamWriter(ms)
            let tw = TextWriter.Synchronized(sw)
            sw.AutoFlush <- true
            Console.SetOut(tw)

            do! f args

            // read from MemoryStream
            let mutable s = ""
            let mutable flg = true
            let sw = new System.Diagnostics.Stopwatch()
            sw.Start()
            while flg = true do
                s <- gatherOutputImpl(ms,50)
                if s.Contains("exited") || s.Contains("Hit breakpoint at") || s.Contains("suspended") then
                    flg <- false
                elif sw.Elapsed.TotalSeconds > 1.5 then
                    flg <- false
                else
                    s <- gatherOutputImpl(ms,50)

            // Switch from MemoryStream to StandardOut
            let std = new StreamWriter(Console.OpenStandardOutput())
            std.AutoFlush <- true
            Console.SetOut(std)

            return s

        with e -> return e.Message
    }


    let run (args:string) = async {

        if (Debugger.State <> State.Exited) then
            Log.Error("an inferior process is already being debugged")
            ()

        elif not (File.Exists(args)) then
            Log.Error("program executable '{0}' does not exist", args)
            ()

        elif (args.Length = 0) && (Debugger.CurrentExecutable = null) then
            Log.Error("no program path given (and no previous program to re-run)")
            ()

        elif (args.Length = 0) && (Debugger.CurrentExecutable <> null) then

            try
                let file = new FileInfo(Debugger.CurrentExecutable.FullName)
                Debugger.Run(file)
            with e ->
                Log.Error("could not open file '{0}':", args)
                Log.Error( e.Message )
        else
            try
                let file = new FileInfo(args)
                Debugger.Run(file)
            with e ->
                Log.Error("could not open file '{0}':", args)
                Log.Error( e.Message )
    }


    let stepOver() = async {
        try
            if (Debugger.State = State.Suspended) then
                Debugger.StepOverLine()
            else
                Log.Error("No suspended inferior process")
        with e -> Log.Info(e.Message)
    }

    let stepInto() = async {
        try
            if (Debugger.State = State.Suspended) then
                Debugger.StepIntoLine()
            else
                Log.Error("No suspended inferior process")
        with e -> Log.Info(e.Message)
    }

    let stepOut() = async {
        try
            if (Debugger.State = State.Suspended) then
                Debugger.StepOutOfMethod()
            else
                Log.Error("No suspended inferior process")
        with e -> Log.Info(e.Message)
    }

    let Continue() = async {
        try
            if (Debugger.State = State.Exited) then
                Log.Error("No inferior process")
            else
                Debugger.Continue()
        with e -> Log.Info(e.Message)
    }


    let func s = async {

        System.Console.Clear()

        let width = System.Console.WindowWidth
        let line02 = Color.DarkBlue + "─── " + Color.DarkYellow + "Expressions "     + Color.DarkBlue  + String.replicate (width - 4 - 12) "─"
        // let line03 = Color.DarkBlue + "─── " + Color.DarkYellow + "Stack "           + Color.DarkBlue  + String.replicate (width - 4 -  6) "─"
        let line03 = Color.DarkBlue + "─── " + Color.DarkYellow + "BackTrace "       + Color.DarkBlue  + String.replicate (width - 4 - 10) "─"
        // let line04 = Color.DarkBlue + "─── " + Color.DarkYellow + "Threads "         + Color.DarkBlue  + String.replicate (width - 4 -  8) "─"
        let line04 = Color.DarkBlue + "─── " + Color.DarkYellow + "Threads "         + Color.DarkBlue  + String.replicate (width - 4 -  8) "─"
        let line05 = Color.DarkBlue + "─── " + Color.DarkYellow + "Assembly "        + Color.DarkBlue  + String.replicate (width - 4 -  9) "─"
        let line01 = Color.DarkBlue + "─── " + Color.DarkYellow + "Output/messages " + Color.DarkBlue  + String.replicate (width - 4 - 16) "─"
        let line06 = Color.DarkBlue + String.replicate width "─"

        Log.Info(line02)
        localVariables() |> Async.RunSynchronously
        watches()        |> Async.RunSynchronously

        Log.Info(line03)
        backTrace()         |> Async.RunSynchronously
        // stack()          |> Async.RunSynchronously

        // Log.Info(line04)
        // threadList()     |> Async.RunSynchronously

        Log.Info(line05)
        Assembly()       |> Async.RunSynchronously

        Log.Info(line01)
        Log.Info(s)

    }


    type MyRun() =
        inherit Command()
        override __.Names         = [|"run"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput run args |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepOver() =
        inherit Command()
        override __.Names         = [|"stepover"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput stepOver () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepInto() =
        inherit Command()
        override __.Names         = [|"stepinto"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput stepInto () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepOut() =
        inherit Command()
        override __.Names         = [|"stepout"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput stepOut () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyContinue() =
        inherit Command()
        override __.Names         = [|"continue"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput Continue () |> Async.RunSynchronously) |> Async.RunSynchronously

    [<Sealed; Command>]
    type MyCommand() =
        inherit MultiCommand()
        do base.AddCommand<MyRun>()
        do base.AddCommand<MyStepOver>()
        do base.AddCommand<MyStepInto>()
        do base.AddCommand<MyStepOut>()
        do base.AddCommand<MyContinue>()
        override this.Names   = [|"mycmd"|]
        override this.Summary = ""
        override this.Syntax  = ""
        override this.Help    = ""
