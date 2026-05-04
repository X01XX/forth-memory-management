\ The token struct, storing a token of up to 15 characters.
#59797 constant token-id
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

\ Start accessors.

\ Get token data cell.
: token-get-string ( tkn -- c-addr u )
    token-string-disp + string@
;

\ Set token data cell.
: token-set-string ( c-addr u tkn -- )
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

\ Check instance type.
: is-allocated-token ( tkn -- flag )
    get-first-word          \ w t | f
    if
        token-id =
    else
        false
    then
;

\ Check TOS for token. Unconventional, no change in stack.
: assert-tos-is-token ( arg0 -- arg0 )
    dup is-allocated-token 0=
    abort" tos is not an allocated token."
;

\ Check list mma usage.
: assert-token-mma-none-in-use ( -- )
    token-mma mma-in-use 0<>
    abort" token-mma use GT 0"
;

\ Return a new token struct instance address, with given data value.
: token-new ( c-addr u -- tkn )
    token-id token-mma
    struct-allocate             \ c-addr u tkn

    \ Store string.
    -rot                        \ tkn c-addr u
    #2 pick                     \ tkn c-addr u tkn
    token-set-string            \ tkn
;

\ Print a token struct instance.
: .token ( tkn -- )        \ Redefines an obsolete function, so a warning displays.
    token-get-string type
;

\ Return true if two tokens are equal.
: token-eq ( tkn1 tkn0 -- flag )
    token-get-string        \ tkn1 c-addr0 u0
    rot                     \ c-addr0 u0 tkn1
    token-get-string        \ c-addr0 u0 c-addr1 u1
    str=                    \ flag
;

\ Return true if a token is equal to a string.
: token-eq-string ( c-addr u tkn0 -- flag )
    token-get-string        \ c-addr u c-addr u
    str=                    \ flag
;

\ Deallocate a token.
: token-deallocate ( tkn -- )
    \ Check argument.
    assert-tos-is-token

    dup struct-get-use-count    \ tkn count
    dup 0< abort" invalid use count"

    dup 1 <
    if
        ." invalid use count" abort
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

