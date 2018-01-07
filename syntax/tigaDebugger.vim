" ===========================================================================
"  FILE    : tigaDebugger.vim
"  AUTHOR  : callmekohei <callmekohei at gmail.com>
"  License : MIT license
" ===========================================================================

syntax match  errorMsg            "\(^.*exception.*$\|^.*Exception.*$\)"
syntax match  msg_tigaDebugger    "(tigaDebugger).*$"
syntax match  warn_tigaDebugger   "(tigaDebugger) Warning --------------------------------"
syntax match  stdin_tigaDebugger  "(tigaDebugger) Please input anything..."
syntax match  stdin_tigaDebugger  "(tigaDebugger) use :SDBCommand"
syntax match  msg_sdb             "Welcome to the Mono soft debugger.*$"
syntax match  msg_sdb             "Type 'help' for a list of commands or 'quit' to exit"
syntax match  msg_sdb             "Inferior.*$"
syntax match  msg_sdb             "Hit.*$"
syntax match  no_process          "No inferior process.*$"
syntax match  no_process          "No suspended inferior process.*$"
syntax match  breakpoints         "Breakpoint.*$"
syntax match  breakpoints         "All breakpoints cleared"
syntax match  event               "Event:.*$"
syntax region location            start="#" end='$\n.*$'

highlight link stdin_tigaDebugger Todo
highlight link errorMsg           Error
highlight link warn_tigaDebugger  Error
highlight link msg_tigaDebugger   Function
highlight link msg_sdb            Statement
highlight link no_process         Constant
highlight link breakpoints        Comment
highlight link location           String
highlight link event              Type

let b:current_syntax = "tigaDebugger"

