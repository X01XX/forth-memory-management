\ Pop the first struct from a list.
: list-pop-struct ( lst0 -- struct t | f )
    \ Check args.
    assert-tos-is-list

    list-pop        \ struct t | f
    if
        dup struct-dec-use-count
        true
    else
        false
    then
;

\ Pop the last struct from a list.
: list-pop-end-struct ( lst0 -- struct t | f )
    \ Check args.
    assert-tos-is-list

    list-pop-end    \ struct t | f
    if
        dup struct-dec-use-count
        true
    else
        false
    then
;

\ Push a struct to a list.
: list-push-struct ( struct1 list0 -- )
    \ Check args.
    assert-tos-is-list

    over struct-inc-use-count
    list-push
;

\ Push a struct to the end of a list.
: list-push-end-struct ( struct1 list0 -- )
    \ Check args.
    assert-tos-is-list

    over struct-inc-use-count
    list-push-end
;

\ Return the union of two lists of structs.
: list-union-struct ( xt list1 list0 -- list-result )
    list-union                          \ list-result
    [ ' struct-inc-use-count ] literal  \ list-result xt
    over list-apply                     \ list-result
;

\ Return the intersection of two lists of structs.
: list-intersection-struct ( xt list1 list0 -- list-result )
    list-intersection                   \ list-result
    [ ' struct-inc-use-count ] literal  \ list-result xt
    over list-apply                     \ list-result
;

\ Return the difference of two lists of structs.
: list-difference-struct ( xt list1 list0 -- list-result )
    list-difference                     \ list-result
    [ ' struct-inc-use-count ] literal  \ list-result xt
    over list-apply                     \ list-result
;

\ Return a list of items that return true for a given xt and data.
: list-find-all-struct ( xt data list0 -- list )
    \ Check args.
    assert-tos-is-list

    list-find-all                                   \ ret-list
    [ ' struct-inc-use-count ] literal over         \ ret-list xt ret-list
    list-apply                                      \ ret-list
;
