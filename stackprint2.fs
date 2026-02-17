\ Print a stack, giving information about structs on the stack.

\ Print the type of one value.
: .stack-struct ( addr -- )
    dup get-first-word                                  \ addr, w t | f
    if
        \                                               \ addr w
        dup
        if
                                                        \ addr w-not-0
            \ Look up struct by id.
            structinfo-list-store                       \ addr w snf-lst
            structinfo-list-find                        \ addr, snf t | f
            if
                                                        \ addr snf
                structinfo-get-name                     \ addr c-addc u
                type                                    \ addr
                drop                                    \
            else
                \ Default                               \ addr
                dup abs 0 <# #S rot sign #> type        \
            then
        else
                                                        \ addr 0
            \ Look up possible unallocated struct.
            drop                                        \ addr
            structinfo-list-store                       \ addr snf-lst
            list-get-links                              \ addr snf-link

            begin
                ?dup
            while
                dup link-get-data                       \ addr snf-link snf
                #2 pick swap                            \ addr snf-link addr snf
                structinfo-get-mma                      \ addr snf-link addr mma
                mma-within-array                        \ addr snf-link bool
                if
                    dup  link-get-data                  \ addr snf-link snf
                    structinfo-get-name                 \ addr snf-link c-addr u
                    type                                \ addr snf-link
                    2drop                               \
                    ." -u"
                    exit
                then

                link-get-next
            repeat
                                                        \ addr
            \ Default
            dup abs 0 <# #S rot sign #> type            \
        then
    else
        \                                               \ addr
        \ Invalid memmory address, just print it.
        dup abs 0 <# #S rot sign #> type                \ addr
    then
;

\ Cycle through each stack item, displaying its struct type.
: .stack-structs
    ." Forth stack: <" depth dup abs 0 <# #S rot sign #> type ." > "
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
                ." List-u "
            else
                ." List-"
                dup list-get-length dup abs 0 <# #S rot sign #> type
                dup list-get-length
                0<> if
                        ." -"
                        dup list-get-links link-get-data
                        .stack-struct
                    else
                        space
                    then
            then
            drop
        else
            .stack-struct
        then
        space
    loop
;

' .stack-structs to .stack-structs-xt

