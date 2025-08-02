\ The List struct.
\ A parking spot for the start of a list of lisks, or no links, that is, an empty list.
\ This struct wholly manages the link struct.

17971 constant list-id
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

\ Check list mma usage.
: assert-list-mma-none-in-use ( -- )
    list-mma mma-in-use 0<>
    if
        ." list-mma use GT 0"
        abort
    then
;

\ Start accessors.

\ Get list length.
: list-get-length ( list-addr -- u-length )
    2w@
;

\ Set list length, use only in this file.
: _list-set-length ( length-value list-addr -- )
    2w!
;

\ Get list first link.
: list-get-links ( list-addr -- list-links-value )
    list-links + @
;

\ Set list links, use only in this file.
: _list-set-links ( links-value list-addr -- )
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

    struct-get-id   \ Here the fetch could abort on an invalid address, like a random number.
    list-id =       \ An unallocated instance should have an ID of zero.
;

: is-not-allocated-list ( list -- flag )
    is-allocated-list 0=
;

\ Check arg0 for list, unconventional, leaves stack unchanged. 
: assert-arg0-is-list ( lst -- lst )
    dup is-not-allocated-list
    if
        cr ." arg0 is not an allocated list."
        abort
    then
;

\ Check arg1 for list, unconventional, leaves stack unchanged. 
: assert-arg1-is-list ( arg1 arg0 -- arg1 arg0 )
    over is-not-allocated-list
    if
        cr ." arg1 is not an allocated list."
        abort
    then
;

: list-inc-length ( list-addr -- )
    \ Check arg.
    assert-arg0-is-list

    dup list-get-length     \ list-addr count
    1+
    swap                    \ count list-addr
    _list-set-length
;

: list-dec-length ( list-addr -- )
    \ Check arg.
    assert-arg0-is-list

    dup list-get-length     \ list-addr count
    1-
    swap                    \ count list-addr
    _list-set-length
;


\ Return an new list struct instance address.
: list-new ( -- addr )
    \ Allocate space.
    list-mma mma-allocate       \ list-addr

    \ Init fields.
    list-id over struct-set-id  \ list-addr
    0 over _list-set-length     \ list-addr
    0 over _list-set-links      \ list-addr
    0 over struct-set-use-count \ list-addr
;


\ Add data to the end of a list.
\ If data is a struct, having a use count, caller to inc use count.
: list-push-end ( data list-addr -- )
    \ Check arg.
    assert-arg0-is-list

    \ Create new link.
    swap link-new               \ list link
    dup struct-inc-use-count    \ list link
    swap                        \ link-new list

    \ Increment list length
    dup list-inc-length         \ link-new

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
        _link-set-next
    else                        \ List is empty
        drop                    \ link-new list
        _list-set-links
    then
;

\ Add an address to the beginning of a list
\ If data is a struct, having a use count, caller to inc use count.
: list-push ( data list-addr -- )
    \ Check arg.
    assert-arg0-is-list

    \ Create new link.
    swap link-new               \ list link-new
    dup struct-inc-use-count    \ list link-new
    swap                        \ link-new list

    \ Increment list length
    dup list-inc-length         \ link-new list

    \ Store list header next pointer to link next pointer
    2dup                \ link-new list link-new list
    list-get-links      \ link-new list link-new first-link (first-link may be 0)
    swap                \ link-new list first-link link-new
    _link-set-next      \ link-new list

    \ Store link-new as first link.
    _list-set-links
;

\ Print a list.
: .list-raw ( addr -- )
    \ Check arg.
    assert-arg0-is-list

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
    \ Check arg.
    assert-arg0-is-list

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
    \ Check arg.
    assert-arg0-is-list

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
    \ Check arg.
    assert-arg0-is-list

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
\ If the data is a struct instance with a use count, that should be adjusted by the caller.
: list-remove ( xt item list -- data true | false )
    \ Check arg.
    assert-arg0-is-list

    \ Check for an empty list.
    dup list-get-length
    0=
    if
        \ Return false.
        2drop drop
        false
        exit
    then

    \ Check first link.     \ xt item list
    dup list-get-links      \ xt item list | link
    dup link-get-data       \ xt item list | link data

    dup                     \ xt item list | link data | data
    4 pick                  \ xt item list | link data | data item
    6 pick                  \ xt item list | link data | data item  xt
    execute                 \ xt item list | link data | flag

    if                      \ xt item list | link data
        \ Adjust first list pointer.
        swap                \ xt item list | data link
        dup link-get-next   \ xt item list | data link link-next
        3 pick              \ xt item list | data link link-next list
        _list-set-links     \ xt item list | data link
        
        \ Deallocate link.
        link-deallocate     \ xt item list | data

        \ Adjust list length.
        swap                \ xt item data list
        list-dec-length     \ xt item data

        \ Clean up stack.
        nip nip             \ data

        \ Add flag.
        true                \ data true
        exit
    else                    \ xt item list | link data
        2drop               \ xt item list
    then

    \ Check subsequent links.
    dup list-get-links      \ xt item list | last-link

    begin
        dup link-get-next   \ xt item list | last-link cur-link
        dup
    while                   \ xt item list | last-link cur-link
        dup link-get-data   \ xt item list | last-link cur-link | data
        4 pick              \ xt item list | last-link cur-link | data item
        6 pick              \ xt item list | last-link cur-link | data item xt
        execute             \ xt item list | last-link cur-link | flag

        if                  \ xt item list | last-link cur-link

            \ Set last-link link-next field.
            swap                    \ xt item list | cur-link last-link
            over link-get-next      \ xt item list | cur-link last-link link-next
            swap _link-set-next     \ xt item list | cur-link

            \ Get data to return.
            dup link-get-data       \ xt item list | cur-link data

            \ Deallocate link.
            swap link-deallocate    \ xt item list | data

            \ Adjust list length.
            swap                \ xt item data list
            list-dec-length     \ xt item data

            \ Cleanup stack.
            nip nip                 \ data

            true                    \ data true
            exit
        then                \ xt item list | last-link cur-link

        nip                 \ xt item list | cur-link
    repeat
    \ xt item list | last-link 0 ( last link-next field value )
    2drop 2drop drop
    false
;

\ Deallocate a list that has a use count of 1.
: list-deallocate-uc-1 ( list-addr -- )
    \ Check arg.
    assert-arg0-is-list

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
    0 over _list-set-length
    0 over _list-set-links

    list-mma mma-deallocate \ Deallocate list.
;

\ Deallocate a list.
\ If the link data is struct instance addresses, the caller may need to deallocated them first.
: list-deallocate ( list-addr -- )
    \ Check arg.
    assert-arg0-is-list

    dup struct-get-use-count        \ list-addr count

    dup 0 < 
    if  
        ." invalid use count" abort
    else
        2 < 
        if  
            list-deallocate-uc-1
        else
            struct-dec-use-count
        then
    then
;

\ Return the difference of two lists, same order as in subrtracting numbers in forth, list1 - list0
\ Provide an xt for determining data equality.
\ If list data are struct instances, the caller should inc the instance use count,
\ using xt list-ret list-apply.
: list-difference ( xt list1 list0 -- list )
    \ Check arg.
    assert-arg0-is-list
    assert-arg1-is-list

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
    \ Check args.
    assert-arg0-is-list
    assert-arg1-is-list

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
    \ Check arg.
    assert-arg0-is-list

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
    \ Check arg.
    assert-arg0-is-list
    assert-arg1-is-list

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
