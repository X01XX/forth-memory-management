\ The List struct.
\ A parking spot for the start of a list of links, or no links, that is, an empty list.
\ This struct wholly manages the link struct.

\ Comparing any two items in a list is easier than in a higher-level language!
\                                       \ list
\    list-get-links                     \ link
\
\   \ For each item.
\   begin
\       ?dup
\   while
\        \ Get link to following items.
\        \ Having direct access to the list links makes this logic effortless,
\        \ compared to using indices at a higher level.
\        dup link-get-next               \ link link+
\
\        \ For each following item.
\        begin
\           ?dup
\       while
\           over link-get-data          \ link link+ dat0
\           over link-get-data          \ link link+ dat0 dat+
\           .
\           \ do comparison.
\           .
\           link-get-next               \ link link+
\       repeat
\
\       link-get-next                   \ link
\   repeat

#17971 constant list-id
    #2 constant list-struct-number-cells

\ List struct fields.
0 constant  list-header             \ 16-bits [0] struct id [1] use count [2] length.
list-header cell+ constant list-links

0 value list-mma  \ Storage for list mma struct instance.

\ Init lisp, and lisk, mma.
: list-mma-init ( num-items -- ) \ Sets list-mma
    dup 1 <
    abort" list-mma-init: Invalid number items."

    cr ." Initializing List store."
    list-struct-number-cells swap mma-new  to list-mma
;

\ Check list mma usage.
: assert-list-mma-none-in-use ( -- )
    list-mma mma-in-use 0<>
    abort" list-mma use GT 0"
;

\  Return true if TOS is an allocated list.
: is-allocated-list ( list -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup list-mma mma-within-array 0=
    if
        drop false exit
    then

    struct-get-id   \ Here the fetch could abort on an invalid address, like a random number.
    list-id =       \ An unallocated instance should have an ID of zero.
;

\ Check TOS for list, unconventional, leaves stack unchanged. 
: assert-tos-is-list ( lst0 -- lst0 )
    dup is-allocated-list 0=
    abort" TOS is not an allocated list."
;

\ Check NOS for list, unconventional, leaves stack unchanged. 
: assert-nos-is-list ( lst1 arg0 -- lst1 arg0 )
    over is-allocated-list 0=
    abort" NOS is not an allocated list."
;

\ Check 3OS for list, unconventional, leaves stack unchanged. 
: assert-3os-is-list ( lst2 arg1 arg0 -- lst2 arg1 arg0 )
    #2 pick is-allocated-list 0=
    abort" 3OS is not an allocated list."
;

\ Start accessors.

\ Get list length.
: list-get-length ( list-addr -- u-length )
    \ Check arg.
    assert-tos-is-list

    2w@
;

\ Set list length, use only in this file.
: _list-set-length ( length-value list-addr -- )
    2w!
;

\ Get list first link.
: list-get-links ( list-addr -- list-links-value )
    \ Check arg.
    assert-tos-is-list

    list-links + @
;

\ Set list links, use only in this file.
: _list-set-links ( links-value list-addr -- )
    list-links + !
;

\ Increment the list length.
: _list-inc-length ( list-addr -- )
    dup list-get-length     \ list-addr count
    1+
    swap                    \ count list-addr
    _list-set-length
;

\ Decrement the list length.
: _list-dec-length ( list-addr -- )
    dup list-get-length     \ list-addr count
    1-
    swap                    \ count list-addr
    _list-set-length
;

\ End accessors.

\ Return an new list struct instance address.
: list-new ( -- addr )
    \ Allocate space.
    list-mma mma-allocate       \ list-addr

    \ Init fields.
    list-id over struct-set-id  \ list-addr
    0 over _list-set-length     \ list-addr
    0 over _list-set-links      \ list-addr
    0 over struct-set-use-count \ list-addr
    \ cr ." list-new: " dup . cr
;

\ Return true if a list is empty.
: list-is-empty ( list0 -- flag )
    \ Check arg.
    assert-tos-is-list

    list-get-length
    0=
;

\ Add data to the end of a list.
\ If data is a struct, having a use count, caller to inc use count.
\ e.g.
\ alist of link-data = number.
\ #5 swap list-push-end
: list-push-end ( data list-addr -- )
    \ Check arg.
    assert-tos-is-list

    \ Create new link.
    swap link-new               \ list link
    dup struct-inc-use-count    \ list link
    swap                        \ link-new list

    \ Increment list length
    dup _list-inc-length        \ link-new

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
\
\ If list data are struct instances, the caller should inc the instance use count befere pushing,
\ using: dup struct-inc-use-count
\ e.g.
\ alist of link-data = number.
\ #5 swap list-push
: list-push ( data list-addr -- )
    \ Check arg.
    assert-tos-is-list

    \ Create new link.
    swap link-new               \ list link-new
    dup struct-inc-use-count    \ list link-new
    swap                        \ link-new list

    \ Increment list length
    dup _list-inc-length        \ link-new list

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
    assert-tos-is-list

    dup list-get-length
    ." length " dec. ."  ("

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
\ xt signature is ( link-data -- )
\ e.g.
\ alist of link-data = number.
\ [ ' . ] literal swap .list
: .list ( xt addr -- )
    \ Check arg.
    assert-tos-is-list

    ." ("

    list-get-links          \ xt first-link
    \ Print first item, if any, without leading space.
    dup
    if
        dup                 \ xt link link
        link-get-data       \ xt link data
        #2 pick             \ xt link data xt
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
        #2 pick             \ xt link data xt
        execute             \ xt link
        link-get-next       \ xt link-next
    repeat
    ." )"
    2drop
;

\ Return true if a list contains an item, based on a given test execution token.
\ xt signature is ( item link-data -- flag ) or ( filler link-data -- flag )
\ e.g.
\ alist of link-data = number.
\ [ ' = ] literal swap #5 swap list-member
: list-member ( xt item list -- flag )
    \ Check arg.
    assert-tos-is-list

    list-get-links          \ xt item first-link
    begin                   \ List is not empty.
        dup                 \ xt item link link
    while                   \ xt item link
        2dup                \ xt item link item link
        link-get-data       \ xt item link item link-data
        #4 pick             \ xt item link item link-data xt
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

\ Return the first data cell of a link, based on a given test execution token and test item.
\ xt signature is ( item link-data -- flag ) or ( filler link-data -- flag )
\ e.g.
\ alist of link-data = number.
\ [ ' = ] literal swap #5 swap list-find
: list-find ( xt item list -- cell true | false )
    \ Check arg.
    assert-tos-is-list

    \ Check for an empty list.
    dup list-get-length
    0=
    if
        \ Return false.
        2drop drop
        false
        exit
    then

    list-get-links          \ xt item first-link
    begin
        dup                 \ xt item link link
    while                   \ xt item link
        2dup                \ xt item link item link
        link-get-data       \ xt item link item link-data
        #4 pick             \ xt item link item link-data xt
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

\ Return a list containing items that match a given test execution token and test item.
\ xt signature is ( item link-data -- flag ) or ( filler link-data -- flag )
\ e.g.
\ alist of link-data = number.
\ [ ' < ] literal swap #5 swap list-find-all
: list-find-all ( xt item list -- list )
    \ Check arg.
    assert-tos-is-list

                        \ xt item list
    rot                 \ item list xt
    list-new            \ item list xt ret
    swap                \ item list ret xt
    2swap               \ ret xt item list

    \ Check for an empty list.
    dup list-get-length
    0=
    if
        \ Return false.
        2drop drop
        exit
    then

    list-get-links          \ ret xt item first-link
    begin
        dup                 \ ret xt item link link
    while                   \ ret xt item link
        2dup                \ ret xt item link item link
        link-get-data       \ ret xt item link item link-data
        #4 pick             \ ret xt item link item link-data xt
        execute             \ ret xt item link flag
        if
            \ Get data.
            dup link-get-data   \ ret xt item link data
            #4 pick             \ ret xt item link data ret
            list-push           \ ret xt item link
        then
        link-get-next       \ ret xt item link-next
    repeat

    \ Cleanup.
    drop 2drop              \ ret-list
;

\ Pop an item from the beginning of a list.
\ If list data are struct instances, the caller should dec the instance use count of the result,
\ using: if dup struct-dec-use-count then
: list-pop ( lst0 -- data true | false )
    \ Check arg.
    assert-tos-is-list

    dup list-is-empty
    if
        drop
        false
        exit
    then
    
    \ Adjust first list pointer.
    dup list-get-links  \ lst0 link
    dup link-get-next   \ lst0 link link-next
    #2 pick             \ lst0 link link-next lst0
    _list-set-links     \ lst0 link

    \ Deallocate link.
    dup link-get-data   \ lst0 link data
    swap                \ lst0 data link
    link-deallocate     \ lst0 data

    \ Adjust list length.
    swap                \ data lst0
    _list-dec-length    \ data

    \ Add flag.
    true                \ data true
;

\ Scan a list for the first item that returns true for the given xt, 
\ remove that link, returning the link-data contents.
\ xt signature is ( item link-data -- flag ) or ( filler link-data -- flag )
\
\ If the data is a struct instance with a use count, that should be adjusted by the caller.
\
\ I like this one. Standard tradecraft, as L. Ron Hubbard once wrote.
\
\ e.g.
\ alist of link-data = number.
\ [ ' = ] literal swap #5 swap list-remove
: list-remove ( xt item list -- data true | false )
    \ Check arg.
    assert-tos-is-list

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

    #3 pick                 \ xt item list | link data | item
    over                    \ xt item list | link data | item data
    #6 pick                 \ xt item list | link data | item data xt
    execute                 \ xt item list | link data | flag

    if                      \ xt item list | link data
        2drop               \ xt item list
        nip nip             \ list
        list-pop
        exit
    else                    \ xt item list | link data
        drop                \ xt item list | link
    then

    \ Check subsequent links.
    begin
        dup link-get-next   \ xt item list | last-link cur-link
        dup
    while                   \ xt item list | last-link cur-link
        #3 pick             \ xt item list | last-link cur-link | item
        over link-get-data  \ xt item list | last-link cur-link | item data
        #6 pick             \ xt item list | last-link cur-link | item data xt
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
            _list-dec-length    \ xt item data

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

\ Remove an item based on index.
\ e.g.
\ alist of link-data = anything.
\ #2 swap list-remove-item
\
\ If list data are struct instances, the caller should dec the instance use count of the result,
\ using: if dup struct-dec-use-count then
: list-remove-item ( u1 lst0 -- data true | false )
    \ Check args.
    assert-tos-is-list
    over                        \ u1 lst0 u1
    0< abort" index LT zero?"

    over                        \ u1 lst0 u1
    over list-get-length        \ u1 lst0 u1 l-len
    >= abort" index too large?" \ u1 lst0

    \ Check for first item
    over 0= if
        nip
        list-pop
        exit
    then
                                \ u1 lst0
    tuck                        \ lst0 u1 lst0
    list-get-links              \ lst0 u1 last-link
    dup                         \ lst0 u1 last-link cur-link
    rot                         \ lst0 last-link cur-link u1
    0 do                        \ lst0 last-link cur-link
        nip                     \ lst0 last-link'
        dup link-get-next       \ lst0 last-link' cur-link
    loop
                                \ lst0 last-link cur-link
    dup link-get-data -rot      \ lst0 data last cur-link
    dup link-get-next           \ lst0 data last cur-link cur-next
    swap link-deallocate        \ lst0 data last cur-next
    swap _link-set-next         \ lst0 data
    rot _list-dec-length        \ data
    true
;

\ Pop the last item in a list.
\ If list data are struct instances, the caller should dec the instance use count of the result,
\ using: if dup struct-dec-use-count then
: list-pop-end ( lst0 -- data true | false )
    \ Check arg.
    assert-tos-is-list

    dup list-is-empty
    if drop false exit then

    dup list-get-length 1-
    swap list-remove-item
;

\ Deallocate a list that has a use count of 1.
: _list-deallocate-uc-1 ( list-addr -- )
    \ Check arg.
    assert-tos-is-list

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
\
\ If the list is a list of structs using use count, dec the use count of the structs, or not, before
\ calling this.
\ e.g.: dup struct-get-use-count #2 < if [ ' <struct-name>-deallocate ] literal over list-apply then
: list-deallocate ( list-addr -- )
    \ Check arg.
    assert-tos-is-list

    dup struct-get-use-count        \ list-addr count

    dup 0 < 
    abort" invalid use count"

    #2 < 
    if  
        _list-deallocate-uc-1
    else
        struct-dec-use-count
    then
;

\ Return the difference of two lists, same order as in subrtracting numbers in forth, list1 - list0
\ Provide an xt for determining data equality.
\ xt signature is ( link-data link-data -- flag )
\
\ If list data are struct instances, the caller should inc the instance use count of the result,
\ using: [ ' struct-inc-use-count ] literal over list-apply.
: list-difference ( xt list1 list0 -- list )
    \ Check arg.
    assert-tos-is-list
    assert-nos-is-list

    \ Allocate list to return.
    list-new                   \ xt list1 list0 list-ret

    \ Get first link of list1, if any.
    rot list-get-links          \ xt list0 list-ret link1
    begin
        dup                     \ xt list0 list-ret link1 link1
    while                       \ Check for link1 addr == zero.
                                \ xt list0 list-ret link1
        #3 pick                 \ xt list0 list-ret link1 xt
        over link-get-data      \ xt list0 list-ret link1 xt data1
        #4 pick                 \ xt list0 list-ret link1 xt data1 list0
        list-member 0=          \ xt list0 list-ret link1 flag
        if
                                \ xt list0 list-ret link1
            dup link-get-data   \ xt list0 list-ret link1 data1
            #2 pick             \ xt list0 list-ret link1 data1 list-ret
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
\ xt signature is ( link-data link-data -- flag )
\
\ If list data are struct instances, the caller should inc the instance use count of the result,
\ using: [ ' struct-inc-use-count ] literal over list-apply.
: list-union ( xt list1 list0 -- list )
    \ Check args.
    assert-tos-is-list
    assert-nos-is-list

    \ Allocate list to return.
    list-new                    \ xt list1 list0 list-ret

    \ Get list0 items 
    swap list-get-links         \ xt list1 list-ret links0
    begin
        dup                     \ xt list1 list-ret link0 link0
    while                       \ Check for link1 addr == zero.
                                \ xt list1 list-ret link0
        #3 pick                 \ xt list1 list-ret link0 xt
        over link-get-data      \ xt list1 list-ret link0 xt data0
        #3 pick                 \ xt list1 list-ret link0 xt data0 list-ret
        list-member 0=          \ xt list1 list-ret link0 flag
        if
                                \ xt list1 list-ret link0
            dup link-get-data   \ xt list1 list-ret link0 data0 list-ret
            #2 pick             \ xt list1 list-ret link0 data0 list-ret
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
        #2 pick                 \ xt list-ret link1 xt
        over link-get-data      \ xt list-ret link0 xt data1
        #3 pick                 \ xt list-ret link0 xt data1 list-ret
        list-member 0=          \ xt list-ret link1 flag
        if
                                \ xt list-ret link1
            dup link-get-data   \ xt list-ret link1 data1 list-ret
            #2 pick             \ xt list-ret link1 data1 list-ret
            list-push           \ xt list-ret link1
        then
        link-get-next           \ xt list-ret link1-next
    repeat
                                \ xt list-ret 0
    drop nip                    \ list-ret
;

\ Apply a function to each item in a list.
\ xt signature is ( link-data -- )
: list-apply ( xt list0 -- )
    \ Check arg.
    assert-tos-is-list

    list-get-links      \ xt links0
    begin
        dup
    while
        dup link-get-data       \ xt link0 data0
        #2 pick                 \ xt link0 data0 xt
        execute                 \ xt link0
        link-get-next           \ xt link-next
    repeat
    \ xt 0
    2drop
;

\ Return the intersection of two lists.
\ xt signature is ( link-data link-data -- flag )
\
\ If list data are struct instances, the caller should inc the instance use count of the result,
\ using: [ ' struct-inc-use-count ] literal over list-apply.
: list-intersection ( xt list1 list0 -- list2 )
    \ Check args.
    assert-tos-is-list
    assert-nos-is-list

    \ Allocate list to return.
    list-new                   \ xt list1 list0 list-ret

    \ Get first link of list1, if any.
    rot list-get-links          \ xt list0 list-ret link1
    begin
        dup                     \ xt list0 list-ret link1 link1
    while                       \ Check for link1 addr == zero.
                                \ xt list0 list-ret link1
        #3 pick                 \ xt list0 list-ret link1 xt
        over link-get-data      \ xt list0 list-ret link1 xt data1
        #4 pick                 \ xt list0 list-ret link1 xt data1 list0
        list-member             \ xt list0 list-ret link1 flag
        if
                                \ xt list0 list-ret link1
            dup link-get-data   \ xt list0 list-ret link1 data1
            #2 pick             \ xt list0 list-ret link1 data1 list-ret

            \ Avoid dups in list-ret
            #5 pick             \ xt list0 list-ret link1 data1 list-ret xt
            #2 pick             \ xt list0 list-ret link1 data1 list-ret xt data1
            #2 pick             \ xt list0 list-ret link1 data1 list-ret xt data1 list-ret
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

\ Return a data item, based on an index into a list.
: list-get-item ( u list -- data )
    \ Check args.
    assert-tos-is-list

    over                        \ u list u
    over list-get-length        \ u list u len
    over                        \ u list u len u
    0<                          
    abort" index LT 0"
                                \ u list u len
    >=
    abort" index too large"
                                \ u list
    \ Step through links the given number of times.
    0 swap                      \ u count list
    list-get-links              \ u count link

    begin
        -rot 2dup <>            \ link u count
    while
        1+
        rot                     \ u count+ link
        link-get-next
    repeat

    \ Get data to return.
    2drop link-get-data
;

\ Sort a list, given an xt that returns true if two successive items
\ should be swapped.
\ e.g.
\ alist of link-data = number.
\ [ ' < ] literal over list-sort
: list-sort ( xt list -- )
    \ Check args.
    assert-tos-is-list

    begin
        \ Go through a list once.  Return true if any pairs have been swapped.
        false                           \ xt list bool
        over list-get-links             \ xt list bool link

        begin
            ?dup
        while
            dup link-get-next           \ xt list bool link link+
            ?dup
            if
                over link-get-data      \ xt list bool link link+ data
                over link-get-data      \ xt list bool link link+ data data+
                #6 pick                 \ xt list bool link link+ data data+ xt
                execute                 \ xt list bool link link+ bool
                if
                                        \ xt list bool link link+
                    over link-get-data  \ xt list bool link link+ data
                    over link-get-data  \ xt list bool link link+ data data+
                    #3 pick             \ xt list bool link link+ data data+ link
                    _link-set-data      \ xt list bool link link+ data
                    swap                \ xt list bool link data link+
                    _link-set-data      \ xt list bool link
                    swap drop           \ xt list link
                    true swap           \ xt list bool link
                else
                    drop                \ xt list bool link
                then
            then

            link-get-next               \ xt list bool link
        repeat
                                    \ xt list bool
        0=
    until
                                    \ xt list
    2drop
;
