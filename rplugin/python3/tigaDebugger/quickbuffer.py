# ===========================================================================
#  FILE    : quickbuffer.py
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

import os
from tigaDebugger.util import Util

class Quickbuffer:

    def __init__(self,vim):

        self.vim = vim
        self.util = Util(self.vim)
        self.baseBuffer = self.vim.current.buffer


    def createBuffer(self,fileType):

        name = 'quickbuffer-output'

        lst = list(filter(lambda b:os.path.basename(b.name) == name, self.vim.buffers ))

        if len(lst) != 0:
            self.vim.command("bwipe!{{{name}}}".format(name=name))

        self.vim.command("vsplit {name}".format(name=name))
        self.vim.command("setlocal buftype=nofile")
        self.vim.command("set filetype={ft}".format(ft=fileType))
        self.vim.command("setlocal bufhidden=hide")
        self.vim.command("setlocal noswapfile")
        self.vim.command("setlocal nobuflisted")
        self.buf_quickbuffer = self.vim.current.buffer
        self.createBuffer_watch('watch-output')
        self.buffer_move(self.baseBuffer.name)


    def createBuffer_watch(self,name):

        lst = list(filter(lambda b:os.path.basename(b.name) == name, self.vim.buffers ))

        if len(lst) != 0:
            self.vim.command("bwipe!{{{name}}}".format(name=name))

        self.vim.command("split {name}".format(name=name))
        self.vim.command("resize 5")
        self.vim.command("setlocal buftype=nofile")
        self.vim.command("set filetype=tigaDebugger")
        self.vim.command("setlocal bufhidden=hide")
        self.vim.command("setlocal noswapfile")
        self.vim.command("setlocal nobuflisted")
        self.buf_watch = self.vim.current.buffer


    def buffer_move(self,name):

        while True:
            if self.vim.current.buffer.name == name:
                break
            self.vim.command("wincmd w")


    def toWrite(self,lines):

        del self.buf_quickbuffer[:]

        [ self.buf_quickbuffer.append(line, line_number)
            for line, line_number in zip ( lines , range(0, len(lines)) ) ]

    def toWrite_watchWindow(self,lines):

        del self.buf_watch[:]
        if not(not lines):
            self.buf_watch[0:] = lines
