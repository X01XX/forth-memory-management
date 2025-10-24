\ Functions for a list of names.

\ Deallocate a name list.
: name-list-deallocate ( name-lst -- )
    dup struct-get-use-count                    \ name-lst uc
    2 < if
        [ ' name-deallocate ] literal over      \ name-lst xt name-lst
        list-apply                              \ Deallocate name instances in the list.
    then
    list-deallocate                             \ Deallocate list and links.
;

\ Return the intersection of two name lists.
: name-list-intersection ( name-list-addr1 name-list-addr0 -- name-list-result-addr )
    [ ' name-eq ] literal -rot          \ xt name-list-addr1 name-list-addr0
    list-intersection                   \ name-list-result-addr
    [ ' struct-inc-use-count ] literal  \ name-list-result-addr xt
    over list-apply                     \ name-list-result-addr
;

\ Return the union of two name lists.
: name-list-union ( name-list-addr1 name-list-addr0 -- name-list-result-addr )
    [ ' name-eq ] literal -rot          \ xt name-list-addr1 name-list-addr0
    list-union                          \ name-list-result-addr
    [ ' struct-inc-use-count ] literal  \ name-list-result-addr xt
    over list-apply                     \ name-list-result-addr
;

\ Return the difference of two name lists.
: name-list-difference ( name-list-addr1 name-list-addr0 -- name-list-result-addr )
    [ ' name-eq ] literal -rot          \ xt name-list-addr1 name-list-addr0
    list-difference                     \ name-list-result-addr
    [ ' struct-inc-use-count ] literal  \ name-list-result-addr xt
    over list-apply                     \ name-list-result-addr
;

