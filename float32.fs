\ The float32 struct, storing a float32 number.
#61717 constant float32-id
    #3 constant float32-struct-number-cells

\ Float struct fields.
0                           constant float32-header-disp   \ 16 bits, [0] id, [1] use count.
float32-header-disp cell+   constant float32-number-disp

0 value float32-mma \ Storage for the float mma instance addr.

\ Init float mma.
: float32-mma-init ( num-items -- ) \ sets float32-mma.
    dup 1 <
    if
        ." float32-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing Float store."
    float32-struct-number-cells swap mma-new to float32-mma
;

\ Check instance type.
: is-allocated-float32 ( addr -- flag )
    dup float32-mma mma-is-item  \ addr bool
    if  
        get-first-word          \ w t | f
        if  
            float32-id =         \ bool
        else
            false               \ f 
        then
    else
        drop
        false                   \ f 
    then
;

\ Check TOS for float32, unconventional, leaves stack unchanged.
: assert-tos-is-float32 ( tos -- tos )
    dup is-allocated-float32
    if exit then

    s" TOS is not an allocated float32"
    abort
;

\ Check NOS for float32, unconventional, leaves stack unchanged.
: assert-nos-is-float32 ( nos tos -- nos tos )
    over is-allocated-float32
    if exit then

    s" NOS is not an allocated float32"
    abort
;

\ Start accessors.

\ Get float data cell.
: float32-get-number ( f32 -- number-addr length )
    float32-number-disp + f@
;

\ Set float data cell.
: float32-set-number ( F: r f32 -- )
    float32-number-disp + f!
;

\ Check instance type.
: is-allocated-float ( f32 -- flag )
    get-first-word          \ w t | f
    if
        float32-id =
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
: assert-float32-mma-none-in-use ( -- )
    float32-mma mma-in-use 0<>
    abort" float32-mma use GT 0"
;

\ Return a new float struct instance address, with given data value.
: float32-new ( F: r -- f32 )
    float32-mma mma-allocate    \ F: r f32
    float32-id over             \ F: r f32 id f32
    struct-set-id               \ F: r len f32
    0 over                      \ F: r len f32 0 f32
    struct-set-use-count        \ F: r len f32
    dup                         \ f32 f32
    float32-set-number          \ f32
;

\ Print a float struct instance.
: .float32 ( f32 -- )
    float32-get-number f.
;

\ Return true if two floats are equal.
: float32-eq ( f32-1 f32-0 -- flag )
    float32-get-number      \ f32-1 F: r0
    float32-get-number      \ F: r0 r1
    f=
;

\ Deallocate a float.
: float32-deallocate ( f32 -- )
    \ Check argument.
    assert-tos-is-float32

    dup struct-get-use-count    \ f32 count

    dup 0< abort" invalid use count"

    #2 <
    if
        float32-mma mma-deallocate \ Deallocate instance.
    else
        struct-dec-use-count
    then
;

\ Return the addition of two float32 instances.
: float32-add ( f32-1 f32-0 -- f32 )
    \ Check arguments.
    assert-tos-is-float32
    assert-nos-is-float32

    float32-get-number      \ F: r0 f32-1
    float32-get-number      \ F: r0 r1

    f+
    float32-new
;

