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

\ Check all project instances are deallocated.
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
            \ Handle links.
            #17137   of
                        mma-in-use          \ snf-lst0 snf-link in-use
                        #2 pick             \ snf-lst0 snf-link in-use snf-lst0
                        list-get-length     \ snf-lst0 snf-link in-use lst-len
                        <> abort" Links left over"
                    endof
            \ Handle lists.
            #17971   of
                        mma-in-use          \ snf-lst0 snf-link in-use
                        1 <> abort" Lists left over"
                    endof
            \ Handle structinfo.
            #53731   of
                        mma-in-use          \ snf-lst0 snf-link in-use
                        #2 pick             \ snf-lst0 snf-link in-use snf-lst0
                        list-get-length     \ snf-lst0 snf-link in-use lst-len
                        <> abort" structinfo left over"
                    endof
            \ Handle other structs.
            swap
            mma-in-use                      \ snf-lst0 snf-link u
            0<> if
                over link-get-data
                structinfo-get-name cr type space ." instances not fully deallocated" cr
                abort
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
        structinfo-get-mma                 \ cnt snf-link snf-mma
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

\ Print a list of structures. 
: structinfo-list-print-struct-list ( lst0 -- )
    \ Check args.
    assert-tos-is-list

    structinfo-list-store               \ lst0 snf-lst

    ." ("

    over list-get-links                 \ lst0 snf-lst lst-link
    begin
        ?dup
    while
        dup link-get-data               \ lst0 snf-lst lst-link struct

        dup get-first-word              \ lst0 snf-lst lst-link struct, w t | f
        if
            #3 pick                     \ lst0 snf-lst lst-link struct w snf-lst
            structinfo-list-find        \ lst0 snf-lst lst-link struct, snf t | f
            if
                structinfo-get-print-xt \ lst0 snf-lst lst-link struct xt
                execute                 \ lst0 snf-lst lst-link
                false                   \ lst0 snf-lst lst-link bool
            else
                . true                  \ lst0 snf-lst lst-link bool
            then
        else
            . true                      \ lst0 snf-lst lst-link true
        then

        \ Skip extra space after a number.
        if
           link-get-next
        else
           link-get-next
           dup 0<> if space then
        then
    repeat
    ." )"
    2drop
;

\ Deallocate a list of structures. 
: structinfo-list-deallocate-struct-list ( lst0 -- )
    \ Check args.
    assert-tos-is-list

    structinfo-list-store                   \ lst0 snf-lst

    over struct-get-use-count               \ lst0 snf-lst uc
    dup 0 < abort" Invalid use count"

    #2 <                                    \ lst0 snf-lst
    if
        over list-get-links                 \ lst0 snf-lst lst-link
        begin
            ?dup
        while
            dup link-get-data               \ lst0 snf-lst lst-link struct
    
            dup get-first-word              \ lst0 snf-lst lst-link struct, w t | f
            if
                #3 pick                     \ lst0 snf-lst lst-link struct w snf-lst
                structinfo-list-find        \ lst0 snf-lst lst-link struct, snf t | f
                if
                    structinfo-get-deallocate-xt    \ lst0 snf-lst lst-link struct xt
                    execute                 \ lst0 snf-lst lst-link 
                else
                    drop
                then
            else
                drop
            then
    
            link-get-next
        repeat
                                            \ lst0 snf-lst
        drop
        list-deallocate
    else                                    \ lst0 snf-lst
        drop
        struct-dec-use-count
    then
;

