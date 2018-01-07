# ===========================================================================
#  FILE    : __init__.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

import neovim
import os
import re

from tigaDebugger.util import Util
from tigaDebugger.quickbuffer import Quickbuffer

from tigaDebugger.source.sdb import SDB as td

@ neovim.plugin
class TigaDebugger(object):

    def __init__(self,nvim):

        self.vim  = nvim
        self.util = Util(self.vim)
        self.tiga_debug_mode = False
        self.tiga_already_set_debugger = False
        self.nr = -1
        self.vim.command(":sign define piet text=>> texthl=Search")
        self.source_file = self.util.expand( self.vim.eval( "substitute( expand('%:p') , '\#', '\\#' , 'g' )" ) )


### ------------------------------------------------------------------------
### Tiga Status
### ------------------------------------------------------------------------

    @ neovim.function("Tiga_IsAlive", sync = False)
    def tiga_IsAlive(self,args):
        if not self.tiga_debug_mode:
            return False
        else:
            return True

### ------------------------------------------------------------------------
### Tiga Commands
### ------------------------------------------------------------------------

    ### general command

    @neovim.function("Tiga_Command", sync=False)
    def tiga_Command(self, args):

        if type(args) == tuple:
            arg = args[0]
        else:
            arg = args

        job = 'term_getjob({nr})'.format(nr=self.nr)
        ch  = 'job_getchannel({job})'.format(job=job)
        self.vim.command("call ch_sendraw({ch},'{s}\n')".format(ch=ch,s=arg))


    ### set debugger, start and quit

    @ neovim.function("Tiga_Set_Debugger", sync = False)
    def tiga_Set_Debugger(self,args):

        if   args[0] == 'sdb':
            from tigaDebugger.source.sdb import SDB as td
            self.set_debugerImpl(args)
        elif args[0] == 'gdb':
            from tigaDebugger.source.gdb import GDB as td
            self.set_debugerImpl(args)
        else:
            self.util.print_cmd('Sorry! Now not avairable!')


    def set_debugerImpl(self,args):
        self.td   = td(self.vim)
        self.tiga_already_set_debugger = True
        self.util.print_cmd('set {s}'.format(s=args[0]))


    @neovim.function("Tiga", sync=False)
    def tiga(self,args):

        if self.tiga_already_set_debugger:
            cmd_str = self.td.tiga(args)
            self.nr = self.vim.eval(cmd_str)
            self.td.set_nr(self.nr)
            self.vim.command("wincmd p")
            self.tiga_debug_mode = True
            self.vim.command('write') ### for keymap
        else:
            self.util.print_cmd('Please set debugger. (e.g) :TigaSetDebugger gdb')


    @ neovim.function("Tiga_Quit", sync = False)
    def tiga_Quit(self,arg):
        n = self.vim.eval('bufnr("tigaDebugger-terminal")')
        self.vim.command('bwipeout! {n}'.format(n=n))
        self.tiga_debug_mode = False
        self.vim.command(":highlight clear CursorLine")
        self.vim.command(':sign unplace *')
        self.vim.command('write') ### for keymap


    ### run, break(kill process) and reset

    @neovim.function("Tiga_Run", sync=False)
    def tiga_Run(self,args):
        cmd_str = self.td.tiga_Run()
        self.tiga_Command(cmd_str)

    @ neovim.function("Tiga_Kill", sync=False)
    def tiga_Kill(self,arg):
        cmd_str = self.td.tiga_Kill()
        self.tiga_Command(cmd_str)

    @ neovim.function("Tiga_Reset", sync=False)
    def tiga_Reset(self,arg):
        cmd_str = self.td.tiga_Reset()
        self.tiga_Command(cmd_str)
        self.vim.command(":highlight clear CursorLine")
        self.vim.command(':sign unplace *')


    ### step and continue

    @neovim.function("Tiga_StepOver", sync=False)
    def tiga_StepOver(self,args):
        cmd_str = self.td.tiga_StepOver()
        self.tiga_Command(cmd_str)

    @neovim.function("Tiga_StepInto", sync=False)
    def tiga_StepInto(self,args):
        cmd_str = self.td.tiga_StepInto()
        self.tiga_Command(cmd_str)

    @ neovim.function("Tiga_StepOut", sync=True)
    def tiga_StepOut(self,arg):
        cmd_str = self.td.tiga_StepOut()
        self.tiga_Command(cmd_str)

    @neovim.function("Tiga_Continue", sync=False)
    def tiga_Continue(self,args):
        cmd_str = self.td.tiga_Continue()
        self.tiga_Command(cmd_str)


    ### add and clear breakpoints

    @neovim.function("Tiga_Breakpoints", sync=False)
    def tiga_Breakpoints(self,args):
        cmd_str = self.td.tiga_Breakpoints()
        self.tiga_Command(cmd_str)

    @ neovim.function("Tiga_BreakpointsAllClear", sync=False)
    def tiga_BreakpointsAllClear(self,arg):
        cmd_str = self.td.tiga_BreakpointsAllClear()
        self.tiga_Command(cmd_str)
        self.vim.command(':sign unplace *')


    ### print variables and watch variables

    @ neovim.function("Tiga_Print", sync=False)
    def tiga_Print(self,args):
        cmd_str = self.td.tiga_Print(args)
        self.tiga_Command(cmd_str)

    @ neovim.function("Tiga_WatchAdd", sync=False)
    def tiga_WatchAdd(self,args):
        cmd_str = self.td.tiga_WatchAdd(args)
        self.tiga_Command(cmd_str)

    @ neovim.function("Tiga_WatchDel", sync=False)
    def tiga_WatchDel(self,args):
        cmd_str = self.td.tiga_WatchDel(args)
        self.tiga_Command(cmd_str)


### ------------------------------------------------------------------------
### Tiga Handler
### ------------------------------------------------------------------------

    @neovim.function("Tiga_HandlerPy", sync=False)
    def tiga_HandlerPy(self, args):
        self.td.tiga_HandlerPy(args)
