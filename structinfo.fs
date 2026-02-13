\ The structinfo struct, storing a structinfo of up to 15 characters.
#53731 constant structinfo-id
    #7 constant structinfo-struct-number-cells

\ Struct info struct fields.
0                                   constant structinfo-header-disp     \ 16 bits, [0] id, [1] use count.
structinfo-header-disp      cell+   constant structinfo-inst-id-disp    \ Struct ID this structinfo instance describes.
structinfo-inst-id-disp     cell+   constant structinfo-mma-disp        \ Struct mma address.
structinfo-mma-disp         cell+   constant structinfo-name-disp       \ Up to 4 cells for name string.

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

\ Check list mma usage.
: assert-structinfo-mma-none-in-use ( -- )
    structinfo-mma mma-in-use 0<>
    abort" structinfo-mma use GT 0"
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

\ End accessors.

\ Return a new structinfo struct instance address, with given data value.
: structinfo-new ( c-addr u mma1 id0 -- snf )
    depth #4 < abort" structinfo-new: too few items on stack"

    structinfo-mma mma-allocate     \ c-addr u snf

    \ Set struct id.
    structinfo-id over              \ c-addr u mma1 id0 snf id0 snf
    struct-set-id                   \ c-addr u mma1 id0 snf

    \ Init use count.
    0 over                          \ c-addr u mma1 id0 snf 0 snf
    struct-set-use-count            \ c-addr u mma1 id0 snf

    \ Set struct instance id.
    tuck _structinfo-set-inst-id    \ c-addr u mma1 snf 

    \ Set struct mma.
    tuck _structinfo-set-mma        \ c-addr u snf 

    -rot                            \ snf c-addr u
    #2 pick                         \ snf c-addr u snf
    _structinfo-set-name            \ snf
;

\ Print a structinfo struct instance.
: .structinfo ( structinfo-addr -- )
    ." Struct: " dup structinfo-get-name type
    space ." mma: " dup structinfo-get-mma hex.
    space ." inst id: " structinfo-get-inst-id dec.
;

\ Return true if two structinfos have equal inst id.
: structinfo-eq ( snf1 snf0 -- flag )
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

