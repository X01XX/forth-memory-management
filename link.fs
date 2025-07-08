17971 constant link-id
   3 constant link-struct-number-cells

0 constant  link-header
link-header cell+ constant link-next
link-next   cell+ constant link-data

0 value link-mma    \ Storage for the link mma instance addr.

\ Init link mma.
: link-mma-init ( num-items -- ) \ sets link-mma.
    dup 1 <
    if
        ." link-mma-init: Invalid number items."
        abort
    then

    cr ." Initializing Link store."  
    link-struct-number-cells swap mma-new to link-mma
;

\ Start accessors.
: link-get-id ( link -- header-value )
    link-header + @
;

: link-set-id ( header-value link -- )
    link-header + !
;

\ Get link data cell.
: link-get-data ( link-addr -- link-data-value )
    link-data + @
;

\ Set link data cell.
: link-set-data ( data-value link-addr -- )
    link-data + !
;

\ Get link next cell.
: link-get-next ( link-addr -- link-next-value )
    link-next + @
;

\ Set link next cell.
: link-set-next ( next-value link-addr -- )
    link-next + !
;
\ End accessors.

\ Check instance type.
: is-allocated-link ( link -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup link-mma mma-within-array 0=
    if
        drop false exit
    then
    
    link-get-id \ Here the fetch could abort on an invalid address, like a random number.
    link-id =
;

: is-not-allocated-link ( link -- flag )
    is-allocated-link 0=
;

\ Return a new link, with given data value, zero next-value.
: link-new ( data-val -- link-addr )
    link-mma mma-allocate  \ ( data-val link-store -- data-val link-addr )
    link-id over link-set-id
    swap over              \ ( data-val link-addr -- link-addr data-val link-addr )
    link-set-data          \ ( link-addr data-val link-addr -- link-addr ) ( data-val stored in second, data-pointer , cell )
    0 over link-set-next   \ ( link-addr -- link-addr ) ( 0 stored in first, next-pointer, cell )
;

\ Print a link in hex.
: .link ( link-addr -- )
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
    \ Check argument.
    dup is-not-allocated-link
    if
        ." link-deallocate: Arg is not an allocated link"
        abort
    then

    \ Clear fields.
    0 over link-set-next
    0 over link-set-data

    link-mma mma-deallocate \ Deallocate link.
;
