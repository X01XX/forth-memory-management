\ Print a stack, after an abort, giving information about structs on the stack.

\ Print the type of one value.
: .stack2 ( addr -- )
    dup link-mma mma-within-array
    if
        dup struct-get-id
        0= if
            ." link-u "
        else
            ." link "
        then
        drop
        exit
    then

    dup list-mma mma-within-array
    if
        dup struct-get-id
        0= if
            ." list-u "
        else
            ." list-"
            dup list-get-length dec.
        then
        drop
        exit
    then

    dup region-mma mma-within-array
    if
        dup struct-get-id
        0= if
            ." region-u "
        else
            ." region "
        then
        drop
        exit
    then

    \ More structs here, as needed.

    \ Default
    dec.
;

\ Cycle through each stack item, displaying its struct type.
: .stack
    depth 0=
    if
        exit
    then
    depth 0 do
        depth 1- i - pick
        .stack2 
    loop
;
