\ The token struct, storing a token of up to 15 characters.
#59797 constant token-struct-id
   #11 constant token-struct-number-cells

\ Token struct fields.
0                       constant token-header-disp   \ 16 bits, [0] id, [1] use count.
token-header-disp cell+ constant token-string-disp

0 value token-mma       \ Storage for the token mma instance addr.

\ Init token mma.
: token-mma-init ( num-items -- ) \ sets token-mma.
    dup 1 <
    if
        ." token-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing Token store."
    token-struct-number-cells swap mma-new to token-mma
;

\ Check instance type.
: is-allocated-token? ( tos -- bool )
    dup token-mma mma-is-item? \ addr bool
    if
        struct-get-id           \ id
        token-struct-id =       \ bool
    else
        drop
        false                   \ f
    then
;

\ Check TOS for token.
: is-token? ( tos -- t )
    dup is-allocated-token?
    if drop true exit then

    s" Selected arg is not an allocated token"
    abort
;


\ Start accessors.

\ Get token data cell.
: token-get-string ( tkn -- c-addr u )
    \ Check arg.
    assert( tos is-token? )

    token-string-disp + string@
;

\ Set token data cell.
: token-set-string ( c-addr u tkn -- )
    \ Check args.
    assert( tos is-token? )

    over #80 >
    if
        ." token-set-string: string length is too large"
        abort
    then
    over 1 <
    if
        ." token-set-string: string length is too small/invalid"
        abort
    then
    token-string-disp + string!
;

\ Return a new token struct instance address, with given data value.
: token-new ( c-addr u -- tkn )
    token-struct-id token-mma
    struct-allocate             \ c-addr u tkn

    \ Store string.
    -rot                        \ tkn c-addr u
    #2 pick                     \ tkn c-addr u tkn
    token-set-string            \ tkn
;

\ Print a token struct instance.
: .token ( tkn -- )
    \ Check arg.
    assert( tos is-token? )

    [char] " emit
    token-get-string type
    [char] " emit
;

\ Return true if two tokens are equal.
: token-eq ( tkn1 tkn2 -- flag )
    \ Check args.
    assert( tos is-token? )
    assert( nos is-token? )

    token-get-string        \ tkn1 c-addr2 u2
    rot                     \ c-addr2 u2 tkn1
    token-get-string        \ c-addr2 u2 c-addr1 u1
    compare                 \ result
    0=                      \ return true if the result is 0.
;

\ Return true if a token is equal to a string.
: token-eq-string ( c-addr u tkn0 -- flag )
    \ Check args.
    assert( tos is-token? )

    token-get-string        \ c-addr u c-addr u
    str=                    \ flag
;

\ Deallocate a token.
: token-deallocate ( tkn0 -- )
    \ Check arg.
    assert( tos is-token? )

    dup struct-get-use-count    \ tkn count
    dup 0< abort" token-deallocate: Invalid use count"

    dup 0<
    if
        ." token-deallocate: Invalid use count" abort
    else
        #2 <
        if
            0 over token-string-disp + !    \ Clear string field first cell.
            token-mma mma-deallocate        \ Deallocate instance.
        else
            struct-dec-use-count
        then
    then
;

