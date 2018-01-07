# ===========================================================================
#  FILE    : tigaDebugger_wrap.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

from tigaDebugger import TigaDebugger as _TigaDebugger
import vim

_obj = _TigaDebugger(vim)


### ------------------------------------------------------------------------
### Tiga Status
### ------------------------------------------------------------------------

def tiga_IsAlive(*args):
    return _obj.tiga_IsAlive(args)


### ------------------------------------------------------------------------
### Tiga Commands
### ------------------------------------------------------------------------

### general command
def tiga_Command(*args):
    return _obj.tiga_Command(args)


### start and quit
def tiga_Set_Debugger(*args):
    return _obj.tiga_Set_Debugger(args)

def tiga(*args):
    return _obj.tiga(args)

def tiga_Quit(*args):
    return _obj.tiga_Quit(args)


### run, break(kill process) and reset
def tiga_Run(*args):
    return _obj.tiga_Run(args)

def tiga_Kill(*args):
    return _obj.tiga_Kill(args)

def tiga_Reset(*args):
    return _obj.tiga_Reset(args)


### step and continue
def tiga_StepOver(*args):
    return _obj.tiga_StepOver(args)

def tiga_StepInto(*args):
    return _obj.tiga_StepInto(args)

def tiga_StepOut(*args):
    return _obj.tiga_StepOut(args)

def tiga_Continue(*args):
    return _obj.tiga_Continue(args)


### add and clear breakpoints
def tiga_Breakpoints(*args):
    return _obj.tiga_Breakpoints(args)

def tiga_BreakpointsAllClear(*args):
    return _obj.tiga_BreakpointsAllClear(args)


### print variables and watch variables
def tiga_Print(*args):
    return _obj.tiga_Print(args)

def tiga_WatchAdd(*args):
    return _obj.tiga_WatchAdd(args)

def tiga_WatchDel(*args):
    return _obj.tiga_WatchDel(args)


### ------------------------------------------------------------------------
### Tiga Handler
### ------------------------------------------------------------------------

def tiga_HandlerPy(*args):
    return _obj.tiga_HandlerPy(args)



