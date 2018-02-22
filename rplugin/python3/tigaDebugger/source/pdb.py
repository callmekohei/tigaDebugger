# ===========================================================================
#  FILE    : pdb.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

import itertools
import neovim
import os
import re
import time

from tigaDebugger.util import Util
from tigaDebugger.quickbuffer import Quickbuffer

class PDB:

    def __init__(self,nvim):
        self.vim  = nvim

        ### external util libraries
        self.util = Util(self.vim)
        self.quickbuffer = Quickbuffer(self.vim)

        ### global variables
        self.tiga_watch_deleted = ''
        self.source_file = self.util.expand( self.vim.eval( "substitute( expand('%:p') , '\#', '\\#' , 'g' )" ) )
        self.ansi_escape = re.compile(r'\x1B\[[0-?]*[ -/]*[@-~]')

        ### global variables for breakpoints
        self.tiga_bp_deleted = ''
        self.tiga_bp_dict = {}
        self.tiga_bp_cnt = 0

        ### for my debug
        self.flg_mydebug = False
        if self.flg_mydebug:
            self.quickbuffer.createBuffer("tigaDebugger_fsharp")
            self.quickbuffer.toWrite(['hello'])

    def startup_setting(self,nr):
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

        self.exe = args[0]

        cmd       = "python3"
        cmd_param = "foo.py"
        # cmd_param = "-m pdb {s}".format(s=args[0])
        ts_param  = '{                               \
              "out_cb"    : function("Tiga_Handler_Python") \
            , "vertical"  : 1                        \
            , "term_name" : "tigaDebugger-terminal"  \
        }'

        # return 'term_start(["{c}","{p0}"],{p1})'.format(c=cmd,p0=cmd_param,p1=ts_param)
        # return 'term_start(["{c}","-m","pdb",{p0}],{p1})'.format(c=cmd,p0=cmd_param,p1=ts_param)
        # return 'term_start(["python3","-m","pdb","foo.py"],{p1})'.format(c=cmd,p0=cmd_param,p1=ts_param)
        return 'term_start(["python3","-m","pdb","{file}"],{p})'.format(file=args[0],p=ts_param)

    def tiga_Run(self):
        # return 'run {file}'.format(file=self.exe)
        return 'run'

    def tiga_Kill(self):
        return 'kill'

    def tiga_Reset(self):
        return 'reset'

    def tiga_StepOver(self):
        return 'next'

    def tiga_StepInto(self):
        return 'step'

    def tiga_StepOut(self):
        return 'until'

    def tiga_Continue(self):
        return 'c'

    # def tiga_Breakpoints(self):
    #     fp      = self.util.expand( self.vim.eval( "substitute( expand('%:p') , '\#', '\\#' , 'g' )" ) )
    #     number  = str(self.vim.eval("line('.')"))
    #     return 'break {fp}:{n}'.format(fp=fp,n=number )

    def tiga_Breakpoints(self):

        fp        = self.util.expand( self.vim.eval( "substitute( expand('%:s:p:t') , '\#', '\\#' , 'g' )" ) )
        number    = str(self.vim.eval("line('.')"))
        fp_number = '{f}:{n}'.format(f=fp,n=number)

        if fp_number not in self.tiga_bp_dict :
            self.util.signPlace( number )
            self.tiga_bp_cnt = self.tiga_bp_cnt + 1
            self.tiga_bp_dict[fp_number] = self.tiga_bp_cnt
            return 'break {s}'.format(s=fp_number)

        else:
            if self.tiga_bp_dict[fp_number] != 0:
                self.util.signUnplace()
                self.util.print_cmd(self.tiga_bp_dict[fp_number])
                number = self.tiga_bp_dict[fp_number]
                self.tiga_bp_dict[fp_number] = 0
                return 'clear {n}'.format(n=number)
            else:
                self.util.signPlace( number )
                self.tiga_bp_cnt = self.tiga_bp_cnt + 1
                self.tiga_bp_dict[fp_number] = self.tiga_bp_cnt
                return 'break {s}'.format(s=fp_number)

    def tiga_BreakpointsAllClear(self):
        self.tiga_bp_dict.clear()
        return 'clear'


    def tiga_Print(self,args):
        return 'pp {arg}'.format(arg=args[0])

    def tiga_WatchAdd(self,args):
        return 'display {arg}'.format(arg=args[0])

    def tiga_WatchDel(self,args):
        # self.tiga_watch_deleted = args[0]
        return 'undisplay {arg}'.format(arg=args[0])


### ------------------------------------------------------------------------
### Handler
### ------------------------------------------------------------------------

    def tiga_HandlerPy(self, args):

        # self.util.print_cmd(args[0])

        lines = self.cutOutProperly('(Pdb++)', self.dataCleaning(args[0]))

        if self.flg_mydebug:
            if not ( not lines ):
                self.quickbuffer.toWrite(lines)

        try:

            flg_watch = False

            for s in lines:

                ### << colored line >>
                # (Pdb) next
                # > /Users/callmekohei/tmp/foo.py(3)foo()
                # -> x = 2
                # (Pdb)

                if '] > ' in s :

                    self.util.print_cmd('callmekohei yes yes yes')

                    fp = s.split(' ')[2].split('(')[0]
                    lineNumber = s.split(' ')[2].split('(')[1].split(')')[0]

                    if os.path.isfile(fp) :

                        self.vim.command(":e {filepath}".format(filepath=fp))
                        self.vim.command(":call cursor({num}, 0)".format(num=lineNumber))
                        self.vim.command(":setlocal cursorline")
                        self.vim.command(":highlight CursorLine ctermfg=Blue")
                        break


                ### << exit >>
                ### e.g Inferior process '13948' ('abc.exe') exited with code '0'
                ### e.g Inferior process '17902' ('foo.exe') exited
                # elif ("exited with code '0'" in s) or ("exited" in s):
                #     self.vim.command(":e {sourceFile}".format(sourceFile=self.source_file))
                #     self.vim.command(":highlight clear CursorLine")
                #     break

                ### << mark breakpoint >>
                ### Breakpoint 2 at /Users/callmekohei/tmp/foo.py:5
                # elif s != '' and 'Breakpoint' in s and 'at' in s :
                #
                #     lst        = s.split(' ')
                #     s          = lst[lst.index('at')+1]
                #     lineNumber = (s.split(':'))[1].strip("'")
                #
                #     self.util.signPlace( lineNumber )

                elif s != '' and 'Clear all breaks?' in s :
                    # self.util.print_cmd("callmekohei!!!!")
                    # self.vim.command('call term_sendkeys("Debugger-terminal","y\n")')
                    self.vim.command('call term_sendkeys({nr},"y\n")'.format(nr=self.nr))
                    self.vim.command('echo "hello hello hello"')
                    break


                ### << unmark breakpoint >>
                ### A breakpoint at '/Users/callmekohei/tmp/foo.fsx:11' already exists ('0')
                # elif s != '' and 'A breakpoint at' in s and 'already exists' in s :
                #
                #     lst   = s.split(' ')
                #     s     = lst[lst.index('exists')+1]
                #     bp_id = s.strip("'(").strip("')")
                #
                #     self.util.signUnplace()
                #     cmd_str = 'bp delete {id}'.format(id=bp_id)
                #     self.tiga_Command(cmd_str)

                ### << delete watch variable >>
                ### (sdb) watch
                ### #0 's': string it = "aaa" (Primitive, Variable)
                ### #2 's': string it = "aaa" (Primitive, Variable)
                ### #3 's': string it = "aaa" (Primitive, Variable)
                # elif s != '' and 'watch' in s and not('add' in s) and not ('del' in s) and not ('Added' in s) :
                #     flg_watch = True
                #
                # elif flg_watch == True:
                #     tmp = s.split(' ')
                #     watchpoint_id = tmp[0].replace('#','').strip("'")
                #     watchpoint_variableName = tmp[1].strip(':').strip("'")
                #
                #     if watchpoint_variableName == self.tiga_watch_deleted:
                #         cmd_str = 'watch del {wp_id}'.format(wp_id=str(watchpoint_id))
                #         self.tiga_Command(cmd_str)
                #         self.tiga_watch_deleted = ''
                #         break

        ### list index out of range
        except Exception as e:
            self.util.print_cmd('"error: {err}"'.format(err=str(e)))


    def cutOutProperly(self,prompt,lst):

        #  (sdb)
        #      step into
        #      foo
        #  (sdb)         <---+
        #      step out      |
        #      bar           |
        #  (sdb)         <---+

        reversed_lst = lst[::-1]
        cnt = 0

        for s,n in zip(reversed_lst,range(0,len(reversed_lst)-1)):
            if prompt in s:
                cnt = cnt + 1
                if cnt == 2:
                    return reversed_lst[:n+1][::-1]
                    break

        #  Welcome...    <---+
        #      ...           |
        #      ...           |
        #  (sdb)         <---+

        if cnt == 1:
            return lst
        else:
            return lst



    def dataCleaning(self,rowlist):
        lstObj = map(self.dataCleaningImpl,rowlist)
        lstObj = itertools.chain.from_iterable(lstObj)
        return list(filter(lambda s:s!='',lstObj))


    def dataCleaningImpl(self,s):
        s = s.replace('\r\n','\n').replace('"',"'").replace('\r','')
        s = self.ansi_escape.sub('', s)
        return s.split('\n')
