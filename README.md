[![MIT-LICENSE](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/callmekohei/tigaDebugger/blob/master/LICENSE)

[![Gitter](https://img.shields.io/gitter/room/nwjs/nw.js.svg)](https://gitter.im/vim-jp/reading-vimrc)

# tigaDebugger

debugger clinet framework for Vim8

<br>
<br>

## ScreenShot

![alt text](./pic/20180215.gif)

## Requirements

Vim8 has('python3')


## Installing

```
$ git clone --depth 1 https://github.com/callmekohei/tigaDebugger
$ git clone --depth 1 https://github.com/roxma/nvim-yarp
$ git clone --depth 1 https://github.com/roxma/vim-hug-neovim-rpc

$ pip3 install neovim

$ vim .vimrc

    set runtimepath+=/path/to/tigaDebugger
    set runtimepath+=/path/to/nvim-yarp
    set runtimepath+=/path/to/vim-hug-neovim-rpc

$ bash build.bash
```

## Usage

if you use `SDB` debugger,  tigaDebugger requires `SDB` installed.

```
// open file
$ vim foo.fsx

// set debugger
: TigaSetDebugger sdb

// start debug mode
: Tiga foo.exe

// debugger command
: TigaCommand bp add at foo.fsx 3

// quit debug mode
: TigaQuit
```

## About Compile

Add `--optimize-` parameter

```
// create exe file
$ fsharpc -g --optimize- foo.fs

// create dll file
$ fsharpc -a -g --optimize- foo.fs
```

## Debugger Shortcut Keys

| Press         | To            |
| :------------ | :-------------|
| ctrl b        | Add or delete <b>B</span></b>reakpoint |
| ctrl d        | <b>D</b>elete all breakpoints |
| ctrl r        | <b>R</b>un |
| ctrl k        | <b>K</b>ill (Break) |
| ctrl p        | Re<b>p</b>lace watch variable |
| ctrl y        | Add watch variable |
| ctrl t        | Delete watch variable |
| ctrl n        | Step over ( <b>N</b>ext ) |
| ctrl i        | Step <b>i</b>n | 
| ctrl u        | Step out ( <b>U</b>p ) | 
| ctrl c        | <b>C</b>ontinue |
