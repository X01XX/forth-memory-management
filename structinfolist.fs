\ Functions for a list of structinfos.

0 value .stack-structs-xt

\ Check if tos is an empty list, or has a structinfo instance as its first item.                                             
: assert-tos-is-structinfo-list ( tos -- tos )
    assert-tos-is-list
    dup list-is-empty
    if
    else
        dup list-get-links link-get-data
        assert-tos-is-structinfo
        drop
    then
;

\ Deallocate a structinfo list.
: structinfo-list-deallocate ( structinfo-lst -- )
    dup struct-get-use-count                        \ structinfo-lst uc
    #2 < if
        [ ' structinfo-deallocate ] literal over    \ structinfo-lst xt structinfo-lst
        list-apply                                  \ Deallocate structinfo instances in the list.
    then
    list-deallocate                                 \ Deallocate list and links.
;

\ Find a structinfo instance in a list, by instance id, if any.
: structinfo-list-find ( id1 si-lst0 -- si true | false )
    \ Check args.
    assert-tos-is-structinfo-list

    [ ' structinfo-inst-id-eq ] literal -rot list-find
;

\ Return the length of the longest struct name.
: structinfo-list-max-name-length ( si-lst0 -- u )
    \ Check args.
    assert-tos-is-structinfo-list

    \ Init length counter.
    0 swap                       \ cnt si-lst0

    \ Prep for loop.
    list-get-links               \ cnt si-link

    begin
        ?dup
    while
        dup link-get-data       \ cnt si-link si
        structinfo-get-name     \ cnt si-link c-addr u
        nip                     \ cnt si-link u
        rot                     \ si-link u cnt
        max                     \ si-link cnt
        swap                    \ cnt si-link

        link-get-next
    repeat
;

\ Print memory use of structs.
: structinfo-list-print-memory-use ( si-lst0 -- )
   \ cr ." At start: " .stack-structs cr
    \ Check args.
    assert-tos-is-structinfo-list

    cr ." Memory use:"
    \ Get/store longest name length.
    dup structinfo-list-max-name-length     \ si-lst0 max
    swap                                    \ max si-lst0

   \ cr ." At mid 1: " .stack-structs cr
    \ Init total counter.
    0 swap                                  \ max tot si-lst0

    \ Prep for loop.
    list-get-links                          \ max tot si-link

    begin
        ?dup
    while
        dup link-get-data                   \ max tot si-link six

        \ Print struct name, and filler.
        dup                         \ max tot si-link six six
        structinfo-get-name         \ max tot si-link six c-addr u
        tuck                        \ max tot si-link six u c-addr u
        cr #4 spaces type           \ max tot si-link six u
        [char] : emit space
        #4 pick swap  - spaces      \ max tot si-link six

        \ Print memory use.
        structinfo-get-mma          \ max tot si-link mmax
        .mma-usage                  \ max tot si-link

        swap                        \ max si-link tot
        over link-get-data          \ max si-link tot six
        structinfo-get-mma          \ max si-link tot mma
        mma-get-total-memory-use    \ max si-link tot mem-use
        +                           \ max si-link tot
        swap                        \ max tot si-link
 
        link-get-next
    repeat
                                    \ max tot
    cr
    swap spaces 
    #116 spaces ." Total: " dup #8 dec.r
    cell / #9 spaces #6 dec.r
    cr .stack-structs-xt execute cr
;

\ Return true if the structinfo-list-store is using a given address
\ for a list, link, or structinfo instance.
: structinfo-list-store-using-addr?  ( addr -- bool )
    \ Check list itself.
    structinfo-list-store            \ addr store
    over =                          \ addr bool
    if
        drop
        true
        exit
    then

    \ Check links and stores.
    structinfo-list-store          \ addr store
    list-get-links                  \ addr lnk

    begin
        ?dup
    while                           \ addr lnk
        \ Check link.
        2dup =                      \ addr lnk bool
        if
            2drop
            true
            exit
        then

        \ Check structinfo.
        dup link-get-data           \ addr lnk stkinf
        #2 pick =                   \ addr lnk bool
        if
            2drop
            true
            exit
        then

        link-get-next
    repeat
                                    \ addr
    drop
    false
;

\ Print out addresses that are still in use,
\ except those in the structinfo-list-store.
\ Run like: <struct name>-mma .mma-in-use-except
\ Returns a count of printed addresses.
: .mma-in-use-except ( mma-addr -- )

    \ Setup for loop.
    dup mma-get-item-size swap      \ size mma
    dup _mma-get-stack swap         \ size stack mma
    dup _mma-get-end-addr swap      \ size stack end mma

    _mma-get-array                  \ size stack end next-item

    begin
        2dup <>
    while
        dup                         \ size stack end item item
        #3 pick                     \ size stack end item item stack
        stack-in                    \ size stack end item flag
        if
        else
            dup structinfo-list-store-using-addr?
            if
            else                    \ size stack end item
                cr dup ." In use: " hex.
            then
        then

        #3 pick                     \ size stack end item size
        +                           \ size stack end next-item
    repeat
    cr
    \ Clear stack
    2drop 2drop
;

\ Check all project struct instances are deallocated.
\ A lost higher-level struct instance will also complain about struct instances it contains.
: structinfo-list-project-deallocated ( snf-lst0 -- )
    \ Check args.
    assert-tos-is-structinfo-list

    dup list-get-links                      \ snf-lst0 snf-link

    begin
        ?dup
    while
        dup link-get-data                   \ snf-lst0 snf-link snfx
        dup structinfo-get-mma             \ snf-lst0 snf-link snfx snf-mma
        swap structinfo-get-inst-id        \ snf-lst0 snf-link snf-mma snf-id
        case
            \ Handle lists, except the list defining structinfo-list-store.
            #17971  of
                        dup mma-in-use      \ snf-lst0 snf-link snf-mma in-use
                        1 <> if
                            cr ." List instances not fully deallocated" cr
                            .mma-in-use-except
                        else
                            drop
                        then
                    endof
            \ Handle links, except the links in structinfo-list-store.
            #17137  of
                        dup mma-in-use          \ snf-lst0 snf-link snf-mma in-use
                        #3 pick                 \ snf-lst0 snf-link snf-mma in-use snf-lst0
                        list-get-length         \ snf-lst0 snf-link snf-mma in-use lst-len
                        <> if
                            cr ." Link instances not fully deallocated" cr
                            .mma-in-use-except
                        else
                            drop
                        then
                    endof
            \ Handle structinfo, except the instances in structinfo-list-store.
            #53731  of
                        dup mma-in-use          \ snf-lst0 snf-link snf-mma in-use
                        #3 pick                 \ snf-lst0 snf-link snf-mma in-use snf-lst0
                        list-get-length         \ snf-lst0 snf-link snf-mma in-use lst-len
                        <> if
                            cr ." structinfo instances not fully deallocated" cr
                            .mma-in-use-except
                        else
                            drop
                        then
                    endof
            \ Handle other structs.
                                            \ snf-lst0 snf-link snf-mma snf-id
            over mma-in-use                 \ snf-lst0 snf-link snf-mma snf-id u
            0<> if
                #2 pick link-get-data
                structinfo-get-name cr type space ." instances not fully deallocated" cr
                swap .mma-in-use
            else
                drop
            then
        endcase

        link-get-next
    repeat
                                    \ snf-lst
    drop
    assert-forth-stack-empty
;

\ Free heap of all mm_arrays.
: structinfo-list-free-heap ( snf-lst0 -- )
    \ Check args.
    assert-tos-is-structinfo-list

    \ Init count.
    dup list-get-length swap                \ cnt snf-lst0
    list-get-links                          \ cnt snf-link

    \ Gather all mm array addresses.
    begin
        ?dup
    while
        dup link-get-data                   \ cnt snf-link snfx
        structinfo-get-mma                  \ cnt snf-link snf-mma
        -rot                                \ snf-mma cnt snf-link

        link-get-next
    repeat

    \ Free each mm array.
                                            \ mma ... mma cnt
    0 do
        mma-free
    loop
;

\ Push a structinfo instance, insure no duplicat id.
: structinfo-list-push-end ( snf1 snf-lst0 -- )
    \ Check args.
    assert-tos-is-structinfo-list
    assert-nos-is-structinfo

    \ Check for duplicate struct id.
    [ ' structinfo-id-eq ] literal      \ snf1 snf-lst0 xt
    #2 pick #2 pick                     \ snf1 snf-lst0 xt snf1 snf-lst1
    list-member                         \ snf1 snf-lst0 bool
    abort" structinfo-list-push-end: Duplicat struct id?"

    list-push-end-struct
;

\ Push a structinfo instance, insure no duplicat id.
: structinfo-list-push ( snf1 snf-lst0 -- )
    \ Check args.
    assert-tos-is-structinfo-list
    assert-nos-is-structinfo

    \ Check for duplicate struct id.
    [ ' structinfo-id-eq ] literal      \ snf1 snf-lst0 xt
    #2 pick #2 pick                     \ snf1 snf-lst0 xt snf1 snf-lst1
    list-member                         \ snf1 snf-lst0 bool
    abort" structinfo-list-push: Duplicate struct id?"

    list-push-struct
;

\ Return true if an number/address refers to a strcut.
: is-struct? ( addr -- bool )
    get-first-word              \ w t | f
    if
        structinfo-list-store   \ w snf-lst
        structinfo-list-find    \ snf t | f
        if
            drop
            true
        else
            false
        then
    else
        false
    then
;

\ Return a structinfo instance for an address, if its a struct instance.
: get-structinfo ( addr -- snf t | f )
    get-first-word              \ w t | f
    if
        structinfo-list-store   \ w snf-lst
        structinfo-list-find    \ snf t | f
    else
        false
    then
;

\ Print a list of structures. 
: structinfo-list-print-struct-list ( lst0 -- )
    \ Check args.
    assert-tos-is-list

    ." ("

    list-get-links                      \ lst-link
    begin
        ?dup
    while
        dup link-get-data               \ lst-link data

        dup get-structinfo              \ lst-link data, snf t | f
        if                              \ lst-link data snf
            \ Print a struct instance.
            structinfo-get-print-xt     \ lst-link data xt
            execute                     \ lst-link

            link-get-next
            dup 0<> if space then
        else                            \ lst-link data
            \ Print a number.
            .

            link-get-next
        then
    repeat
    ." )"                               \
;

\ Deallocate a list of structures. 
: structinfo-list-deallocate-struct-list ( lst0 -- )
    \ Check args.
    assert-tos-is-list

    dup struct-get-use-count                \ lst0 uc
    dup 0 < abort" Invalid use count"

    #2 <                                    \ lst0 bool
    if
        dup list-get-links                  \ lst0 lst-link
        begin
            ?dup
        while
            dup link-get-data               \ lst0 lst-link link-data 
    
            dup get-structinfo              \ lst0 lst-link link-data, snf t | f
            if
                \ Deallocate struct instance.
                structinfo-get-deallocate-xt    \ lst0 lst-link link-struct xt
                execute                     \ lst0 lst-link 
            else
                \ Just a number.            \ lst0 lst-link u
                drop
            then
    
            link-get-next
        repeat
                                            \ lst0
        list-deallocate
    else                                    \ lst0
        struct-dec-use-count
    then
;

