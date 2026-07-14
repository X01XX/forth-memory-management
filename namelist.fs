\ Functions for a list of names.

\ Check TOS for name-list.
: is-name-list? ( tos -- bool )
    dup is-list?            \ tos bool
    ifnot
        drop
        false
        exit
    then

    dup list-is-empty?      \ tos bool
    if
        drop
        true
        exit
    then

    list-get-links          \ link
    link-get-data           \ data
    is-name?                \ bool
;

\ Deallocate a name list.
: name-list-deallocate ( name-lst -- )
    \ Check arg.
    assert( tos is-name-list? )

    dup struct-get-use-count                    \ name-lst uc
    #2 < if
        [ ' name-deallocate ] literal over      \ name-lst xt name-lst
        list-apply                              \ Deallocate name instances in the list.

        list-deallocate                             \ Deallocate list and links.
    else
        struct-dec-use-count
    then
;

\ Return the intersection of two name lists.
\ Or use ' name-eq list1 list0 list-intersection-struct
: name-list-intersection ( name-list-addr1 name-list-addr0 -- name-list-result-addr )
    \ Check arg.
    assert( tos is-name-list? )
    assert( nos is-name-list? )

    [ ' name-eq ] literal -rot          \ xt name-list-addr1 name-list-addr0
    list-intersection                   \ name-list-result-addr
    [ ' struct-inc-use-count ] literal  \ name-list-result-addr xt
    over list-apply                     \ name-list-result-addr
;

\ Return the union of two name lists.
\ Or use ' name-eq list1 list0 list-union-struct
: name-list-union ( name-list-addr1 name-list-addr0 -- name-list-result-addr )
    \ Check arg.
    assert( tos is-name-list? )
    assert( nos is-name-list? )

    [ ' name-eq ] literal -rot          \ xt name-list-addr1 name-list-addr0
    list-union                          \ name-list-result-addr
    [ ' struct-inc-use-count ] literal  \ name-list-result-addr xt
    over list-apply                     \ name-list-result-addr
;

\ Return the difference of two name lists.
\ Or use ' name-eq list1 list0 list-difference-struct
: name-list-difference ( name-list-addr1 name-list-addr0 -- name-list-result-addr )
    \ Check arg.
    assert( tos is-name-list? )
    assert( nos is-name-list? )

    [ ' name-eq ] literal -rot          \ xt name-list-addr1 name-list-addr0
    list-difference                     \ name-list-result-addr
    [ ' struct-inc-use-count ] literal  \ name-list-result-addr xt
    over list-apply                     \ name-list-result-addr
;

