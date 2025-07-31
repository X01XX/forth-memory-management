\ Memmory Management array
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
   

\ Return the addr of the array end.
: mma-array-end-addr ( mma-addr -- end-addr )
    dup _mma-get-stack      \ mma-addr stack-addr
    stack-get-num-free      \ mma-addr num-items-
    over _mma-get-item-size \ mma-addr num-items- item-size
    *                       \ mma-addr item-offset
    swap _mma-get-array     \ item-offset array-start
    +
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

\ ( mma-deallocate: Run like: <item-addr> <mma-addr> mma-deallocate )
: mma-deallocate ( item-addr mma-addr -- )
    depth 2 <
    if  
        ." mma-deallocate: data stack has too few items"
        abort
    then

    _mma-get-stack  \ item-addr stack-addr
    over swap       \ item-addr item-addr stack-addr
    stack-push      \ item-addr
    0 swap !        \ ( zero out first cell of deallocated item )
;

\ ( mma-allocate: Run like: <mma-addr> mma-allocate -- array-item-addr )
: mma-allocate ( mma-addr -- item-addr )
    _mma-get-stack  \ stack-addr
    stack-pop       \ item-addr
;

\ ( mma-new.  Run like: <num-cells-per-item> <num-items> mma-new value <mma-name>.
: mma-new ( num-cells-per-item  num-items -- mma-addr )

    swap        \ n-i n-c-p-i
    over        \ n-i n-c-p-i n-i

    swap        \ n-i n-i n-c-p-i
    cells       \ n-i n-i item-size
    swap        \ n-i item-size n-i
    over        \ n-i item-size n-i item-size
    *           \ n-i item-size all-items-size
    dup         \ n-i item-size total-size total-size
    3 cells +   \ n-i item-size total-size ( add three cells for the stack address, item size, end of array )

    cfalign

    \ Allocate memory for mma-array instance.
    allocate    \ n-i item-size total-size array-addr flag
    0<> 
    if  
        ." mma-new: memory allocation error"
        abort
    then
    \ n-i item-size total-size mma-addr

    \ Store end of array.
    swap over           \ n-i item-size mma-addr total-size mma-addr
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
: mma-free ( addr -- )
    dup   _mma-get-stack     \ mma-addr stack-addr
    free                    \ mma-addr
    0<> if ." mma-array stack free failed" then
    free
    0<> if ." mma-array free failed" then   
;

\ Return the number af array items in use.
: mma-in-use ( mma-addr -- u )
    _mma-get-stack           \ stack-addr
    dup stack-get-capacity   \ stack-addr capacity
    swap stack-get-num-free  \ capacity free
    -
;

\ .mma-usage. Run like: "<mma-name> .mma-usage"
: .mma-usage ( mma-addr -- )
    dup             \ mma-addr mma-addr
    _mma-get-stack  \ mma-addr stack-addr
    ." Capacity:"
    space
    dup             \ mma-addr stack-addr stack-addr
    stack-get-capacity  \ mma-addr stack-addr capacity
    dup             \ mma-addr stack-addr capacity capacity
    3 .r            \ mma-addr stack-addr capacity (emit capacity)
    44 emit         \ mma-addr stack-addr capacity (emit comma)
    space
    ." Free:"
    space
    dup rot         \ mma-addr capacity capacity stack-addr 
    stack-get-num-free  \ mma-addr capacity capacity num-free
    dup             \ mma-addr capacity capacity num-free num-free
    3 .r            \ mma-addr capacity capacity num-free
    44 emit         \ mma-addr capacity capacity num-free
    space
   ." In use:"
   space
   - 3 .r           \ mma-addr capacity
   swap             \ capacity mma-addr
   space
   ." Item Size:" space
   _mma-get-item-size    \ capacity item-size
   dup              \ capacity item-size item-size
   3 .r space       \ capacity item-size
   over *           \ capacity array-size
   tuck             \ array-size capacity array-size
   ." Array size:" space
   3 .r space       \ array-size capacity
   3 +              \ array-size capacity-cells-in-stack-plust-3-cells-overhead 
   cells            \ array-size overhead-size
   dup              \ array-size overhead-size overhead-size
   ." Overhead:" space
   3 .r    space    \ array-size overhead-size
   ." Total:" space
   + dup            \ total-size total-size
   4 .r     space   \ total-size
   cell /           \ number cells
   ." Cells:" space
   3 .r
;

