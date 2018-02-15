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


    let threadList() = async {

        try

            let p = Debugger.ActiveProcess
            if (p = null) then
                Log.Error("No active inferior process")

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

    // TODO: more improve!
    let gatherOutput f args =

        try
            // Switch MemoryStream
            let ms = new MemoryStream()
            let sw = new StreamWriter(ms)
            let tw = TextWriter.Synchronized(sw)
            sw.AutoFlush <- true
            Console.SetOut(tw)

            f args

            // read data from MemoryStream
            let sr = new System.IO.StreamReader(ms)
            let mutable tmp = int64 0
            let mutable flg = true

            // it seems not to do well...
            while flg = true do
                // wait for output
                System.Threading.Thread.Sleep 150

                if tmp = int64 0 then
                    tmp <- ms.Position
                else
                    if tmp = ms.Position then
                        flg <- false
                    else
                        tmp <- ms.Position

            ms.Position <- int64 0
            let rtn = sr.ReadToEnd()

            // Switch StandardOut
            let std = new StreamWriter(Console.OpenStandardOutput())
            std.AutoFlush <- true
            Console.SetOut(std)

            rtn
        with e -> e.Message


    let run (args:string) =

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


    let stepOver() =
        try
            if (Debugger.State = State.Suspended) then
                Debugger.StepOverLine()
            else
                Log.Error("No suspended inferior process")
        with e -> Log.Info(e.Message)


    let stepInto() =
        try
            if (Debugger.State = State.Suspended) then
                Debugger.StepIntoLine()
            else
                Log.Error("No suspended inferior process")
        with e -> Log.Info(e.Message)


    let stepOut() =
        try
            if (Debugger.State = State.Suspended) then
                Debugger.StepOutOfMethod()
            else
                Log.Error("No suspended inferior process")
        with e -> Log.Info(e.Message)


    let Continue() =
        try
            if (Debugger.State = State.Exited) then
                Log.Error("No inferior process")
            else
                Debugger.Continue()
        with e -> Log.Info(e.Message)


    let func s =
        System.Console.Clear()

        let width = System.Console.WindowWidth
        let line01 = Color.DarkBlue + "─── " + Color.DarkYellow + "Output/messages " + Color.DarkBlue  + String.replicate (width - 4 - 16) "─"
        let line02 = Color.DarkBlue + "─── " + Color.DarkYellow + "Expressions "     + Color.DarkBlue  + String.replicate (width - 4 - 12) "─"
        let line03 = Color.DarkBlue + "─── " + Color.DarkYellow + "Stack "           + Color.DarkBlue  + String.replicate (width - 4 -  6) "─"
        let line04 = Color.DarkBlue + "─── " + Color.DarkYellow + "Threads "         + Color.DarkBlue  + String.replicate (width - 4 -  8) "─"
        let line05 = Color.DarkBlue + "─── " + Color.DarkYellow + "Assembly "        + Color.DarkBlue  + String.replicate (width - 4 -  9) "─"
        let line06 = Color.DarkBlue + String.replicate width "─"

        Log.Info(line02)
        localVariables() |> Async.RunSynchronously
        watches()        |> Async.RunSynchronously

        Log.Info(line03)
        stack()          |> Async.RunSynchronously

        Log.Info(line04)
        threadList()     |> Async.RunSynchronously

        Log.Info(line05)
        Assembly()       |> Async.RunSynchronously

        Log.Info(line01)
        Log.Info(s)


    type MyRun() =
        inherit Command()
        override __.Names         = [|"run"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput run args)

    type MyStepOver() =
        inherit Command()
        override __.Names         = [|"stepover"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput stepOver () )

    type MyStepInto() =
        inherit Command()
        override __.Names         = [|"stepinto"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput stepInto () )

    type MyStepOut() =
        inherit Command()
        override __.Names         = [|"stepout"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput stepOut () )

    type MyContinue() =
        inherit Command()
        override __.Names         = [|"continue"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func (gatherOutput Continue () )

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
