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
    dup 0 <
    abort" use count cannot be negative."

    1-
    swap struct-set-use-count
;

\ Increment struct use count.
: struct-inc-use-count ( struct-addr -- )
    dup struct-get-use-count      \ struct-addr use-count
    1+
    swap struct-set-use-count
;

