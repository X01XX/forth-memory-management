
\ Fetch a number from the [0] 16 bits of a cell.
: 0w@ ( addr -- u-16 )
    uw@
;

\ Fetch a number from the [1] 16 bits of a cell.
: 1w@ ( addr  -- u-16 )
    2 + uw@
;

\ Fetch a number from the [2] 16 bits of a cell.
: 2w@ ( addr  -- u-16 )
    4 + uw@
;

\ Fetch a number from the [3] 16 bits of a cell.
: 3w@ ( addr  -- u-16 )
    6 + uw@
;

\ Store a number into the [0] 16 bits of a cell.
: 0w! ( u-16 addr -- )
    w!
;

\ Store a number into the [1] 16 bits of a cell.
: 1w! ( u-16 addr -- )
    2 + w!
;

\ Store a number into the [2] 16 bits of a cell.
: 2w! ( u-16 addr -- )
    4 + w!
;

\ Store a number into the [3] 16 bits of a cell.
: 3w! ( u-16 addr -- )
    6 + w!
;

: cs clearstack ; \ Shorthand.

\ Store a string on the stack to a given address.
: string! ( string-addr length target-addr -- )
    2dup c!         \ Store the length at target[0].
    1+              \ Point to target[1].
    swap cmove      \ Move characters to target[1].
;

\ Fetch a string from a given address, put on stack.
: string@ ( string-addr -- string-addr+1 length )
    dup c@          \ addr length
    swap 1+ swap    \ addr+1 length
;


\ Return the struct id from a struct instance.
: struct-get-id ( addr -- u1 )
    0w@               \ Fetch the ID. 
;

\ Set the struct id,
: struct-set-id ( u addr -- )
    0w!    \ Store the ID.
;

\ Get struct use count.
: struct-get-use-count ( struct-addr -- u-uc )
    1w@ 
;

\ Set struct use count.
: struct-set-use-count ( u-16 struct-addr -- )
    1w! 
;

\ Decrement struct use count.
: struct-dec-use-count ( struct-addr -- )
    dup struct-get-use-count      \ struct-addr use-count
    1-  
    swap struct-set-use-count
;

\ Increment struct use count.
: struct-inc-use-count ( struct-addr -- )
    dup struct-get-use-count      \ struct-addr use-count
    1+
    swap struct-set-use-count
;

