\ The name struct, storing a name of up to 15 characters.
#61979 constant name-id
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

\ Start accessors.

\ Get name data cell.
: name-get-string ( name-addr -- string-addr length )
    name-string-disp + string@
;

\ Set name data cell.
: name-set-string ( string-addr length name-addr -- )
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

\ Check instance type.
: is-allocated-name ( name-addr -- flag )
    get-first-word          \ w t | f
    if
        name-id =
    else
        false
    then
;

\ Check TOS for name. Unconventional, no change in stack.
: assert-tos-is-name ( arg0 --  arg0 )
    dup is-allocated-name 0=
    abort" tos is not an allocated name."
;

\ Check list mma usage.
: assert-name-mma-none-in-use ( -- )
    name-mma mma-in-use 0<>
    abort" name-mma use GT 0"
;

\ Return a new name struct instance address, with given data value.
: name-new ( string-addr length -- name-addr )
    name-mma mma-allocate       \ str-addr len name-addr
    name-id over                \ str-addr len name-addr id name-addr
    struct-set-id               \ str-addr len name-addr
    0 over                      \ str-addr len name-addr 1 addr
    struct-set-use-count        \ str-addr len name-addr

    -rot                        \ name-addr str-addr len
    #2 pick                     \ name-addr str-addr len name-addr
    name-set-string             \ name-addr
;

\ Print a name struct instance.
: .name ( name-addr -- )        \ redefines an obsolete function, so a warning displays.
    name-get-string type
;

\ Return true if two names are equal.
: name-eq ( name-addr1 name-addr2 -- flag )
    name-get-string         \ name-addr1 string-addr2 string-length2
    rot                     \ string-addr2 string-length2 name-addr1
    name-get-string         \ string-addr2 string-length2 string-addr1 string-length1
    compare                 \ result
    0=                      \ return true if the result is 0.
;

\ Deallocate a name.
: name-deallocate ( name-addr -- )
    \ Check argument.
    assert-tos-is-name

    dup struct-get-use-count    \ name-addr count

    dup 1 <
    if 
        ." invalid use count" abort
    else
        #2 <
        if
            0 over name-string-disp + !  \ Clear string field first cell.
            name-mma mma-deallocate \ Deallocate instance.
        else
            struct-dec-use-count
        then
    then
;
