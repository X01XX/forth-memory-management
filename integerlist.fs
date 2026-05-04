
: integer-list-from-token-list ( tkn-lst -- int-lst t | f )
    \ Check arg.
    assert-tos-is-token-list

    \ Init stack.
    dup token-list-depth                    \ tkn-lst depth
    1+                                      \ tkn-lst depth+
    stack-new                               \ tkn-lst stk

    \ Init return list.
    list-new                                \ tkn-lst stk ret-lst
    over stack-push                         \ tkn-lst stk

    \ Prep for loop.
    swap                                    \ stk tkn-lst
    list-get-links                          \ stk tkn-link

    begin
        ?dup
    while
        \ Check for left paren.
        s" ("                               \ stk tkn-link c-addr u
        #2 pick link-get-data               \ stk tkn-link c-addr u tkt
        token-eq-string                     \ stk tkn-link flag
        if
            \ Process left paren.

            \ Start new list.
            list-new dup                    \ stk tkn-link next-list next-list

            \ Add new list to last list.
            #3 pick                         \ stk tkn-link next-list next-list stk
            stack-tos                       \ stk tkn-link next-list next-list last-list
            list-push-end-struct            \ stk tkn-link next-list

            \ Make new list tos.
            #2 pick                         \ stk tkn-link next-list stk
            stack-push                      \ stk tkn-link next-list
        else
            \ Check for right paren.
            s" )"                           \ stk tkn-link c-addr u
            #2 pick link-get-data           \ stk tkn-link c-addr u tkt
            token-eq-string                 \ stk tkn-link flag
            if
                \ Process right paren.
                over                        \ stk tkn-link stk
                stack-pop                   \ stk tkn-link list
                drop                        \ stk tkn-link
            else
                \ Process number token.
                dup link-get-data           \ stk tkn-link tkn
                token-get-string            \ stk tkn-link c-addr u
                snumber?                    \ stk tkn-link, n t | f
                if
                    \ Add number to list on stack.
                    #2 pick                 \ stk tkn-link n stk
                    stack-tos               \ stk tkn-link n top-list
                    list-push-end           \ stk tkn-link
                else
                    \ Process bad number.
                    drop                    \ stk

                    \ Get first list put on stack.
                    dup stack-pop swap      \ last-list stk
                    begin
                        dup stack-empty? invert
                    while
                        dup stack-pop       \ last-list stk next-list
                        rot drop            \ stk next-list
                        swap                \ next-list stk
                    repeat

                    \ Free stack.
                    free                    \ last-list flag
                    0<> abort" stack free failed?"

                    list-deallocate-recursive
                    false
                    exit
                then
            then
        then

        link-get-next
    repeat
                                            \ stk
    \ Get highest level list.
    dup stack-pop                           \ stk int-lst

    \ Free stack.
    swap free                               \ int-lst
    0<> abort" stack free failed"

    \ Avoid unneeded top-level list.
    dup list-get-first-item                 \ int-lst itm0
    is-allocated-list                       \ int-lst bool
    if
        dup list-get-length                 \ int-lst len
        1 =
        if
            \ Get rid of upper-level list.
            dup list-pop                    \ int-lst next-lst bool
            invert abort" pop failed?"
            swap list-deallocate            \ next-lst
        then
    then

    true
;

: .integer-list ( int-lst -- )
    \ Check arg.
    assert-tos-is-list

    [ ' . ] literal swap .list
;

: integer-list-deallocate ( int-lst -- )
    \ Check arg.
    assert-tos-is-list

    list-deallocate-recursive
;

