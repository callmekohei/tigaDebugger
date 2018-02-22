// ===========================================================================
//  FILE    : sdbplg.fsx
//  AUTHOR  : callmekohei <callmekohei at gmail.com>
//  License : MIT license
// ===========================================================================

namespace Mono.Debugger.Client.Commands

open System
open System.IO
open System.Text
open System.Diagnostics

#r "/usr/local/lib/sdb/sdb.exe"
#r "/usr/local/lib/sdb/Mono.Debugging.dll"
#r "/usr/local/lib/sdb/Mono.Debugging.Soft.dll"
open Mono.Debugger.Client
open Mono.Debugging.Client

#r @"./packages/System.Reactive.Core/lib/net46/System.Reactive.Core.dll"
#r @"./packages/System.Reactive.Linq/lib/net46/System.Reactive.Linq.dll"
#r @"./packages/System.Reactive.Interfaces/lib/net45/System.Reactive.Interfaces.dll"
#r @"./packages/System.Reactive.PlatformServices/lib/net46/System.Reactive.PlatformServices.dll"
#r @"./packages/FSharp.Control.Reactive/lib/net45/FSharp.Control.Reactive.dll"
open FSharp.Control.Reactive


type Generator()  =
    let m_Event = new Event<_>()

    do
        m_Event.Publish
        |> Observable.throttle ( System.TimeSpan.FromMilliseconds(1000.) )
        |> Observable.add ( fun ( flg:ref<bool> ) -> flg := false )
        |> ignore

    member this.PrintOut( s:ref<string>
                        , flg:ref<bool>
                        , gatherOutputImpl: System.IO.MemoryStream * int -> string
                        , ms:MemoryStream
                        , prevLength:ref<int>
                        ) =

        if (!s).Contains("exited") || (!s).Contains("Hit breakpoint at") || (!s).Contains("suspended") then
            flg := false
        else
            s := gatherOutputImpl(ms, 50)
            if !prevLength <> (!s).Length then
                m_Event.Trigger( flg )
            prevLength := (!s).Length


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


    let Source (args:string) =

        let f = Debugger.ActiveFrame

        if (f = null) then
            Log.Error("No active stack frame")
        else

            let lower = 5
            let upper = 5

            let loc  = f.SourceLocation
            let file = loc.FileName
            let line = loc.Line

            if file <> null && line <> -1 then
                if not ( File.Exists(file) ) then
                    Log.Error("Source file '{0}' not found", file);
                else
                    try
                        use reader = new StreamReader (file)

                        let exec = Debugger.CurrentExecutable

                        if (exec <> null && File.GetLastWriteTime(file) > exec.LastWriteTime) then
                            Log.Notice("Source file '{0}' is newer than the debuggee executable", file)

                        let mutable cur:int = 0

                        while ( not reader.EndOfStream ) do
                            let str = reader.ReadLine()

                            let i = line - cur
                            let j = cur - line

                            if (i > 0 && i < lower + 2 || j >= 0 && j < upper) then

                                if (cur = line - 1) then
                                    Log.Info( String.Format("{0,8}: >> {1}" , cur + 1 , Color.Red + str + Color.Reset) )
                                else
                                    Log.Info( String.Format("{0,8}:    {1}", cur + 1, str) )

                            cur <- cur + 1
                    with e ->
                        Log.Error("Could not open source file '{0}'", file)
                        Log.Error( e.Message )

            else
                Log.Error("No source information available")


    let gatherOutputImpl(ms:MemoryStream, time_ms:int) =

        let sr = new System.IO.StreamReader(ms)
        let mutable tmp = int64 0
        let mutable flg = true

        while flg do

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


    let ggg = Generator()


    let gatherOutput f args = async {

        // let throttle = new Throttle<ref<bool>>(1000, fun flg -> flg := false )

        try

            // Switch from StandardOut to MemoryStream
            let ms = new MemoryStream()
            let sw = new StreamWriter(ms)
            let tw = TextWriter.Synchronized(sw)
            sw.AutoFlush <- true
            Console.SetOut(tw)

            do! f args

            // read from MemoryStream
            let s = ref ""
            let flg = ref true
            let prevLength = ref 0
            while !flg do
                ggg.PrintOut( s, flg, gatherOutputImpl, ms, prevLength )

            // Switch from MemoryStream to StandardOut
            let std = new StreamWriter(Console.OpenStandardOutput())
            std.AutoFlush <- true
            Console.SetOut(std)

            return !s

        with e -> return e.Message
    }


    let run (args:string) = async {

        if (Debugger.State <> State.Exited) then
            Log.Error("an inferior process is already being debugged")
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

        elif not (File.Exists( args )) then
            Log.Error("program executable '{0}' does not exist", args)
            ()

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


    // command foo

    let func args s = async {

        System.Console.Clear()

        let width = System.Console.WindowWidth
        let line00 = Color.DarkBlue + "─── " + Color.DarkYellow + "Expressions "     + Color.DarkBlue  + String.replicate (width - 4 - 12) "─"
        let line01 = Color.DarkBlue + "─── " + Color.DarkYellow + "BackTrace "       + Color.DarkBlue  + String.replicate (width - 4 - 10) "─"
        let line02 = Color.DarkBlue + "─── " + Color.DarkYellow + "Source "          + Color.DarkBlue  + String.replicate (width - 4 -  9) "─"
        let line03 = Color.DarkBlue + "─── " + Color.DarkYellow + "Output/messages " + Color.DarkBlue  + String.replicate (width - 4 - 16) "─"

        Log.Info(line00)
        localVariables() |> Async.RunSynchronously
        watches()        |> Async.RunSynchronously

        Log.Info(line01)
        backTrace()         |> Async.RunSynchronously

        Log.Info(line02)
        Source(args)

        Log.Info(line03)
        Log.Info(s)

        // enable to echoback
        Process.Start("stty","echo") |> ignore
        System.Threading.Thread.Sleep 5
        ()

    }

    type MyRun() =
        inherit Command()
        override __.Names         = [|"run"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func args (gatherOutput run args |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepOver() =
        inherit Command()
        override __.Names         = [|"stepover"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func args (gatherOutput stepOver () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepInto() =
        inherit Command()
        override __.Names         = [|"stepinto"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func args (gatherOutput stepInto () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepOut() =
        inherit Command()
        override __.Names         = [|"stepout"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func args (gatherOutput stepOut () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyContinue() =
        inherit Command()
        override __.Names         = [|"continue"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func args (gatherOutput Continue () |> Async.RunSynchronously) |> Async.RunSynchronously

    [<Sealed; Command>]
    type MyCommand() =
        inherit MultiCommand()
        do base.AddCommand<MyRun>()
        do base.AddCommand<MyStepOver>()
        do base.AddCommand<MyStepInto>()
        do base.AddCommand<MyStepOut>()
        do base.AddCommand<MyContinue>()
        override this.Names   = [|"foo"|]
        override this.Summary = ""
        override this.Syntax  = ""
        override this.Help    = ""


    // command bar

    let func_bar args s = async {

        System.Console.Clear()

        let width = System.Console.WindowWidth
        let line00 = Color.DarkBlue + "─── " + Color.DarkYellow + "Expressions "     + Color.DarkBlue  + String.replicate (width - 4 - 12) "─"
        let line01 = Color.DarkBlue + "─── " + Color.DarkYellow + "BackTrace "       + Color.DarkBlue  + String.replicate (width - 4 - 10) "─"
        let line02 = Color.DarkBlue + "─── " + Color.DarkYellow + "Assembly "        + Color.DarkBlue  + String.replicate (width - 4 -  9) "─"
        let line03 = Color.DarkBlue + "─── " + Color.DarkYellow + "Output/messages " + Color.DarkBlue  + String.replicate (width - 4 - 16) "─"

        Log.Info(line00)
        localVariables() |> Async.RunSynchronously
        watches()        |> Async.RunSynchronously

        Log.Info(line01)
        backTrace()         |> Async.RunSynchronously

        Log.Info(line02)
        Assembly()       |> Async.RunSynchronously

        Log.Info(line03)
        Log.Info(s)

        // enable to echoback
        Process.Start("stty","echo") |> ignore
        System.Threading.Thread.Sleep 5
        ()

    }

    type MyRunBar() =
        inherit Command()
        override __.Names         = [|"run"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func_bar args (gatherOutput run args |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepOverBar() =
        inherit Command()
        override __.Names         = [|"stepover"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func_bar args (gatherOutput stepOver () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepIntoBar() =
        inherit Command()
        override __.Names         = [|"stepinto"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func_bar args (gatherOutput stepInto () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyStepOutBar() =
        inherit Command()
        override __.Names         = [|"stepout"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func_bar args (gatherOutput stepOut () |> Async.RunSynchronously) |> Async.RunSynchronously

    type MyContinueBar() =
        inherit Command()
        override __.Names         = [|"continue"|]
        override __.Summary       = ""
        override __.Syntax        = ""
        override __.Help          = ""
        override __.Process(args) = func_bar args (gatherOutput Continue () |> Async.RunSynchronously) |> Async.RunSynchronously

    [<Sealed; Command>]
    type MyCommandBar() =
        inherit MultiCommand()
        do base.AddCommand<MyRunBar>()
        do base.AddCommand<MyStepOverBar>()
        do base.AddCommand<MyStepIntoBar>()
        do base.AddCommand<MyStepOutBar>()
        do base.AddCommand<MyContinueBar>()
        override this.Names   = [|"bar"|]
        override this.Summary = ""
        override this.Syntax  = ""
        override this.Help    = ""
