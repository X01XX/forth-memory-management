\ The structinfo struct, storing a structinfo of up to 15 characters.
#53731 constant structinfo-id
    #9 constant structinfo-struct-number-cells

\ Struct info struct fields.
0                                       constant structinfo-header-disp         \ 16 bits, [0] id, [1] use count.
structinfo-header-disp          cell+   constant structinfo-inst-id-disp        \ Struct ID this structinfo instance describes.
structinfo-inst-id-disp         cell+   constant structinfo-mma-disp            \ Struct mma address.
structinfo-mma-disp             cell+   constant structinfo-print-xt-disp       \ ' noop, or xt to print a struct.
structinfo-print-xt-disp        cell+   constant structinfo-deallocate-xt-disp  \ ' noop, or xt to deallocate a struct.
structinfo-deallocate-xt-disp   cell+   constant structinfo-name-disp           \ Up to 4 cells for name string.

0 value structinfo-mma     \ Storage for the structinfo mma instance addr.

\ Init structinfo mma.
: structinfo-mma-init ( num-items -- ) \ sets structinfo-mma.
    dup 1 <
    if
        ." structinfo-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing StructInfo store."
    structinfo-struct-number-cells swap mma-new to structinfo-mma
;

\ Check instance type.
: is-allocated-structinfo ( tos -- flag )
    get-first-word          \ w t | f
    if
        structinfo-id =
    else
        false
    then
;

\ Check TOS for structinfo. Unconventional, no change in stack.
: assert-tos-is-structinfo ( tos --  tos )
    dup is-allocated-structinfo 0=
    abort" tos is not an allocated structinfo."
;

\ Check NOS for structinfo. Unconventional, no change in stack.
: assert-nos-is-structinfo ( nos tos --  nos tos )
    over is-allocated-structinfo 0=
    abort" nos is not an allocated structinfo."
;

\ Start accessors.

\ Get structinfo instance id name cell.
: structinfo-get-inst-id ( snf0 -- id )
    \ Check arg.
    assert-tos-is-structinfo

    structinfo-inst-id-disp + @
;

\ Set structinfo instance id cell.
: _structinfo-set-inst-id ( id1 snf0 -- )
    structinfo-inst-id-disp + !
;

\ Get structinfo mma cell.
: structinfo-get-mma ( snf0 -- mma )
    \ Check arg.
    assert-tos-is-structinfo

    structinfo-mma-disp + @
;

\ Set structinfo mma cell.
: _structinfo-set-mma ( mma1 snf0 -- )
    structinfo-mma-disp + !
;

\ Get structinfo name cell.
: structinfo-get-name ( snf0 -- c-addr u )
    \ Check arg.
    assert-tos-is-structinfo

    structinfo-name-disp + string@
;

\ Set structinfo name cell.
: _structinfo-set-name ( c-addr u snf0 -- )
    over #31 >
    if
        ." structinfo-set-name: string length is too large"
        abort
    then
    over 1 <
    if
        ." structinfo-set-name: string length is too small/invalid"
        abort
    then
    structinfo-name-disp + string!
;

\ Set structinfo print-xt cell, can be ' noop.
: _structinfo-set-print-xt ( xt1 snf0 -- )
    structinfo-print-xt-disp + !
;

\ Get structinfo print-xt cell, may be ' noop.
: structinfo-get-print-xt ( snf0 -- xt )
    \ Check arg.
    assert-tos-is-structinfo

    structinfo-print-xt-disp + @
;

\ Set structinfo deallocate-xt cell, can be ' noop.
: _structinfo-set-deallocate-xt ( xt1 snf0 -- )
    structinfo-deallocate-xt-disp + !
;

\ Get structinfo deallocate-xt cell, may be ' noop.
: structinfo-get-deallocate-xt ( snf0 -- xt )
    \ Check arg.
    assert-tos-is-structinfo

    structinfo-deallocate-xt-disp + @
;

\ End accessors.

\ Return a new structinfo struct instance address, with given data value.
: structinfo-new ( deallcate-xt print-xt c-addr u mma1 id0 -- snf )

    structinfo-mma mma-allocate     \ d-xt p-xt c-addr u snf

    \ Set struct id.
    structinfo-id over              \ d-xt p-xt c-addr u mma1 id0 snf id0 snf
    struct-set-id                   \ d-xt p-xt c-addr u mma1 id0 snf

    \ Init use count.
    0 over                          \ d-xt p-xt c-addr u mma1 id0 snf 0 snf
    struct-set-use-count            \ d-xt p-xt c-addr u mma1 id0 snf

    \ Set struct instance id.
    tuck _structinfo-set-inst-id    \ d-xt p-xt c-addr u mma1 snf 

    \ Set struct mma.
    tuck _structinfo-set-mma        \ d-xt p-xt c-addr u snf 

    \ Set struct name.
    -rot                            \ d-xt p-xt snf c-addr u
    #2 pick                         \ d-xt p-xt snf c-addr u snf
    _structinfo-set-name            \ d-xt p-xt snf

    \ Set print xt.
    tuck _structinfo-set-print-xt   \ d-xt snf

    \ Set deallocate xt.
    tuck _structinfo-set-deallocate-xt  \ snf
;

\ Print a structinfo struct instance.
: .structinfo ( structinfo-addr -- )
    ." Struct: " dup structinfo-get-name type
    space ." mma: " dup structinfo-get-mma hex.
    space ." inst id: " structinfo-get-inst-id dec.
;

\ Return true if two structinfos have equal inst id.
: structinfo-id-eq ( snf1 snf0 -- flag )
    structinfo-get-inst-id          \ snf1 id0
    swap                            \ id0 snf1
    structinfo-get-inst-id          \ id0 id1
    =                               \ flag
;

\ Deallocate a structinfo.
: structinfo-deallocate ( snf0 -- )
    \ Check arg.
    assert-tos-is-structinfo

    dup struct-get-use-count    \ structinfo-addr count

    dup 1 <
    if 
        ." invalid use count" abort
    else
        #2 <
        if
            structinfo-mma mma-deallocate  \ Deallocate instance.
        else
            struct-dec-use-count
        then
    then
;

\ Return true if a structinfo-inst-id matches a number.
: structinfo-inst-id-eq ( id1 snf0 -- flag )                                                                                      
    \ Check args.
    assert-tos-is-structinfo

    structinfo-get-inst-id
    =
;

