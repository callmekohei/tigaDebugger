# ===========================================================================
#  FILE    : sdb.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

import neovim
import os
import re

from tigaDebugger.util import Util
from tigaDebugger.quickbuffer import Quickbuffer

class SDB:

    def __init__(self,nvim):
        self.vim = nvim
        self.util = Util(self.vim)
        self.tiga_watch_deleted = ''
        self.source_file = self.util.expand( self.vim.eval( "substitute( expand('%:p') , '\#', '\\#' , 'g' )" ) )

    def set_nr(self,nr):
        self.nr = nr

    def tiga_Command(self, args):

        if type(args) == tuple:
            arg = args[0]
        else:
            arg = args

        job = 'term_getjob({nr})'.format(nr=self.nr)
        ch  = 'job_getchannel({job})'.format(job=job)
        self.vim.command("call ch_sendraw({ch},'{s}\n')".format(ch=ch,s=arg))


### ------------------------------------------------------------------------
### Commands
### ------------------------------------------------------------------------

    def tiga(self,args):
        return 'term_start(["sdb","run {s}"],{{"out_cb":function("Tiga_Handler"), "vertical":1, "term_name":"tigaDebugger-terminal"}})'.format(s=args[0])

    def tiga_Run(self):
        return 'run'

    def tiga_Kill(self):
        return 'kill'

    def tiga_Reset(self):
        return 'reset'

    def tiga_StepOver(self):
        return 'step over'

    def tiga_StepInto(self):
        return 'step into'

    def tiga_StepOut(self):
        return 'step out'

    def tiga_Continue(self):
        return 'continue'

    def tiga_Breakpoints(self):
        fp      = self.util.expand( self.vim.eval( "substitute( expand('%:p') , '\#', '\\#' , 'g' )" ) )
        number  = str(self.vim.eval("line('.')"))
        return 'bp add at {fp} {n}'.format(fp=fp,n=number )

    def tiga_BreakpointsAllClear(self):
        return 'bp clear'

    def tiga_Print(self,args):
        return 'print {arg}'.format(arg=args[0])

    def tiga_WatchAdd(self,args):
        return 'watch add {arg}'.format(arg=args[0])

    def tiga_WatchDel(self,args):
        self.tiga_watch_deleted = args[0]
        self.util.print_cmd(self.tiga_watch_deleted)
        return 'watch'


### ------------------------------------------------------------------------
### Handler
### ------------------------------------------------------------------------

    def tiga_HandlerPy(self, args):

        lst = args[0]
        ### TODO: more simple
        lst = list(map(lambda s:s.replace('\r\n',' ') ,lst))
        lst = list(map(lambda s:re.sub("\x1B\[([0-9]{1,2}(;[0-9]{1,2})*)?m","",s),lst))
        lst = list(map(lambda s:re.sub('#','\#',s),lst))
        lst = list(map(lambda s:re.sub('"',"'",s),lst))

        reversed_lst = lst[::-1]
        cnt = 0
        fst = 0
        lines = []

        for s,n in zip(reversed_lst,range(0,len(reversed_lst))):
            if '(sdb)' in s:
                cnt = cnt + 1
                if cnt == 1:
                    fst = cnt
                if cnt == 2:
                    ll = reversed_lst[:n + 1][::-1]
                    ll[0] = re.sub('^.*\(sdb\)','(sdb)',ll[0])
                    lines = ll
                    break

        try:

            flg_watch = False

            for s in lines:

                ### << colored line >>
                # ### e.g #0 [0x00000000] ABC.DEF.bar at /Users/callmekohei/tmp/sample/abc.fsx:5
                # ### e.g #0 [0x00000000] Microsoft.FSharp.Core.PrintfFormat<Microsoft.FSharp.Core.FSharpFunc<int,Microsoft.FSharp.Core.Unit>,System.IO.TextWriter,Microsoft.FSharp.Core.Unit,Microsoft.FSharp.Core.Unit,int>..ctor at /private/tmp/mono--fsharp-20170917-21849-5wz89g/src/fsharp/FSharp.Core/printf.fs:9 (no source)
                if s != '' and '[0x' in s and not ('no source' in s ):

                    lst = s.split(' ')
                    lst = lst[lst.index('at')+1]

                    fp = (lst.split(':'))[0].strip("'")
                    lineNumber = (lst.split(':'))[1].strip("'")

                    self.vim.command(":e {filepath}".format(filepath=fp))
                    self.vim.command(":call cursor({num}, 0)".format(num=lineNumber))
                    self.vim.command(":setlocal cursorline")
                    self.vim.command(":highlight CursorLine ctermfg=Blue")
                    break

                elif ("exited with code '0'" in s) or ("exited" in s):
                    self.vim.command(":e {sourceFile}".format(sourceFile=self.source_file))
                    self.vim.command(":highlight clear CursorLine")
                    break

                ### << mark breakpoint >>
                ### Breakpoint '0' added at '/Users/callmekohei/tmp/foo.fsx:11' (sdb)
                elif s != '' and 'Breakpoint' in s and 'added' in s :

                    lst        = s.split(' ')
                    s          = lst[lst.index('at')+1]
                    lineNumber = (s.split(':'))[1].strip("'")

                    self.util.signPlace( lineNumber )

                ### << unmark breakpoint >>
                ### A breakpoint at '/Users/callmekohei/tmp/foo.fsx:11' already exists ('0')
                elif s != '' and 'A breakpoint at' in s and 'already exists' in s :

                    lst   = s.split(' ')
                    s     = lst[lst.index('exists')+1]
                    bp_id = s.strip("'(").strip("')")

                    self.util.signUnplace()
                    cmd_str = 'bp delete {id}'.format(id=bp_id)
                    self.tiga_Command(cmd_str)

                ### << delete watch variable >>
                ### (sdb) watch
                ### #0 's': string it = "aaa" (Primitive, Variable)
                ### #2 's': string it = "aaa" (Primitive, Variable)
                ### #3 's': string it = "aaa" (Primitive, Variable)
                elif s != '' and 'watch' in s and not('add' in s) and not ('del' in s) :
                    flg_watch = True

                elif flg_watch == True:

                    tmp = s.split(' ')
                    watchpoint_id = tmp[0].replace('\#','').strip("'")
                    watchpoint_variableName = tmp[1].strip(':').strip("'")

                    self.util.print_cmd(watchpoint_variableName)
                    self.util.print_cmd(self.tiga_watch_deleted)

                    if watchpoint_variableName == self.tiga_watch_deleted:
                        cmd_str = 'watch del {wp_id}'.format(wp_id=str(watchpoint_id))
                        self.tiga_Command(cmd_str)
                        self.tiga_watch_deleted = ''
                        break

        ### list index out of range
        except Exception as e:
            self.util.print_cmd('"error: {err}"'.format(err=str(e)))
