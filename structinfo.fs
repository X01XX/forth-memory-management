\ The struct-info struct, storing a struct-info of up to 15 characters.
#53731 constant struct-info-id
    #7 constant struct-info-struct-number-cells

\ Struct info struct fields.
0                                   constant struct-info-header-disp    \ 16 bits, [0] id, [1] use count.
struct-info-header-disp     cell+   constant struct-info-inst-id-disp   \ Struct ID this struct-info instance describes.
struct-info-inst-id-disp    cell+   constant struct-info-mma-disp       \ Struct mma address.
struct-info-mma-disp        cell+   constant struct-info-name-disp      \ Up to 4 cells for name string.

0 value struct-info-mma    \ Storage for the struct-info mma instance addr.

\ Init struct-info mma.
: struct-info-mma-init ( num-items -- ) \ sets struct-info-mma.
    dup 1 <
    if
        ." struct-info-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing Struct info store."
    struct-info-struct-number-cells swap mma-new to struct-info-mma
;

\ Check instance type.
: is-allocated-struct-info ( struct-info-addr -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup struct-info-mma mma-within-array 0=
    if
        drop false exit
    then
    
    struct-get-id   \ Here the fetch could abort on an invalid address, without the previous check.
    struct-info-id =
;

\ Check TOS for struct-info. Unconventional, no change in stack.
: assert-tos-is-struct-info ( arg0 --  arg0 )
    dup is-allocated-struct-info 0=
    abort" tos is not an allocated struct-info."
;

\ Check list mma usage.
: assert-struct-info-mma-none-in-use ( -- )
    struct-info-mma mma-in-use 0<>
    abort" struct-info-mma use GT 0"
;

\ Start accessors.

\ Get struct-info instance id name cell.
: struct-info-get-inst-id ( struct-info -- id )
    struct-info-inst-id-disp + @
;

\ Set struct-info instance id cell.
: _struct-info-set-inst-id ( id struct-info -- )
    struct-info-inst-id-disp + !
;

\ Get struct-info mma cell.
: struct-info-get-mma ( struct-info -- mma )
    struct-info-mma-disp + @
;

\ Set struct-info mma cell.
: _struct-info-set-mma ( mma struct-info -- )
    struct-info-mma-disp + !
;

\ Get struct-info name cell.
: struct-info-get-name ( struct-info -- c-addr u )
    struct-info-name-disp + string@
;

\ Set struct-info name cell.
: _struct-info-set-name ( c-addr u si-addr -- )
    over #31 >
    if
        ." struct-info-set-name: string length is too large"
        abort
    then
    over 1 <
    if
        ." struct-info-set-name: string length is too small/invalid"
        abort
    then
    struct-info-name-disp + string!
;

\ End accessors.

\ Return a new struct-info struct instance address, with given data value.
: struct-info-new ( c-addr u struct-mma struct-id -- si-addr )
    depth 4 < abort" struct-info-new: too few items on stack"

    struct-info-mma mma-allocate    \ str-addr len si-addr

    \ Set struct id.
    struct-info-id over             \ str-addr len struct-mma struct-id si-addr id si-addr
    struct-set-id                   \ str-addr len struct-mma struct-id si-addr

    \ Init use count.
    0 over                          \ str-addr len struct-mma struct-id si-addr 1 addr
    struct-set-use-count            \ str-addr len struct-mma struct-id si-addr

    \ Set struct instance id.
    tuck _struct-info-set-inst-id   \ str-addr len struct-mma si-addr 

    \ Set struct mma.
    tuck _struct-info-set-mma       \ str-addr len si-addr 

    -rot                            \ si-addr str-addr len
    #2 pick                         \ si-addr str-addr len si-addr
    _struct-info-set-name           \ si-addr
;

\ Print a struct-info struct instance.
: .struct-info ( struct-info-addr -- )
    ." Struct: " dup struct-info-get-name type
    space ." mma: " dup struct-info-get-mma hex.
    space ." inst id: " struct-info-get-inst-id dec.
;

\ Return true if two struct-infos have equal inst id.
: struct-info-eq ( si-addr1 si-addr0 -- flag )
    struct-info-get-inst-id         \ si-addr1 id0
    swap                            \ id0 si-addr1
    struct-info-get-inst-id         \ id0 id1
    =                               \ flag
;

\ Deallocate a struct-info.
: struct-info-deallocate ( si-addr0 -- )
    \ Check argument.
    assert-tos-is-struct-info

    dup struct-get-use-count    \ struct-info-addr count

    dup 1 <
    if 
        ." invalid use count" abort
    else
        #2 <
        if
            struct-info-mma mma-deallocate \ Deallocate instance.
        else
            struct-dec-use-count
        then
    then
;

\ Return true if a struct-info-inst-id matches a number.
: struct-info-inst-id-eq ( id1 si0 -- flag )                                                                                      
    \ Check args.
    assert-tos-is-struct-info

    struct-info-get-inst-id
    =
;

