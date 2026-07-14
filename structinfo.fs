\ The structinfo struct, storing information about structs.
\ This is an add-on to the memory managment code, it is not required for memory management
\ unless you want the functions it provides.
#53731 constant structinfo-struct-id
   #10 constant structinfo-struct-number-cells

\ Struct info struct fields.
0                                       constant structinfo-header-disp         \ 16 bits, [0] id, [1] use count.
structinfo-header-disp          cell+   constant structinfo-inst-id-disp        \ Struct ID this structinfo instance describes.
structinfo-inst-id-disp         cell+   constant structinfo-mma-disp            \ Struct mma address.
structinfo-mma-disp             cell+   constant structinfo-print-xt-disp       \ ' noop, or an xt to print a struct. ( instance -- )
structinfo-print-xt-disp        cell+   constant structinfo-deallocate-xt-disp  \ ' noop, or an xt to deallocate a struct. ( instance -- )
structinfo-deallocate-xt-disp   cell+   constant structinfo-from-string-xt-disp \ ' noop, or an xt to return a struct. ( c-addr u -- instance t | f )
structinfo-from-string-xt-disp  cell+   constant structinfo-name-disp           \ Up to 4 cells for name string.

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

\ Check if tos is an allocated structinfo.
: is-structinfo? ( tos -- flag )
    get-first-word          \ w t | f
    if
        structinfo-struct-id =
    else
        false
    then
;

\ Start accessors.

\ Get structinfo instance id name cell.
: structinfo-get-inst-id ( snf0 -- id )
    \ Check arg.
    assert( tos is-structinfo? )

    structinfo-inst-id-disp + @
;

\ Set structinfo instance id cell.
: _structinfo-set-inst-id ( id1 snf0 -- )
    structinfo-inst-id-disp + !
;

\ Get structinfo mma cell.
: structinfo-get-mma ( snf0 -- mma )
    \ Check arg.
    assert( tos is-structinfo? )

    structinfo-mma-disp + @
;

' structinfo-get-mma to structinfo-get-mma-xt

\ Set structinfo mma cell.
: _structinfo-set-mma ( mma1 snf0 -- )
    structinfo-mma-disp + !
;

\ Get structinfo name cell.
: structinfo-get-name ( snf0 -- c-addr u )
    \ Check arg.
    assert( tos is-structinfo? )

    structinfo-name-disp + string@
;

' structinfo-get-name to structinfo-get-name-xt

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
    assert( tos is-structinfo? )

    structinfo-print-xt-disp + @
;

\ Set structinfo deallocate-xt cell, can be ' noop.
: _structinfo-set-deallocate-xt ( xt1 snf0 -- )
    structinfo-deallocate-xt-disp + !
;

\ Get structinfo deallocate-xt cell, may be ' noop.
: structinfo-get-deallocate-xt ( snf0 -- xt )
    \ Check arg.
    assert( tos is-structinfo? )

    structinfo-deallocate-xt-disp + @
;

\ Set structinfo from-string-xt cell, can be ' noop.
: _structinfo-set-from-string-xt ( xt1 snf0 -- )
    structinfo-from-string-xt-disp + !
;

\ Get structinfo from-string-xt cell, may be ' noop.
: structinfo-get-from-string-xt ( snf0 -- xt )
    \ Check arg.
    assert( tos is-structinfo? )

    structinfo-from-string-xt-disp + @
;

\ End accessors.

\ Return a new structinfo struct instance address, with given data value.
: structinfo-new ( from-string-xt deallocate-xt print-xt c-addr u mma1 id0 -- snf )

    structinfo-struct-id structinfo-mma
    struct-allocate                     \ fs-xt d-xt p-xt c-addr u mma1 id0 snf

    \ Set struct instance id.
    tuck _structinfo-set-inst-id        \ fs-xt d-xt p-xt c-addr u mma1 snf

    \ Set struct mma.
    tuck _structinfo-set-mma            \ fs-xt d-xt p-xt c-addr u snf

    \ Set struct name.
    -rot                                \ fs-xt d-xt p-xt snf c-addr u
    #2 pick                             \ fs-xt d-xt p-xt snf c-addr u snf
    _structinfo-set-name                \ fs-xt d-xt p-xt snf

    \ Set print xt.
    tuck _structinfo-set-print-xt       \ fs-xt d-xt snf

    \ Set deallocate xt.
    tuck _structinfo-set-deallocate-xt  \ fs-xt snf

    \ Set from-string xt.
    tuck _structinfo-set-from-string-xt \ snf
;

\ Print a structinfo struct instance.
: .structinfo ( snf0 -- )
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
    assert( tos is-structinfo? )

    dup struct-get-use-count    \ structinfo-addr count
    dup 0< abort" structinfo-deallocate: Invalid use count"

    #2 <
    if
        structinfo-mma mma-deallocate  \ Deallocate instance.
    else
        struct-dec-use-count
    then
;

\ Return true if a structinfo-inst-id matches a number.
: structinfo-inst-id-eq ( id1 snf0 -- flag )
    \ Check args.
    assert( tos is-structinfo? )

    structinfo-get-inst-id
    =
;

