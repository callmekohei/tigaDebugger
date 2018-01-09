" ===========================================================================
"  FILE    : tigaDebugger.vim
"  AUTHOR  : callmekohei <callmekohei at gmail.com>
"  License : MIT license
" ===========================================================================

" prompt and commands
syntax match prompt_cmd '(gdb).*$'
syntax match prompt_cmd '^run.*$'
syntax match prompt_cmd '^next.*$'
syntax match prompt_cmd '^step.*$'
syntax match prompt_cmd '^continue.*$'
syntax match prompt_cmd '^break.*$'
syntax match prompt_cmd '^delete.*$'
syntax match prompt_cmd '^print.*$'
syntax match prompt_cmd '^watch.*$'
highlight link prompt_cmd Type

" messages
syntax match msg "^Starting program: .*"
highlight link msg String

" warning
syntax match warning "^warning:.*"
highlight link warning Error

" thread and process
syntax match thread_process "^\[.*Thread.*]$"
syntax match thread_process "^\[.*Inferior.*]$"
highlight link thread_process Statement

" variables
syntax match zzz '^\w.*\s=.*$'
highlight link zzz Character

" line number
syntax match ccc '^\d.*$'
highlight link ccc Underlined

let b:current_syntax = 'tigaDebugger_c'
