
: .num-list ( list-addr -- )
    [ ' . ] literal         \ Put dot xt on stack.
    swap                    \ xt list
    .list
;

\ Return true if a number if in a num-list
: num-list-member ( n num-list -- flag )
    \ Argument checking is possible.
    dup is-not-allocated-list
    if
        ." num-list-member: arg0 is not an allocated list"
        abort
    then

    ' =         \ n list xt
    -rot        \ xt n list
    list-member \ flag
;

\ Return the intersection of two num lists
: num-list-intersection ( list1 list0 -- list-intersection )
    ' =                 \ list1 list2 xt
    -rot                \ xt list1 list2
    list-intersection   \ list3
;

\ Return the difference of two num lists, same order as in subrtracting numbers in forth, list1 - list0
: num-list-difference ( list1 list0 -- list )
    ' =                 \ list1 list2 xt
    -rot                \ xt list1 list2
    list-difference     \ list3
;

\ Return the union of two num lists
: num-list-union ( list1 list0 -- list-union )
    ' =         \ list1 list2 xt
    -rot        \ xt list1 list2
    list-union  \ list3
;

: num-list-deallocate ( list-addr -- )
    list-deallocate
;

\ Given an xt and number, apply the xt of the number and each list number, returning a new list.
\
\ Like: ' + 3 list1 num-list-apply
\ Like: ' - 2 list1 num-list-apply
\ Like: ' * 4 list1 num-list-apply
\ Like: ' / 2 list1 num-list-apply
\ If calling from a word definition, use:  [ ' * ] literal
\ to put the xt onto the stack.
: num-list-apply ( xt u list0 -- list1 )
    list-new swap           \ xt u list-ret list0
    list-get-links          \ xt u list-ret link
    begin
        dup
    while
        dup link-get-data   \ xt u list-ret link data
        3 pick              \ xt u list-ret link data u
        5 pick              \ xt u list-ret link data u xt
        execute             \ xt u list-ret link result
        2 pick              \ xt u list-ret link result list-ret
        list-push           \ xt u list-ret link
        link-get-next
    repeat
    \ xt u list-ret 0
    drop nip nip
;
