\ The List struct.
\ A parking spot for the start of a list of lisks, or no links, that is, an empty list.
\ This struct wholly manages the link struct.

19717 constant list-id
    3 constant list-struct-number-cells

0 constant  list-header
list-header cell+ constant list-length
list-length cell+ constant list-links

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
: list-get-id ( list -- header-value )
    list-header + @
;

: list-set-id ( list -- )
    list-header +
    list-id swap !
;

\ Get list length cell.
: list-get-length ( list-addr -- list-length-value )
    list-length + @
;

\ Set list length cell.
: list-set-length ( length-value list-addr -- )
    list-length + !
;

\ Get list links cell.
: list-get-links ( list-addr -- list-links-value )
    list-links + @
;

\ Set list links cell.
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

\ Return an new list instance.
: list-new ( -- addr )
    \ Allocate space.
    list-mma mma-allocate

    \ Init fields.
    dup list-set-id
    0 over list-set-length
    0 over list-set-links
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
: .list ( addr -- )
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
            dup                 \ first-link first-link
        while
            dup .link           \ Print link.
            link-get-next       \ link-next
        repeat
        drop

    ." )"
;

\ Return true if a list contains an item, based on a given test execution token.
: list-member ( xt item list -- flag )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-member: Argument is not an allocated list"
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
            \ Return true.
            2drop drop
            true
            exit
        else
            link-get-next       \ link-next
        then
    repeat

    \ Cleanup, return TOS, that is false.
    nip nip
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
    cr .s cr
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

    cr .s cr
    -rot 2 pick             \ list xt item list
    cr .s cr
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
: list-deallocate ( list-addr -- )
    \ Check argument.
    dup is-not-allocated-list
    if
        ." list-deallocate: Arg is not an allocated list"
        abort
    then

    \ Deallocate links.
    \ If the link data is struct instance addresses, the caller should have deallocated them.
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
