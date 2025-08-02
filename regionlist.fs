\ Functions for region lists.

\ Deallocate a region list.
: region-list-deallocate ( list0 -- )
    [ ' region-deallocate ] literal over list-apply \ Deallocate region instances in the list.
    list-deallocate                                 \ Deallocate list and links.
;

\ Return the intersection of two region lists.
: region-list-set-intersection ( list1 list0 -- list-result )
    [ ' region-eq ] literal -rot        \ xt list1 list0
    list-intersection                   \ list-result
    [ ' struct-inc-use-count ] literal  \ list-result xt
    over list-apply                     \ list-result
;

\ Return the union of two region lists.
: region-list-set-union ( list1 list0 -- list-result )
    [ ' region-eq ] literal -rot        \ xt list1 list0
    list-union                          \ list-result
    [ ' struct-inc-use-count ] literal  \ list-result xt
    over list-apply                     \ list-result
;

\ Return the difference of two region lists.
: region-list-set-difference ( list1 list0 -- list-result )
    [ ' region-eq ] literal -rot        \ xt list1 list0
    list-difference                     \ list-result
    [ ' struct-inc-use-count ] literal  \ list-result xt
    over list-apply                     \ list-result
;

\ Print a region-list
: .region-list ( list0 -- )
    \ Check args.
    assert-arg0-is-list
    [ ' .region ] literal swap .list
;

\ Push a region to a region-list.
: _region-list-push ( reg1 list0 -- )
    \ Check args.
    assert-arg0-is-list
    assert-arg1-is-region

    over struct-inc-use-count
    list-push
;

\ Remove a region from a region-list, and deallocate.
\ xt signature is ( item list-data -- flag )
\ Return true if a region was removed.
: region-list-remove ( xt reg list -- bool )
    \ Check args.
    assert-arg0-is-list
    assert-arg1-is-region

    list-remove
    if
        region-deallocate
        true
    else
        false
    then
;

\ Push a region onto a list, if there are no supersets in the list.
\ If there are no supersets in the list, delete any subsets.
: region-list-push-nosubs ( reg1 list0 -- )
    \ Check args.
    assert-arg0-is-list
    assert-arg1-is-region

    \ Return if any region in the list is a superset of reg1.
    2dup                                    \ reg1 list0 reg1 list0
    [ ' region-superset-of ] literal        \ reg1 list0 reg1 list0 xt
    -rot                                    \ reg1 list0 xt reg1 list0
    list-member                             \ reg1 list0 flag
    if
        2drop
        false
        exit
    then
                                            \ reg1 list0

    begin
        2dup
        [ ' region-superset-of ] literal -rot \ reg1 list0 xt reg1 list0
        region-list-remove                  \ reg1 list0 | flag
    while
    repeat

    \ reg1 list0
    region-list-push
    true
;

\ Return a list of region intersections with a region-list, no subsets.
: region-list-region-intersections ( list1 list0 -- list-result )
    \ Check args.
    assert-arg0-is-list
    assert-arg1-is-list

    \ list1 list0
    list-get-links                  \ list1 link0
    list-new -rot                   \ ret-list list1 link0
    begin
        dup
    while
                                    \ ret-list list1 link0
        dup link-get-data           \ ret-list list1 link0 data0
        2 pick list-get-links       \ ret-list list1 link0 data0 link1

        begin
            dup
        while
            dup link-get-data       \ ret-list list1 link0 data0 link1 data1
            2 pick                  \ ret-list list1 link0 data0 link1 data1 data0
            region-intersection     \ ret-list list1 link0 data0 link1 | reg-int true | false
            if
                                        \ ret-list list1 link0 data0 link1 | reg-int
                dup                     \ ret-list list1 link0 data0 link1 | reg-int reg-int
                6 pick                  \ ret-list list1 link0 data0 link1 | reg-int reg-int ret-list
                region-list-push-nosubs \ ret-list list1 link0 data0 link1 | reg-int flag
                if
                    drop
                else
                    dup struct-inc-use-count
                    region-deallocate
                then
            then
                                    \ ret-list list1 link0 data0 link1
            link-get-next           \ ret-list list1 link0 data0 link1-next
        repeat
        2drop                       \ ret-list list1 link0
        link-get-next               \ ret-list list1 link0-next
    repeat
    \ ret-list list1 0
    2drop                           \ ret-list
;
