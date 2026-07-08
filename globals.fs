
: display-debug-status
    assert-level @ if
        cr ." debug on"
    else
        cr ." debug off"
    then
;

1 assert-level !    \ 0 to turn most asserts off, 1 to turn them on.
display-debug-status

' dup alias tos 
' over alias nos 

: 3os ( 3os nos tos -- 3os nos tos 3os )
    #2 pick
;

