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

\ Return a list that is a copy of a given list, but with a specific item replaced by a given struct item.
: list-copy-except-struct ( new-item2 index1 lst0 -- lst )
     \ Check args.
    assert-tos-is-list
    over 0< abort" list-copy-except: index negative?"
    over over list-get-length < 0= abort" list-copy-except: index out of range?"

    list-get-links                  \ new-item2 index1 link
    list-new -rot                   \ new-item2 lst-new index1 link 
    begin
        ?dup
    while
        over 0=
        if
            #3 pick #3 pick         \ new-item2 lst-new index1 link new-item2 lst-new
            list-push-end-struct    \ new-item2 lst-new index1 link
        else
            dup link-get-data       \ new-item2 lst-new index1 link data
            #3 pick                 \ new-item2 lst-new index1 link data lst-new
            list-push-end-struct    \ new-item2 lst-new index1 link
        then

        \ Dec index.
        swap 1- swap
        link-get-next
    repeat
                                    \ new-item2 lst-new index1
    drop nip
;

