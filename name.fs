\ The name struct, storing a name of up to 15 characters.
17971 constant name-id
   3 constant name-struct-number-cells

\ Name struct fields.
0 constant  name-header         \ [0] id [1] use count.
name-header cell+ constant name-string

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
: name-get-id ( name-addr -- id )
    0w@
;

: name-set-id ( name-addr -- )
    name-id swap 0w!
;

: name-get-use-count ( name-addr -- u-uc )
    1w@
;

: name-set-use-count ( u-16 name-addr -- )
    1w!
;

: name-dec-use-count ( name-addr -- )
    dup name-get-use-count      \ name-addr use-count
    1-
    swap name-set-use-count
;

: name-inc-use-count ( name-addr -- )
    dup name-get-use-count      \ name-addr use-count
    1+
    swap name-set-use-count
;

\ Get name data cell.
: name-get-string ( name-addr -- string-addr length )
    name-string + string@
;

\ Set name data cell.
: name-set-string ( string-addr length name-addr -- )
    over 15 >
    if
        ." name-set-string: string length is too large"
        abort
    then
    over 1 <
    if
        ." name-set-string: string length is too small/invalid"
        abort
    then
    name-string + string!
;

\ Check instance type.
: is-allocated-name ( name-addr -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup name-mma mma-within-array 0=
    if
        drop false exit
    then
    
    name-get-id \ Here the fetch could abort on an invalid address, like a random number.
    name-id =
;

: is-not-allocated-name ( name-addr -- flag )
    is-allocated-name 0=
;

\ Return a new name struct instance address, with given data value.
: name-new ( string-addr length -- name-addr )
    name-mma mma-allocate       \ str-addr len name-addr
    dup name-set-id             \ str-addr len name-addr
    1 over name-set-use-count   \ str-addr len name-addr

    -rot                        \ name-addr str-addr len
    2 pick                      \ name-addr str-addr len name-addr
    name-set-string             \ name-addr
;

\ Print a name struct instance.
: .name ( name-addr -- )
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
    dup is-not-allocated-name
    if
        ." name-deallocate: Arg is not an allocated name"
        abort
    then

    dup name-get-use-count      \ name-addr count
    2 <
    if
        \ Clear fields.
        0 over name-string + !

        name-mma mma-deallocate \ Deallocate name.
    else
        name-dec-use-count
    then
;
