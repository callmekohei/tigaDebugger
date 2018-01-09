" ===========================================================================
"  FILE    : tigaDebugger_fsharp.vim
"  AUTHOR  : callmekohei <callmekohei at gmail.com>
"  License : MIT license
" ===========================================================================

" prompt and commands
syntax match prompt_cmd '(sdb).*$'
syntax match prompt_cmd '^run.*$'
syntax match prompt_cmd '^kill.*$'
syntax match prompt_cmd '^reset.*$'
syntax match prompt_cmd '^step over.*$'
syntax match prompt_cmd '^step into.*$'
syntax match prompt_cmd '^step out.*$'
syntax match prompt_cmd '^continue.*$'
syntax match prompt_cmd '^bp add at.*$'
syntax match prompt_cmd '^bp clear.*$'
syntax match prompt_cmd '^print.*$'
syntax match prompt_cmd '^watch add.*$'
syntax match prompt_cmd '^watch.*$'
highlight link prompt_cmd Type

" messages
syntax match  msg "Welcome to the Mono soft debugger.*$"
syntax match  msg "Type 'help' for a list of commands or 'quit' to exit"
syntax match  msg "Hit.*$"
syntax match  msg "No inferior process.*$"
syntax match  msg "No suspended inferior process.*$"
syntax match  msg "Event:.*$"
highlight link msg String

" warning and errors
syntax match  errorMsg "\(^.*exception.*$\|^.*Exception.*$\)"
highlight link warning Error

" thread and process
syntax match  thread_process "Inferior.*$"
highlight link thread_process Statement

" address
syntax region address start="#" end='$\n.*$'
highlight link address Underlined

let b:current_syntax = 'tigaDebugger_fsharp'


