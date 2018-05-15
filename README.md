[![MIT-LICENSE](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/callmekohei/tigaDebugger/blob/master/LICENSE)
[![Gitter](https://img.shields.io/gitter/room/nwjs/nw.js.svg)](https://gitter.im/vim-jp/reading-vimrc)

![alt text](./pic/20180217.gif)

# tigaDebugger

`tigaDebugger` is a tiny debugger for F#.


## Requires
vim ( above version 8,  +python3, +terminal )  
[sdb](https://github.com/mono/sdb)  
[sdbplg](https://github.com/callmekohei/sdbplg)


## Install

```
// download
$ git clone --depth 1 https://github.com/callmekohei/tigaDebugger
$ git clone --depth 1 https://github.com/roxma/nvim-yarp
$ git clone --depth 1 https://github.com/roxma/vim-hug-neovim-rpc

// install neovim plugins
$ pip3 install neovim

// set runtimepath
$ vim .vimrc

    set runtimepath+=/path/to/tigaDebugger
    set runtimepath+=/path/to/nvim-yarp
    set runtimepath+=/path/to/vim-hug-neovim-rpc
```

## Usage

```shell
// write fsharp code
$ vim foo.fsx

    let foo() =
        let mutable x = 1
        x <- 2
        x <- 3
        x

    foo ()
    |> stdout.WriteLine


// compile file
$ fsharpc -g --optimize- foo.fsx

// open file
$ vim foo.fsx

// start debug mode
: TigaSetDebugger sdb
: Tiga foo.exe

// set break point
: TigaCommand bp add at foo.fsx 3

// run
: TigaCommand r

// next
: TigaCommand n

// quit debug mode
: TigaQuit
```

## Shortcut Keys

| Press         | To                                     |
| :------------ | :-------------                         |
| ctrl b        | Add or delete <b>B</span></b>reakpoint |
| ctrl d        | <b>D</b>elete all breakpoints          |
| ctrl r        | <b>R</b>un                             |
| ctrl k        | <b>K</b>ill (Break)                    |
| ctrl p        | Re<b>p</b>lace watch variable          |
| ctrl y        | Add watch variable                     |
| ctrl t        | Delete watch variable                  |
| ctrl n        | Step over ( <b>N</b>ext )              |
| ctrl i        | Step <b>I</b>n                         |
| ctrl u        | Step out ( <b>U</b>p )                 |
| ctrl c        | <b>C</b>ontinue                        |


## About Compile

`--optimize-` parameter is required.

```
// create exe file
$ fsharpc -g --optimize- foo.fsx

// create dll file
$ fsharpc -a -g --optimize- foo.fsx
```

## About Top-Level variables

see also: [about top-level variables](https://github.com/Microsoft/visualfsharp/issues/4149)

Top-Level varibables needs full namespaces.

```fsharp
// file name is foo.fsx
module Bar =
    let mutable x = "hello"
    x <- "world"
    stdout.WriteLine(x)
```

tiga command 
```
: TigaWatchAdd Foo.Bar.x
```

result
```
─── Expressions ─────────────────
No locals
#0 'Foo.Bar.x':string it = "hello"
```

## About terminal buffer mode

`Terminal buffer` must have either a `Terminal-Job` or `Terminal-Normal` mode.

vim help
```vim
: help Terminal-mode
```

Terminal-Job mode
```vim
'i' or 'a'
```

Terminal-Normal mode ( enable to scroll )
```vim
CTRL-w N
```

