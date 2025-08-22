\ Memory Management array
\
\ Create and manage an array, where:
\   Allocation involves popping an array element address from a stack,
\   Deallocation involves pushing an array element address onto the stack.
\
\   Memory is arranged as:
\
\     Array
\       one cell for stack address
\       one cell for item size in bytes
\       one cell for end-of-array address.
\       items
\
\     Stack
\       one cell for info, capacity and current number free
\       one cell for each array item
\
\
\ Trouble-shooting leftover struct instances.
\
\ Comment out freeing heap memory, if needed.
\
\ Assuming mma stores are created like:
\   0 value <struct-name>-mma
\   <num-cells-per-item> <num-items> mma-new to <struct name>-mma
\
\ After running a program, display the usage of all mm-arrays, like: <struct name>-mma .mma-usage
\ noticing some instances are still in use.
\
\ Run <struct name>-mma .mma-in-use
\
\ Check in-use addresses with:  addr  is-allocated-<struct name> ( see is-allocated-region in region.fs )
\ If that returns true, try:    addr  .<struct name>             ( see .region in region.fs )
\ and                           addr  struct-get-use-count
\
\ In the <struct name>-new function ( see region-new in region.fs ), after <struct-name>-mma mma-allocate
\ add a line like: cr dup ." <struct name>-new" dup . cr
\
\ Add statements like: cr ." at ...." cr
\ in other areas of the code, to narrow down where/when the dangling instance is created.
include stack.fs 

' @ alias _mma-get-stack ( mma-addr -- stack-addr )
' ! alias _mma-set-stack ( stack-addr mma-addr -- )

: _mma-set-item-size ( item-size mma-addr -- )
    cell+ !
;

: _mma-get-item-size ( mma-addr -- item-size )
    cell+ @
;

: _mma-set-end-addr ( end-addr mma-addr -- )
    2 cells + !
;

: _mma-get-end-addr ( mma-addr -- end-addr )
    2 cells + @
;

: _mma-get-array ( mma-addr -- array-addr )
    3 cells +
;

\ Return the addr of the first array item.
: mma-array-start-addr ( mma-addr -- start-addr )
    _mma-get-array
;

\ Return true if an address is within an array, its a valid address and could be an instance.
\ So the caller will avoid fetching from a random number, causing an invalid address abort.
: mma-within-array ( addr mma-addr -- flag )
    dup mma-array-start-addr    \ addr mma-addr start-addr
    2 pick                      \ addr mma-addr start-addr addr
    swap                        \ addr mma-addr addr start-addr
    <                           \ addr mma-addr 
    if
        2drop false exit
    then

    _mma-get-end-addr            \ addr end-adder 
    >= if
        false
    else
        true
    then
;

\ Run like: <item-addr> <struct name>-mma mma-deallocate
: mma-deallocate ( item-addr mma-addr -- )
    depth 2 <
    abort" mma-deallocate: data stack has too few items"

    _mma-get-stack  \ item-addr stack-addr
    over swap       \ item-addr item-addr stack-addr
    stack-push      \ item-addr
    0 swap !        \ ( zero out first cell of deallocated item )
;

\ Run like: <struct name>-mma mma-allocate
: mma-allocate ( mma-addr -- item-addr )
    _mma-get-stack  \ stack-addr
    stack-pop       \ item-addr
;

\ Create something like: 0 value <struct name>-mma
\ Run like: <num-cells-per-item> <num-items> mma-new to <struct name>-mma
: mma-new ( num-cells-per-item  num-items -- mma-addr )

    tuck        \ n-i n-c-p-i n-i

    swap        \ n-i n-i n-c-p-i
    cells       \ n-i n-i item-size
    tuck        \ n-i item-size n-i item-size
    *           \ n-i item-size all-items-size
    dup         \ n-i item-size total-size total-size
    3 cells +   \ n-i item-size total-size ( add three cells for the stack address, item size, end of array )

    cfalign

    \ Allocate memory for mma-array instance.
    allocate    \ n-i item-size total-size array-addr flag
    0<> 
    abort" mma-new: memory allocation error"

    \ n-i item-size total-size mma-addr

    \ Store end of array.
    tuck                \ n-i item-size mma-addr total-size mma-addr
    _mma-get-array      \ n-i item-size mma-addr total-size array-addr
    +                   \ n-i item-size mma-addr end-addr
    over                \ n-i item-size mma-addr end-addr mma-addr
    _mma-set-end-addr   \ n-i item-size mma-addr

    \ Create stack, store address.
    rot                 \ item-size mma-addr n-i
    2dup                \ item-size mma-addr n-i mma-addr n-i
    stack-new           \ item-size mma-addr n-i mma-addr stack-addr ( stack allocated )
    swap                \ item-size mma-addr n-i stack-addr mma-addr 
    _mma-set-stack      \ item-size mma-addr n-i ( stack-addr stored in first array cell )

    \ Store item size
    rot                 \ mma-addr n-i item-size
    dup                 \ mma-addr n-i item-size item-size
    3 pick              \ mma-addr n-i item-size item-size mma-addr
    _mma-set-item-size   \ mma-addr n-i item-size

    \ Get array addr.
    2 pick              \ mma-addr n-i item-size mma-addr
    _mma-get-array      \ mma-addr n-i item-size array-addr

    \ Set up end/start do loop parameters
    rot 0               \ mma-addr item-size array-addr n-i 0

    \ Initialize each item in array, add to item addr to stack.
    do                  \ mma-addr item-size item-addr
        \ Init item, add to stack.
        dup             \ mma-addr item-size item-addr item-addr
        3 pick          \ mma-addr item-size item-addr item-addr mma-addr
        mma-deallocate  \ mma-addr item-size item-addr

        \ Point to the next item in array.
        over        \ mma-addr item-size item-addr item-size
        +           \ mma-addr item-size next-item-addr
    loop
    \ Return mm_array instance addr.
    2drop           \ array-addr
;

\ Free heap memory when done.
\ Run like: <struct name>-mma mma-free
: mma-free ( addr -- )
    dup   _mma-get-stack        \ mma-addr stack-addr
    free                        \ mma-addr
    0<> if ." mma-array stack free failed" then
    free
    0<> if ." mma-array free failed" then   
;

\ Return the number af array items in use.
\ Run like: <struct name>-mma mma-in-use
: mma-in-use ( mma-addr -- u )
    _mma-get-stack           \ stack-addr
    dup stack-get-capacity   \ stack-addr capacity
    swap stack-get-num-free  \ capacity free
    -
;

\ Print one-line of  
\ Run like: "<struct name>-mma .mma-usage
: .mma-usage ( mma-addr -- )
    dup             \ mma-addr mma-addr
    _mma-get-stack  \ mma-addr stack-addr
    ." Capacity:"
    space
    dup             \ mma-addr stack-addr stack-addr
    stack-get-capacity  \ mma-addr stack-addr capacity
    dup             \ mma-addr stack-addr capacity capacity
    5 .r            \ mma-addr stack-addr capacity (emit capacity)
    2 spaces
    ." Free:"
    space
    dup rot         \ mma-addr capacity capacity stack-addr 
    stack-get-num-free  \ mma-addr capacity capacity num-free
    dup             \ mma-addr capacity capacity num-free num-free
    5 .r            \ mma-addr capacity capacity num-free
    2 spaces
   ." In use:"
   space
   - 5 .r           \ mma-addr capacity
   swap             \ capacity mma-addr
   2 spaces
   ." Item Size:" space
   _mma-get-item-size    \ capacity item-size
   dup              \ capacity item-size item-size
   3 .r 2 spaces    \ capacity item-size
   over *           \ capacity array-size
   tuck             \ array-size capacity array-size
   ." Array size:" space
   6 .r 2 spaces    \ array-size capacity
   3 +              \ array-size capacity-cells-in-stack-plust-3-cells-overhead 
   cells            \ array-size overhead-size
   dup              \ array-size overhead-size overhead-size
   ." Overhead:" space
   6 .r  2 spaces   \ array-size overhead-size
   ." Total:" space
   + dup            \ total-size total-size
   6 .r  2 spaces   \ total-size
   cell /           \ number cells
   ." Cells:" space
   5 .r
;

\ Print out addresses that are still in use.
\ Run like: <struct name>-mma .mma-in-use
: .mma-in-use ( mma-addr -- )

    \ Save current base, change to hex.
    base @ swap                     \ bs
    hex                             \ bs

    dup _mma-get-item-size swap     \ bs size mma
    dup _mma-get-stack swap         \ bs size stack mma
    dup _mma-get-end-addr swap      \ bs size stack end mma
    mma-array-start-addr            \ bs size stack end next-item

    begin
        2dup <>
    while
        dup                         \ bs size stack end item item
        3 pick                      \ bs size stack end item item stack
        stack-in                    \ bs size stack end item flag
        if
        else
            cr dup ." In use: $" .
        then

        3 pick                      \ bs size stack end item size
        +                           \ bs size stack end next-item
    repeat
    cr
    \ Clear stack
    2drop 2drop                     \ bs
    \ Restore saved base.
    base !                          \
;
