" ===========================================================================
"  FILE    : tigaDebugger.vim
"  AUTHOR  : callmekohei <callmekohei at gmail.com>
"  License : MIT license
" ===========================================================================

augroup tigaDebugger
    autocmd!

    " run and kill
    let s:defalutKeymap_normal_ctrl_r = maparg('<C-r>','n')
    let s:defalutKeymap_normal_ctrl_k = maparg('<C-k>','n')

    " step and continue
    let s:defalutKeymap_normal_ctrl_n = maparg('<C-n>','n')
    let s:defalutKeymap_normal_ctrl_i = maparg('<C-i>','n')
    let s:defalutKeymap_normal_ctrl_u = maparg('<C-u>','n')
    let s:defalutKeymap_normal_ctrl_c = maparg('<C-c>','n')

    " breakpoints
    let s:defalutKeymap_normal_ctrl_b = maparg('<C-b>','n')
    let s:defalutKeymap_normal_ctrl_d = maparg('<C-d>','n')

    " print and watch
    let s:defalutKeymap_normal_ctrl_p = maparg('<C-p>','n')
    let s:defalutKeymap_normal_ctrl_y = maparg('<C-y>','n')
    let s:defalutKeymap_normal_ctrl_t = maparg('<C-t>','n')

    autocmd BufNewFile,BufRead *.c,*.fs,*.fsx,*.cs call s:command_custom()
    autocmd BufNewFile,BufRead,BufWrite *.c,*.fs,*.fsx,*.cs call s:nnoremap_custom()

augroup END


function! s:command_custom() abort

    " general command
    command! -nargs=? -buffer TigaCommand : call Tiga_Command(<f-args>)

    " start and quit
    command! -nargs=? -buffer -complete=customlist,DebuggerList TigaSetDebugger : call Tiga_Set_Debugger(<f-args>)
    command! -nargs=? -buffer -complete=file Tiga : call Tiga(<f-args>)
    command! -buffer TigaQuit                     : call Tiga_Quit()

    " run and reset
    command! -nargs=? -buffer -complete=file TigaRun : call Tiga_Run(<f-args>)
    command! -buffer TigaReset                       : call Tiga_Reset()

    " print and watch
    command! -nargs=? -buffer TigaPrint    : call Tiga_Print(<f-args>)
    command! -nargs=? -buffer TigaWatchAdd : call Tiga_WatchAdd(<f-args>)
    command! -nargs=? -buffer TigaWatchDel : call Tiga_WatchDel(<f-args>)

endfunction

function! DebuggerList(lead, line, pos)
  return ["sdb","gdb"]
endfunction


function! s:nnoremap_custom() abort

    " run and kill
    nnoremap <buffer> <C-r> :call <SID>tiga_Run_wrap()<CR>
    nnoremap <buffer> <C-k> :call <SID>tiga_Kill_wrap()<CR>

    " step and continue
    nnoremap <buffer> <C-n> :call <SID>tiga_StepOver_wrap()<CR>
    nnoremap <buffer> <C-i> :call <SID>tiga_StepInto_wrap()<CR>
    nnoremap <buffer> <C-u> :call <SID>tiga_StepOut_wrap()<CR>
    nnoremap <buffer> <C-c> :call <SID>tiga_Continue_wrap()<CR>

    " breakpoints
    nnoremap <buffer> <C-b> :call <SID>tiga_Breakpoints_wrap()<CR>
    nnoremap <buffer> <C-d> :call <SID>tiga_BreakpointsAllClear_wrap()<CR>

    " print and watch
    nnoremap <buffer><expr> <C-p> Tiga_IsAlive() ? ":\<C-u>TigaPrint "    : ":\<C-u>call <SID>tiga_Print_wrap()<CR>"
    nnoremap <buffer><expr> <C-y> Tiga_IsAlive() ? ":\<C-u>TigaWatchAdd " : ":\<C-u>call <SID>tiga_WatchAdd_wrap()<CR>"
    nnoremap <buffer><expr> <C-t> Tiga_IsAlive() ? ":\<C-u>TigaWatchDel " : ":\<C-u>call <SID>tiga_WatchDel_wrap()<CR>"

endfunction


function! s:tiga_Run_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_Run()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_r != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_r, "<CR>", "", "g"))
        else
            unmap <buffer> <C-r>
            :execute "normal \<C-r>"
        endif
    endif
endfunction

function! s:tiga_Kill_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_Kill()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_k != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_k, "<CR>", "", "g"))
        else
            unmap <buffer> <C-k>
            :execute "normal \<C-k>"
        endif
    endif
endfunction


function! s:tiga_StepOver_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_StepOver()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_n != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_n, "<CR>", "", "g"))
        else
            unmap <buffer> <C-n>
            :execute "normal \<C-n>"
        endif
    endif
endfunction

function! s:tiga_StepInto_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_StepInto()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_i != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_n, "<CR>", "", "g"))
        else
            unmap <buffer> <C-i>
            :execute "normal \<C-i>"
        endif
    endif
endfunction

function! s:tiga_StepOut_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_StepOut()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_u != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_u, "<CR>", "", "g"))
        else
            unmap <buffer> <C-u>
            :execute "normal \<C-u>"
        endif
    endif
endfunction

function! s:tiga_Continue_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_Continue()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_c != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_c, "<CR>", "", "g"))
        else
            unmap <buffer> <C-c>
            :execute "normal \<C-c>"
        endif
    endif
endfunction

function! s:tiga_Breakpoints_wrap() abort
    if Tiga_IsAlive()
        : call Tiga_Breakpoints()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_b != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_b, "<CR>", "", "g"))
        else
            unmap <buffer> <C-b>
            :execute "normal \<C-b>"
        endif
    endif
endfunction

function! s:tiga_BreakpointsAllClear_wrap() abort
    if Tiga_IsAlive()
        :call Tiga_BreakpointsAllClear()
    elseif ! Tiga_IsAlive()
        if s:defalutKeymap_normal_ctrl_d != ''
            execute(substitute(s:defalutKeymap_normal_ctrl_d, "<CR>", "", "g"))
        else
            unmap <buffer> <C-d>
            :execute "normal \<C-d>"
        endif
    endif
endfunction

function! s:tiga_Print_wrap() abort
    if s:defalutKeymap_normal_ctrl_p != ''
        let s = s:defalutKeymap_normal_ctrl_p
        let s = substitute(s,"<CR>","",'g')
        let s = substitute(s,":","",'g')
        execute(s)
    else
        unmap <buffer> <C-p>
        :execute "normal \<C-p>"
    endif
endfunction

function! s:tiga_WatchAdd_wrap() abort
    if s:defalutKeymap_normal_ctrl_y != ''
        let s = s:defalutKeymap_normal_ctrl_y
        let s = substitute(s,"<CR>","",'g')
        let s = substitute(s,":","",'g')
        execute(s)
    else
        unmap <buffer> <C-y>
        :execute "normal \<C-y>"
    endif
endfunction

function! s:tiga_WatchDel_wrap() abort
    if s:defalutKeymap_normal_ctrl_y != ''
        let s = s:defalutKeymap_normal_ctrl_y
        let s = substitute(s,"<CR>","",'g')
        let s = substitute(s,":","",'g')
        execute(s)
    else
        unmap <buffer> <C-t>
        :execute "normal \<C-t>"
    endif
endfunction

let g:list = []
function! Tiga_Handler(ch,msg) abort

    if len(g:list) > 1000
        g:list = []
    endif

    if stridx(a:msg,'(sdb)') > 0 || stridx(a:msg,'(gdb)') > 0
        call add(g:list,a:msg)
        call Tiga_HandlerPy(g:list)
    endif

    call add(g:list, a:msg)

endfunction


if !has('nvim')

    let s:tigaDebugger = yarp#py3('tigaDebugger_wrap')

" ------------------------------------------------------------------------
" Tiga Status
" ------------------------------------------------------------------------

    func! Tiga_IsAlive()
        return s:tigaDebugger.call('tiga_IsAlive')
    endfunc

" ------------------------------------------------------------------------
" Tiga Commands
" ------------------------------------------------------------------------

    " general command
    func! Tiga_Command(v)
        return s:tigaDebugger.call('tiga_Command',a:v)
    endfunc


    " set debugger, start and quit
    func! Tiga_Set_Debugger(v)
        return s:tigaDebugger.call('tiga_Set_Debugger',a:v)
    endfunc

    func! Tiga(v)
        return s:tigaDebugger.call('tiga',a:v)
    endfunc

    func! Tiga_Quit()
        return s:tigaDebugger.call('tiga_Quit')
    endfunc


    " run, break(kill process) and reset
    func! Tiga_Run()
        return s:tigaDebugger.call('tiga_Run')
    endfunc

    func! Tiga_Kill()
        return s:tigaDebugger.call('tiga_Kill')
    endfunc

    func! Tiga_Reset()
        return s:tigaDebugger.call('tiga_Reset')
    endfunc


    " step and continue
    func! Tiga_StepOver()
        return s:tigaDebugger.call('tiga_StepOver')
    endfunc

    func! Tiga_StepInto()
        return s:tigaDebugger.call('tiga_StepInto')
    endfunc

    func! Tiga_StepOut()
        return s:tigaDebugger.call('tiga_StepOut')
    endfunc

    func! Tiga_Continue()
        return s:tigaDebugger.call('tiga_Continue')
    endfunc


    " add and clear breakpoints
    func! Tiga_Breakpoints()
        return s:tigaDebugger.call('tiga_Breakpoints')
    endfunc

    func! Tiga_BreakpointsAllClear()
        return s:tigaDebugger.call('tiga_BreakpointsAllClear')
    endfunc


    " print variables and watch variables
    func! Tiga_Print(v)
        return s:tigaDebugger.call('tiga_Print',a:v)
    endfunc

    func! Tiga_WatchAdd(v)
        return s:tigaDebugger.call('tiga_WatchAdd',a:v)
    endfunc

    func! Tiga_WatchDel(v)
        return s:tigaDebugger.call('tiga_WatchDel',a:v)
endfunc


" ------------------------------------------------------------------------
" Tiga Handler
" ------------------------------------------------------------------------

    func! Tiga_HandlerPy(v)
        return s:tigaDebugger.call('tiga_HandlerPy',a:v)
    endfunc

endif
