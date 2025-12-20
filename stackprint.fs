\ Print a stack, after an abort, giving information about structs on the stack.

\ Print the type of one value.
: .stack-structs2 ( addr -- )
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

    \ Default
    dec.
;

\ Cycle through each stack item, displaying its struct type.
: .stack-structs
    depth 0=
    if
        exit
    then
    depth 0 do
        depth 1- i - pick

        dup list-mma mma-within-array
        if
            dup struct-get-id
            0= if
                ." list-u "
            else
                ." list-"
                dup list-get-length dup abs 0 <# #S rot sign #> type
                dup list-get-length
                0<> if
                        ." -"
                        dup list-get-links link-get-data
                        .stack-structs2
                    else
                        space
                    then
            then
            drop
        else
            .stack-structs2
        then
    loop
;

