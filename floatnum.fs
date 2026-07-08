\ The floatnum struct, storing a float number.
#61717 constant floatnum-struct-id
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
: is-allocated-floatnum? ( addr -- flag )
    dup floatnum-mma mma-is-item? \ addr bool
    if
        get-first-word              \ w t | f
        if
            floatnum-struct-id =    \ bool
        else
            false                   \ f
        then
    else
        drop
        false                       \ f
    then
;

\ Check TOS for floatnum.
: is-floatnum? ( tos -- t )
    dup is-allocated-floatnum?
    if drop true exit then

    s" Selected arg is not an allocated floatnum"
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

\ End accessors.

\ Return a new float struct instance address, with given data value.
: floatnum-new ( F: r -- fnum )
    floatnum-struct-id floatnum-mma
    struct-allocate             \ F: r fnum
    dup                         \ F: r fum fnum
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
    assert( tos is-floatnum? )

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
    assert( tos is-floatnum? )
    assert( nos is-floatnum? )

    floatnum-get-number      \ F: r0 fnum-1
    floatnum-get-number      \ F: r0 r1

    f+
    floatnum-new
;

\ Return a floatnum from a string.
: floatnum-from-string ( c-addr u -- fltn t | f )
    >float              \ t | f
    if
        floatnum-new
        true
    else
        false
    then
;
