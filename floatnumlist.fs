\ Functions for a list of floatnums.

\ Check TOS for floatnum-list.
: is-floatnum-list? ( tos -- bool )
    dup is-list?            \ tos bool
    ifnot
        drop
        false
        exit
    then

    dup list-is-empty?      \ tos bool
    if
        drop
        true
        exit
    then

    list-get-links          \ link
    link-get-data           \ data
    is-floatnum?            \ bool
;

\ Deallocate a float list.
: floatnum-list-deallocate ( fnum-lst0 -- )
    \ Check arg.
    assert( tos is-floatnum-list? )

    dup struct-get-use-count                    \ floatnum-lst uc
    #2 < if
        [ ' floatnum-deallocate ] literal over   \ floatnum-lst xt floatnum-lst
        list-apply                              \ Deallocate float instances in the list.
    then
    list-deallocate                             \ Deallocate list and links.
;

\ Print a floatnum-list
: .floatnum-list ( fnum-lst0 -- )
    \ Check arg.
    assert( tos is-floatnum-list? )

    [ ' .floatnum ] literal swap .list
;

\ Push a floatnum to a floatnum-list.
: floatnum-list-push ( fnum fnum-lst0 -- )
    \ Check args.
    assert( tos is-floatnum-list? )
    assert( nos is-floatnum? )

    list-push-struct
;

\ Push a floatnum to the end floatnum-list.
: floatnum-list-push-end ( fnum fnum-lst0 -- )
    \ Check args.
    assert( tos is-floatnum-list? )
    assert( nos is-floatnum? )

    list-push-end-struct
;

\ Do an opperation on a list, returning a result list.
\ The xt signature is expected to be ( fnum fnum -- fnum )
: floatnum-list-do-op ( xt fnum-1 fnum-lst0 -- fnum-lst )
    \ Check args.
    assert( tos is-floatnum-list? )
    assert( nos is-floatnum? )

    \ Init return list.
    list-new swap               \ xt fnum-1 ret-lst fnum-lst

    \ Prep for loop.
    list-get-links              \ xt fnum-1 ret-lst fnum-lnk

    begin
        ?dup
    while
        dup link-get-data       \ xt fnum-1 ret-lst fnum-lnk fnumx
        #3 pick                 \ xt fnum-1 ret-lst fnum-lnk fnumx fnum-1
        #5 pick                 \ xt fnum-1 ret-lst fnum-lnk fnumx fnum-1 xt
        execute                 \ xt fnum-1 ret-lst fnum-lnk fnum-rslt
        #2 pick                 \ xt fnum-1 ret-lst fnum-lnk fnum-rslt ren-lst
        floatnum-list-push-end  \ xt fnum-1 ret-lst fnum-lnk

        link-get-next
    repeat
                                \ xt fnum-1 ret-lst
    nip nip
;

