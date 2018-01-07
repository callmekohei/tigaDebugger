# ===========================================================================
#  FILE    : util.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

import os

class Util:

    def __init__(self, vim):
        self.vim = vim


    def print_cmd(self, s):
        self.vim.command('echomsg "{s}"'.format(s=s))


    def expand(self,path):
        return os.path.expandvars(os.path.expanduser(path))


    def isNumeric(self,arg):
        try:
            float(arg)
            return True
        except:
            return False


    # this code form Shougo/deoplete.nvim util.py
    # see: https://github.com/Shougo/deoplete.nvim/blob/master/rplugin/python3/deoplete/util.py
    def getlines(self, vim, start=1, end='$'):
        if end == '$':
            end = len(vim.current.buffer)
        max_len = min([end - start, 5000])
        lines = []
        current = start
        while current <= end:
            lines += vim.call('getline', current, current + max_len)
            current += max_len + 1
        return lines


    def signPlace(self,line_number):
        self.vim.command(":exe ':sign place {ln} line={ln} name=piet file=' . expand('%:p')".format(ln=line_number))


    def signUnplace(self):
        self.vim.command(':sign unplace')
