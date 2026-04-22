\ The floatnum struct, storing a float number.
#61717 constant floatnum-id
    #2 constant floatnum-struct-number-cells

\ Float struct fields.
0                            constant floatnum-header-disp   \ 16 bits, [0] id, [1] use count.
floatnum-header-disp cell+   constant floatnum-number-disp

0 value floatnum-mma \ Storage for the float mma instance addr.

\ Init float mma.
: floatnum-mma-init ( num-items -- ) \ sets floatnum-mma.
    dup 1 <
    if
        ." floatnum-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing FloatNum store."
    floatnum-struct-number-cells swap mma-new to floatnum-mma
;

\ Check instance type.
: is-allocated-floatnum ( addr -- flag )
    dup floatnum-mma mma-is-item  \ addr bool
    if
        get-first-word          \ w t | f
        if
            floatnum-id =         \ bool
        else
            false               \ f
        then
    else
        drop
        false                   \ f
    then
;

\ Check TOS for floatnum, unconventional, leaves stack unchanged.
: assert-tos-is-floatnum ( tos -- tos )
    dup is-allocated-floatnum
    if exit then

    s" TOS is not an allocated floatnum"
    abort
;

\ Check NOS for floatnum, unconventional, leaves stack unchanged.
: assert-nos-is-floatnum ( nos tos -- nos tos )
    over is-allocated-floatnum
    if exit then

    s" NOS is not an allocated floatnum"
    abort
;

\ Start accessors.

\ Get float data cell.
: floatnum-get-number ( fnum -- number-addr length )
    floatnum-number-disp + f@
;

\ Set float data cell.
: floatnum-set-number ( F: r fnum -- )
    floatnum-number-disp + f!
;

\ Check instance type.
: is-allocated-float ( fnum -- flag )
    get-first-word          \ w t | f
    if
        floatnum-id =
    else
        false
    then
;

\ Check TOS for float. Unconventional, no change in stack.
: assert-tos-is-float ( arg0 --  arg0 )
    dup is-allocated-float 0=
    abort" tos is not an allocated float."
;

\ Check list mma usage.
: assert-floatnum-mma-none-in-use ( -- )
    floatnum-mma mma-in-use 0<>
    abort" floatnum-mma use GT 0"
;

\ Return a new float struct instance address, with given data value.
: floatnum-new ( F: r -- fnum )
    floatnum-mma mma-allocate   \ F: r fnum
    floatnum-id over            \ F: r fnum id fnum
    struct-set-id               \ F: r len fnum
    0 over                      \ F: r len fnum 0 fnum
    struct-set-use-count        \ F: r len fnum
    dup                         \ fnum fnum
    floatnum-set-number         \ fnum
;

\ Print a float struct instance.
: .floatnum ( fnum -- )
    floatnum-get-number f.
;

\ Return true if two floats are equal.
: floatnum-eq ( fnum-1 fnum-0 -- flag )
    floatnum-get-number     \ fnum-1 F: r0
    floatnum-get-number     \ F: r0 r1
    f=
;

\ Deallocate a float.
: floatnum-deallocate ( fnum -- )
    \ Check argument.
    assert-tos-is-floatnum

    dup struct-get-use-count    \ fnum count

    dup 0< abort" invalid use count"

    #2 <
    if
        floatnum-mma mma-deallocate \ Deallocate instance.
    else
        struct-dec-use-count
    then
;

\ Return the addition of two floatnum instances.
: floatnum-add ( fnum-1 fnum-0 -- fnum )
    \ Check arguments.
    assert-tos-is-floatnum
    assert-nos-is-floatnum

    floatnum-get-number      \ F: r0 fnum-1
    floatnum-get-number      \ F: r0 r1

    f+
    floatnum-new
;

