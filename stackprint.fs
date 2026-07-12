\ Print a stack, after an abort, giving information about structs on the stack.

\ Print the type of one value.
: .stack-structs2 ( addr -- )
    dup link-mma mma-within-array
    if
        dup struct-get-id
        if
            ." link "
        else
            ." link-u "
        then
        drop
        exit
    then

    dup list-mma mma-within-array
    if
        dup struct-get-id
        if
            ." list-"
            dup list-get-length dec.
        else
            ." list-u "
        then
        drop
        exit
    then

    dup region-mma mma-within-array
    if
        dup struct-get-id
        if
            ." region "
        else
            ." region-u "
        then
        drop
        exit
    then

    \ Default
    dec.
;

\ Cycle through each stack item, displaying its struct type.
: .stack-structs
    ." Forth stack: <" depth dup abs 0 <# #S rot sign #> type ." > "
    depth
    ifnot
        exit
    then

    depth 0 do
        depth 1- i - pick

        dup list-mma mma-within-array
        if
            dup struct-get-id
            if
                ." list-"
                dup list-get-length dup abs 0 <# #S rot sign #> type
                dup list-get-length
                if
                    ." -"
                    dup list-get-links link-get-data
                    .stack-structs2
                else
                    space
                then
            else
                ." list-u "
            then
            drop
        else
            .stack-structs2
        then
    loop
;

