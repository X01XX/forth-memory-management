\ Functions for a list of floats.

\ Check if tos is an empty list, or has a float32 instance as its first item.
: assert-tos-is-float32-list ( tos -- tos )                                                                             
    dup list-is-empty
    if
    else
        dup list-get-links link-get-data
        assert-tos-is-float32
        drop
    then
;   

\ Deallocate a float list.
: float32-list-deallocate ( f32-lst0 -- )
    \ Check arg.
    assert-tos-is-float32-list

    dup struct-get-use-count                    \ float32-lst uc
    2 < if
        [ ' float32-deallocate ] literal over   \ float32-lst xt float32-lst
        list-apply                              \ Deallocate float instances in the list.
    then
    list-deallocate                             \ Deallocate list and links.
;

\ Print a float32-list
: .float32-list ( f32-lst0 -- )
    \ Check arg.
    assert-tos-is-float32-list

    [ ' .float32 ] literal swap .list
;

\ Push a float32 to a float32-list.
: float32-list-push ( f32 f32-lst0 -- )                                                                                   
    \ Check args.
    assert-tos-is-float32-list
    assert-nos-is-float32

    list-push-struct
;

\ Push a float32 to the end float32-list.
: float32-list-push-end ( f32 f32-lst0 -- )                                                                                   
    \ Check args.
    assert-tos-is-float32-list
    assert-nos-is-float32

    list-push-end-struct
;

\ Do an opperation on a list, returning a result list.
\ The xt signature is expected to be ( f32 f32 -- f32 )
: float32-do-op ( xt f32-1 f32-lst0 -- f32-lst )
    \ Check args.
    assert-tos-is-float32-list
    assert-nos-is-float32

    \ Init return list.
    list-new swap               \ xt f32-1 ret-lst f32-lst

    \ Prep for loop.
    list-get-links              \ xt f32-1 ret-lst f32-lnk

    begin
        ?dup
    while
        dup link-get-data       \ xt f32-1 ret-lst f32-lnk f32x
        #3 pick                 \ xt f32-1 ret-lst f32-lnk f32x f32-1
        #5 pick                 \ xt f32-1 ret-lst f32-lnk f32x f32-1 xt
        execute                 \ xt f32-1 ret-lst f32-lnk f32-rslt
        #2 pick                 \ xt f32-1 ret-lst f32-lnk f32-rslt ren-lst
        float32-list-push-end   \ xt f32-1 ret-lst f32-lnk

        link-get-next
    repeat
                            \ xt f32-1 ret-lst
    nip nip
;

