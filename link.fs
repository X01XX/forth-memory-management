17137 constant link-id
    3 constant link-struct-number-cells

\ Link struct fields.
0 constant  link-header         \ 16-bits [0] struct id [1] use count.
link-header cell+ constant link-next
link-next   cell+ constant link-data

0 value link-mma    \ Storage for the link mma instance addr.

\ Init link mma.
: link-mma-init ( num-items -- ) \ sets link-mma.
    dup 1 <
    abort" link-mma-init: Invalid number items."

    cr ." Initializing Link store."  
    link-struct-number-cells swap mma-new to link-mma
;

\ Check link mma usage.
: assert-link-mma-none-in-use ( -- )
    link-mma mma-in-use 0<>
    abort" link-mma use GT 0"
;

\ Start accessors.

\ Get link data cell.
: link-get-data ( link-addr -- link-data-value )
    link-data + @
;

\ Set link data cell, use only in this file.
: _link-set-data ( data-value link-addr -- )
    link-data + !
;

\ Get link next cell.
: link-get-next ( link-addr -- link-next-value )
    link-next + @
;

\ Set link next cell, use only in this file, and list.fs.
: _link-set-next ( next-value link-addr -- )
    link-next + !
;
\ End accessors.

\ Return true if TOS is an allocated link.
: is-allocated-link ( link -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup link-mma mma-within-array 0=
    if
        drop false exit
    then
    
    struct-get-id \ Here the fetch could abort on an invalid address, like a random number.
    link-id =
;

\ Return true if TOS is not an allocated link.
: is-not-allocated-link ( link -- flag )
    is-allocated-link 0=
;

\ Check TOS for link, unconventional, leaves stack unchanged. 
: assert-tos-is-link ( arg0 -- arg0 )
    dup is-allocated-link 0=
    abort" TOS is not an allocated link."
;

\ Return a new link struct instance address, with given data value, zero next-value.
: link-new ( data-val -- link-addr )
    link-mma mma-allocate       \ data-val link-addr
    link-id over struct-set-id  \ data-val link-addr
    tuck                        \ link-addr data-val link-addr
    _link-set-data              \ link-addr
    0 over _link-set-next       \ link-addr
    0 over struct-set-use-count \ link-addr
;

\ Print a link in hex.
: .link ( link-addr -- )
    \ Check arg.
    assert-tos-is-link

    base @ swap         \ Save the current base.
    hex
    ." Link: "
    dup .
    ." next: "
    dup link-get-next .
    ." data: "
    link-get-data .
    base !              \ Restore saved base.
;

\ Deallocate a link.
: link-deallocate ( link-addr -- )
    \ Check arg.
    assert-tos-is-link

    dup struct-get-use-count    \ link-addr count

    dup 0 < 
    abort" invalid use count"
    1 = 
    if  
        \ Clear fields.
        0 over _link-set-next
        0 over _link-set-data
        link-mma mma-deallocate \ Deallocate link.
    else
        struct-dec-use-count
    then
;
