# ===========================================================================
#  FILE    : gdb.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

import itertools
import neovim

from tigaDebugger.util import Util
from tigaDebugger.quickbuffer import Quickbuffer

class GDB:

    def __init__(self,nvim):
        self.vim  = nvim

        ### external util libraries
        self.util = Util(self.vim)
        self.quickbuffer = Quickbuffer(self.vim)

        ### global variables for breakpoints
        self.tiga_bp_deleted = ''
        self.tiga_bp_dict = {}
        self.tiga_bp_cnt = 0

        self.source_file = self.util.expand( self.vim.eval( "substitute( expand('%:p') , '\#', '\\#' , 'g' )" ) )

        ### for my debug
        self.flg_mydebug = False
        if self.flg_mydebug:
            self.quickbuffer.createBuffer("tigaDebugger_c")
            self.quickbuffer.toWrite(['hello'])



    def startup_setting(self,nr):
        self.nr = nr
        self.tiga_Command('set confirm off')

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

        cmd       = "gdb"
        cmd_p0    = "-quiet"
        cmd_p1    = "{s}".format(s=args[0])
        ts_param  = '{                               \
              "out_cb"    : function("Tiga_Handler") \
            , "vertical"  : 1                        \
            , "term_name" : "tigaDebugger-terminal"  \
        }'

        return 'term_start(["{c}","{p0}", "{p1}"],{p2})'.format(c=cmd,p0=cmd_p0,p1=cmd_p1,p2=ts_param)

    def tiga_Run(self):
        return 'run'

    def tiga_Kill(self):
        return ''

    def tiga_Reset(self):
        return ''

    def tiga_StepOver(self):
        return 'next'

    def tiga_StepInto(self):
        return 'step'

    def tiga_StepOut(self):
        return ''

    def tiga_Continue(self):
        return 'continue'

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
                return 'delete {n}'.format(n=number)
            else:
                self.util.signPlace( number )
                self.tiga_bp_cnt = self.tiga_bp_cnt + 1
                self.tiga_bp_dict[fp_number] = self.tiga_bp_cnt
                return 'break {s}'.format(s=fp_number)

    def tiga_BreakpointsAllClear(self):
        self.tiga_bp_dict.clear()
        return 'delete'

    def tiga_Print(self,args):
        return 'print {arg}'.format(arg=args[0])

    def tiga_WatchAdd(self,args):
        return 'watch {arg}'.format(arg=args[0])

    def tiga_WatchDel(self,args):
        self.tiga_watch_deleted = args[0]
        return ''


### ------------------------------------------------------------------------
### Handler
### ------------------------------------------------------------------------

    def tiga_HandlerPy(self, args):

        lines = self.cutOutProperly('(gdb)',self.dataCleaning(args[0]))


        if self.flg_mydebug:
            self.quickbuffer.toWrite(lines)

        try:

            for s in lines:
                ###  << exit >>
                ###  [Inferior 1 (process 5683) exited normally]
                if 'exited normally' in s:
                    self.vim.command(":highlight clear CursorLine")
                    break

                ###  << colored line >>
                ###  8	    n = 4;
                elif self.util.isNumeric(s.split(' ')[0]):
                    lineNumber = s.split(' ')[0]
                    self.vim.command(":call cursor({num}, 0)".format(num=lineNumber))
                    self.vim.command(":setlocal cursorline")
                    self.vim.command(":highlight CursorLine ctermfg=Blue")
                    break

        ### list index out of range
        except Exception as e:
            self.util.print_cmd('"error: {err}"'.format(err=str(e)))


    def cutOutProperly(self,prompt,lst):

        # TODO: needs code for case of no endmark (gdb)

        #  (gdb)
        #      step into
        #      foo
        #  (gdb)         <---+
        #      step out      |
        #      bar           |
        #  (gdb)         <---+

        reversed_lst = lst[::-1]
        cnt = 0

        for s,n in zip(reversed_lst,range(0,len(reversed_lst)-1)):
            if prompt in s:
                cnt = cnt + 1
                if cnt == 2:
                    return reversed_lst[:n+1][::-1]
                    break


    def dataCleaning(self,rowlist):
        lstObj = map(self.dataCleaningImpl,rowlist)
        lstObj = itertools.chain.from_iterable(lstObj)
        return list(filter(lambda s:s!='',lstObj))


    def dataCleaningImpl(self,s):
        s = s.replace('\r\n','\n').replace('"',"'")
        return s.split('\n')
