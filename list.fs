\ The List struct.
\ A parking spot for the start of a list of lisks, or no links, that is, an empty list.
\ This struct wholly manages the link struct.

19317 constant list-id
    2 constant list-struct-number-cells

\ List struct fields.
0 constant  list-header             \ 16-bits [0] id [1] use count [2] length.
list-header cell+ constant list-links

0 value list-mma  \ Storage for list mma struct instance.

\ Init lisp, and lisk, mma.
: list-mma-init ( num-items -- ) \ Sets list-mma
    dup 1 <
    if
        ." list-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing List store."
    list-struct-number-cells swap mma-new  to list-mma
;

\ Start accessors.
: list-get-id ( list -- id-value )
    0w@
;

: list-set-id ( list -- )
    list-id swap 0w!
;

: list-get-use-count ( list -- uc-value )
    1w@
;

: list-set-use-count ( u-16 list -- )
    1w!
;

: list-get-length ( list-addr -- u-length )
    2w@
;

: list-set-length ( length-value list-addr -- )
    2w!
;

: list-get-links ( list-addr -- list-links-value )
    list-links + @
;

: list-set-links ( links-value list-addr -- )
    list-links + !
;
\ End accessors.

\ Check instance type.
: is-allocated-list ( list -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup list-mma mma-within-array 0=
    if
        drop false exit
    then

    list-get-id     \ Here the fetch could abort on an invalid address, like a random number.
    list-id =       \ An unallocated instance should have an ID of zero.
;

: is-not-allocated-list ( list -- flag )
    is-allocated-list 0=
;

\ Return an new list struct instance address.
: list-new ( -- addr )
    \ Allocate space.
    list-mma mma-allocate       \ link-addr

    \ Init fields.
    dup list-set-id             \ link-addr
    0 over list-set-length      \ link-addr
    0 over list-set-links       \ link-addr
    1 over list-set-use-count   \ link-addr
;


\ Add data to the end of a list.
: list-push-end ( data list-addr -- )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-push-end: Argument is not an allocated list"
        abort
    then

    \ Create new link.
    swap link-new swap          \ link-new list

    \ Increment list length
    dup list-get-length 1+      \ link-new list len+
    over list-set-length        \ link-new list

    \ Check for an empty list
    dup list-get-links          \ link-new list first-link
    dup if                      \ List is not empty.
        begin
            dup                 \ link-new list first-link first-link
        while
            nip
            dup link-get-next   \ link-new cur-link link-next
        repeat
        drop                    \ link-new link-last
        link-set-next
    else                        \ List is empty
        drop                    \ link-new list
        list-set-links
    then
;

\ Add an address to the beginning of a list
: list-push ( data list-addr -- ) 
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-push: Argument is not an allocated list"
        abort
    then

    \ Create new link.
    swap link-new swap          \ link-new list

    \ Increment list length
    dup list-get-length 1+      \ link-new list len+
    over list-set-length        \ link-new list

    \ Store list header next pointer to link next pointer
    2dup                \ link-new list link-new list
    list-get-links      \ link-new list link-new first-link (first-link may be 0)
    swap                \ link-new list first-link link-new
    link-set-next       \ link-new list

    \ Store link-new as first link.
    list-set-links
;

\ Print a list.
: .list-raw ( addr -- )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-push: Argument is not an allocated list"
        abort
    then

    dup list-get-length
    ." length " . ."  ("

    list-get-links              \ first-link
        begin                   \ List is not empty.
            dup                 \ link link
        while
            dup .link           \ Print link.
            link-get-next       \ link-next
        repeat
        drop

    ." )"
;

\ Print a list, given an xt to print the data.
: .list ( xt addr -- )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-push: Argument is not an allocated list"
        abort
    then

    ." ("

    list-get-links          \ xt first-link
    \ Print first item, if any, without leading space.
    dup
    if
        dup                 \ xt link link
        link-get-data       \ xt link data
        2 pick              \ xt link data xt
        execute             \ xt link
        link-get-next       \ xt link-next
    then

    \ Print subsequent items, if any.
    begin                   \ List is not empty.
        dup                 \ xt link link
    while
        space
        dup                 \ xt link link
        link-get-data       \ xt link data
        2 pick              \ xt link data xt
        execute             \ xt link
        link-get-next       \ xt link-next
    repeat
    ." )"
    2drop
;

\ Return true if a list contains an item, based on a given test execution token.
: list-member ( xt item list -- flag )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-member: Argument is not an allocated list"
        abort
    then

    list-get-links          \ xt item first-link
    begin                   \ List is not empty.
        dup                 \ xt item link link
    while                   \ xt item link
        2dup                \ xt item link item link
        link-get-data       \ xt item link item link-data
        4 pick              \ xt item link item link-data xt
        execute             \ xt item link flag
        if
            \ Return true.
            2drop drop
            true
            exit
        else
            link-get-next       \ link-next
        then
    repeat

    \ Cleanup, return false.
    2drop drop
    false
;


\ Return an data cell if a list contains an item, based on a given test execution token.
: list-find ( xt item list -- cell true | false )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-find: Argument is not an allocated list"
        abort
    then

    \ Check for an empty list.
    dup list-get-length
    0=
    if
        \ Return false.
        2drop drop
        false
        exit
    then

    list-get-links              \ xt item first-link
    begin                   \ List is not empty.
        dup                 \ xt item link link
    while                   \ xt item link
        2dup                \ xt item link item link
        link-get-data       \ xt item link item link-data
        4 pick              \ xt item link item link-data xt
        execute             \ xt item link flag
        if
            \ Return cell true.
            link-get-data   \ xt item data
            nip nip true
            exit
        else
            link-get-next       \ link-next
        then
    repeat

    \ Cleanup, return TOS, that is false.
    nip nip
;

\ Scan a list for the first item that returns true for the given xt, 
\ remove that link, returning the link-data contents.
: list-remove ( xt item list -- data true | false )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-remove: Argument is not an allocated list"
        abort
    then

    \ Check for an empty list.
    dup list-get-length
    0=
    if
        \ Return false.
        2drop drop
        false
        exit
    then

    -rot 2 pick             \ list xt item list
    list-get-links          \ list xt item first-link
    0 swap                  \ list xt item last-linx link, add cycle at-start flag
    begin
        dup                 \ list xt item last-link link link
    while                   \ list xt item last-link link
        2 pick over         \ list xt item last-link link item link
        link-get-data       \ list xt item last-link link item link-data
        5 pick              \ list xt item last-link link item link-data xt
        execute             \ list xt item last-link link last-link
        if                  \ list xt item last-link link
            \ Check last link
            over 0=                     \ list xt item last-link link flag
            if                          \ list xt item last-link link
                \ Handle first link.  Set list-links to link after (if any) the first link.
                dup                     \ list xt item last-link link link
                link-get-next           \ list xt item last-link link next-link
                5 pick                  \ list xt item last-link link next-link list
                list-set-links          \ list xt item last-link link
                dup link-get-data swap  \ list xt item last-link data link

                \ Deallocate link.
                link-deallocate     \ list xt item last-link data
                nip nip nip         \ list data

                \ Adjust list length.
                swap dup                \ data list list
                list-get-length 1-      \ data list new-len
                swap list-set-length    \ data
                true                    \ data flag
            else
                \ Handle subsequent link.
            then
            exit
        else                    \ list xt item last-link link
            nip dup             \ list xt item link link
            link-get-next       \ list xt item last-link next-link
        then
    repeat
    \ Cleanup stack.
    2drop 2drop drop
    \ Return negative result.
    false
;

\ Deallocate a list.
\ If the link data is struct instance addresses, the caller may need to deallocated them first.
: list-deallocate ( list-addr -- )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-deallocate: Arg is not an allocated list"
        abort
    then

    \ Deallocate links.
    dup list-get-links      \ list links
    begin
        dup                 \ list link link
    while                   \ list link
        dup link-get-next   \ list link link-next
        swap                \ list link-next link
        link-deallocate     \ list lnk-next
    repeat
    drop                    \ list

    \ Clear fields.
    0 over list-set-length
    0 over list-set-links

    list-mma mma-deallocate \ Deallocate list.
;

\ Return the difference of two lists, same order as in subrtracting numbers in forth, list1 - list0
\ Provide an xt for determining data equality.
\ If list data are struct instances, the caller should inc the instance use count,
\ using xt list-ret list-apply.
: list-difference ( xt list1 list0 -- list )
    dup is-not-allocated-list
    if  
        ." list-difference: arg0 is not an allocated list"
        abort
    then
    over is-not-allocated-list
    if  
        ." list-difference: arg1 is not an allocated list"
        abort
    then

    \ Allocate list to return.
    list-new                   \ xt list1 list0 list-ret

    \ Get first link of list1, if any.
    rot list-get-links          \ xt list0 list-ret link1
    begin
        dup                     \ xt list0 list-ret link1 link1
    while                       \ Check for link1 addr == zero.
                                \ xt list0 list-ret link1
        3 pick                  \ xt list0 list-ret link1 xt
        over link-get-data      \ xt list0 list-ret link1 xt data1
        4 pick                  \ xt list0 list-ret link1 xt data1 list0
        list-member 0=          \ xt list0 list-ret link1 flag
        if
                                \ xt list0 list-ret link1
            dup link-get-data   \ xt list0 list-ret link1 data1
            2 pick              \ xt list0 list-ret link1 data1 list-ret
            list-push           \ xt list0 list-ret link1
        then
                                \ xt list0 list-ret link1
        link-get-next           \ xt list0 list-ret link1-next
    repeat
    \ xt list0 list-ret 0
    drop nip nip                \ list-ret
;

\ Return the union of two lists.
\ Provide an xt for determining data equality.
\ If list data are struct instances, the caller should inc the instance use count,
\ using xt list-ret list-apply.
: list-union ( xt list1 list0 -- list )
    dup is-not-allocated-list
    if  
        ." list-union: arg0 is not an allocated list"
        abort
    then
    over is-not-allocated-list
    if  
        ." list-union: arg1 is not an allocated list"
        abort
    then

    \ Allocate list to return.
    list-new                    \ xt list1 list0 list-ret

    \ Get list0 items 
    swap list-get-links         \ xt list1 list-ret links0
    begin
        dup                     \ xt list1 list-ret link0 link0
    while                       \ Check for link1 addr == zero.
                                \ xt list1 list-ret link0
        3 pick                  \ xt list1 list-ret link0 xt
        over link-get-data      \ xt list1 list-ret link0 xt data0
        3 pick                  \ xt list1 list-ret link0 xt data0 list-ret
        list-member 0=          \ xt list1 list-ret link0 flag
        if
                                \ xt list1 list-ret link0
            dup link-get-data   \ xt list1 list-ret link0 data0 list-ret
            2 pick              \ xt list1 list-ret link0 data0 list-ret
            list-push           \ xt list1 list-ret link0
        then
        link-get-next           \ xt list1 list-ret link0-next
    repeat
                                \ xt list1 list-ret 0
    drop                        \ xt list1 list-ret

    \ Get list1 items 
    swap list-get-links         \ xt list-ret links1
    begin
        dup                     \ xt list-ret link1 link1
    while                       \ Check for link1 addr == zero.
                                \ xt list-ret link1
        2 pick                  \ xt list-ret link1 xt
        over link-get-data      \ xt list-ret link0 xt data1
        3 pick                  \ xt list-ret link0 xt data1 list-ret
        list-member 0=          \ xt list-ret link1 flag
        if
                                \ xt list-ret link1
            dup link-get-data   \ xt list-ret link1 data1 list-ret
            2 pick              \ xt list-ret link1 data1 list-ret
            list-push           \ xt list-ret link1
        then
        link-get-next           \ xt list-ret link1-next
    repeat
                                \ xt list-ret 0
    drop nip                    \ list-ret
;

\ Apply a function to each item in a list.
: list-apply ( xt list0 -- )
    dup is-not-allocated-list
    if  
        ." list-apply: list0 is not an allocated list"
        abort
    then

    list-get-links      \ xt links0
    begin
        dup
    while
        dup link-get-data       \ xt link0 data0
        2 pick                  \ xt link0 data0 xt
        execute                 \ xt link0
        link-get-next           \ xt link-next
    repeat
    \ xt 0
    2drop
;

\ Return the intersection of two lists.
: list-intersection ( xt list1 list0 -- list2 )
    dup is-not-allocated-list
    if  
        ." list-intersection: list0 is not an allocated list"
        abort
    then
    over is-not-allocated-list
    if  
        ." list-intersection: list1 is not an allocated list"
        abort
    then

    \ Allocate list to return.
    list-new                   \ xt list1 list0 list-ret

    \ Get first link of list1, if any.
    rot list-get-links          \ xt list0 list-ret link1
    begin
        dup                     \ xt list0 list-ret link1 link1
    while                       \ Check for link1 addr == zero.
                                \ xt list0 list-ret link1
        3 pick                  \ xt list0 list-ret link1 xt
        over link-get-data      \ xt list0 list-ret link1 xt data1
        4 pick                  \ xt list0 list-ret link1 xt data1 list0
        list-member             \ xt list0 list-ret link1 flag
        if
                                \ xt list0 list-ret link1
            dup link-get-data   \ xt list0 list-ret link1 data1
            2 pick              \ xt list0 list-ret link1 data1 list-ret

            \ Avoid dups in list-ret
            5 pick              \ xt list0 list-ret link1 data1 list-ret xt
            2 pick              \ xt list0 list-ret link1 data1 list-ret xt data1
            2 pick              \ xt list0 list-ret link1 data1 list-ret xt data1 list-ret
            list-member         \ xt list0 list-ret link1 data1 list-ret flag
            if
                2drop           \ xt list0 list-ret link1
            else
              list-push         \ xt list0 list-ret link1
            then
        then
                                \ xt list0 list-ret link1
        link-get-next           \ xt list0 list-ret link1-next
    repeat
    \ xt list0 list-ret 0
    drop nip nip                \ list-ret
;
