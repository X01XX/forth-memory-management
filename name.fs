\ The name struct, storing a name of up to 15 characters.
#61979 constant name-struct-id
    #3 constant name-struct-number-cells

\ Name struct fields.
0                       constant name-header-disp   \ 16 bits, [0] id, [1] use count.
name-header-disp cell+  constant name-string-disp

0 value name-mma    \ Storage for the name mma instance addr.

\ Init name mma.
: name-mma-init ( num-items -- ) \ sets name-mma.
    dup 1 <
    if
        ." name-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing Name store."
    name-struct-number-cells swap mma-new to name-mma
;

\ Check name mma usage.
: assert-name-mma-none-in-use ( -- )
    name-mma mma-in-use 0<>
    abort" link-mma use GT 0"
;

\ Check if tos is an allocated name.
: is-name? ( tos -- bool )
    dup name-mma mma-is-item?   \ tos bool
    if  
        struct-get-id           \ id
        name-struct-id =        \ bool
    else
        drop
        false                   \ f
    then
;

\ Start accessors.

\ Get name data cell.
: name-get-string ( name-addr -- string-addr length )
    \ Check argument.
    assert( tos is-name? )

    name-string-disp + string@
;

\ Set name data cell.
: name-set-string ( string-addr length name-addr -- )
    \ Check argument.
    assert( tos is-name? )

    over #15 >
    if
        ." name-set-string: string length is too large"
        abort
    then
    over 1 <
    if
        ." name-set-string: string length is too small/invalid"
        abort
    then
    name-string-disp + string!
;

\ End accessors.

\ Return a new name struct instance address, with given data value.
: name-new ( string-addr length -- name-addr )
    name-struct-id name-mma
    struct-allocate             \ str-addr len name-addr

    \ Store string.
    -rot                        \ name-addr str-addr len
    #2 pick                     \ name-addr str-addr len name-addr
    name-set-string             \ name-addr
;

\ Print a name struct instance.
: .name ( name-addr -- )        \ redefines an obsolete function, so a warning displays.
    \ Check argument.
    assert( tos is-name? )

    name-get-string type
;

\ Return true if two names are equal.
: name-eq ( name-addr1 name-addr2 -- flag )
    \ Check argument.
    assert( tos is-name? )
    assert( nos is-name? )

    name-get-string         \ name-addr1 string-addr2 string-length2
    rot                     \ string-addr2 string-length2 name-addr1
    name-get-string         \ string-addr2 string-length2 string-addr1 string-length1
    compare                 \ result
    0=                      \ return true if the result is 0.
;

\ Deallocate a name.
: name-deallocate ( name-addr -- )
    \ Check argument.
    assert( tos is-name? )

    dup struct-get-use-count    \ name-addr count

    dup 0<
    abort" invalid use count"

    #2 <
    if
        0 over name-string-disp + !  \ Clear string field first cell.
        name-mma mma-deallocate \ Deallocate instance.
    else
        struct-dec-use-count
    then
;
