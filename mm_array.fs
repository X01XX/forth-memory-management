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
\       items
\
\     Stack
\       one cell for info, capacity and current number free
\       one cell for each array item
\

include stack.fs 

' @ alias _mm-get-stack ( mma-addr -- stack-addr )
' ! alias _mm-set-stack ( stack-addr mma-addr -- )

: _mm-set-item-size ( item-size mma-addr -- )
    cell+ !
;

: _mm-get-item-size ( mma-addr -- item-size )
    cell+ @
;

: _mm-get-array ( mma-addr -- array-addr )
    2 cells +
;

\ ( mma-deallocate: Run like: <item-addr> <mma-addr> mma-deallocate )
: mma-deallocate ( item-addr mma-addr -- )
    depth 2 <
    if  
        ." mma-deallocate: data stack has too few items"
        abort
    then

    _mm-get-stack   \ item-addr stack-addr
    over swap       \ item-addr item-addr stack-addr
    stack-push      \ item-addr
    0 swap !        \ ( zero out first cell of deallocated item )
;

\ ( mma-allocate: Run like: <mma-addr> mma-allocate -- array-item-addr )
: mma-allocate ( mma-addr -- item-addr )
    _mm-get-stack    \ stack-addr
    stack-pop    \ item-addr
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
    2 cells +   \ n-i item-size total-size ( add two cells for the stack address, item size )

    cfalign

    \ Allocat e memory for mm-array instance.
    allocate    \ n-i item-size array-addr flag
    0<> 
    if  
        ." mma-new: memory allocation error"
        abort
    then
    \ n-i item-size array-addr

    \ Create stack, store address.
    rot             \ item-size array-addr n-i
    2dup            \ item-size array-addr n-i array-addr n-i
    stack-new       \ item-size array-addr n-i array-addr stack-addr ( stack allocated )
    swap            \ item-size array-addr n-i stack-addr array-addr 
    _mm-set-stack   \ item-size array-addr n-i ( stack-addr stored in first array cell )

    \ Store item size
    rot             \ array-addr n-i item-size
    dup             \ array-addr n-i item-size item-size
    3 pick          \ array-addr n-i item-size item-size array-addr
    _mm-set-item-size   \ array-addr n-i item-size

    \ Get array addr.
    2 pick          \ array-addr n-i item-size array-addr
    _mm-get-array   \ array-addr n-i item-size array-addr+

    \ Set up end/start do loop parameters
    rot 0           \ array-addr item-size array-addr+ n-i 0

    \ Initialize each item in array, add to item addr to stack.
    do              \ array-addr item-size array-addr+
        \ Init item, add to stack.
        dup         \ array-addr item-size array-addr+ array-addr+
        3 pick      \ array-addr item-size array-addr+ array-addr+ array-addr
        mma-deallocate  \ array-addr item-size array-addr+

        \ Point to the next instance.
        over        \ array-addr item-size array-addr+ item-size
        +           \ array-addr item-size array-addr+
    loop
    \ Return mm_array instance addr.
    2drop           \ array-addr
;

\ Free heap memory when done.
: mma-free ( addr - )
    dup   _mm-get-stack    \ mma-addr stack-addr
    free                  \ mma-addr
    0<> if ." mm-array stack free failed" then
    free                  \   
    0<> if ." mm-array free failed" then   
;

\ .mma-usage. Run like: "<mma-name> .mma-usage"
: .mma-usage ( mma-addr -- )
    dup            \ mma-addr mma-addr
    _mm-get-stack    \ mma-addr stack-addr
    ." Capacity:"
    space
    dup            \ mma-addr stack-addr stack-addr
    _stack-get-capacity    \ mma-addr stack-addr capacity
    dup            \ mma-addr stack-addr capacity capacity
    3 .r        \ mma-addr stack-addr capacity (emit capacity)
    44 emit        \ mma-addr stack-addr capacity (emit comma)
    space
    ." Free:"
    space
    dup rot        \ mma-addr capacity capacity stack-addr 
   _stack-get-num-free    \ mma-addr capacity capacity num-free
    dup            \ mma-addr capacity capacity num-free num-free
    3 .r        \ mma-addr capacity capacity num-free
    44 emit        \ mma-addr capacity capacity num-free
    space
   ." In use:"
   space
   - 3 .r        \ mma-addr capacity
   swap            \ capacity mma-addr
   space
   ." Item Size:" space
   _mm-get-item-size    \ capacity item-size
   dup            \ capacity item-size item-size
   3 .r space        \ capacity item-size
   over *        \ capacity array-size
   tuck            \ array-size capacity array-size
   ." Array size:" space
   3 .r space         \ array-size capacity
   3 +            \ array-size capacity-cells-in-stack-plust-3-cells-overhead 
   cells        \ array-size overhead-size
   dup            \ array-size overhead-size overhead-size
   ." Overhead:" space
   3 .r    space        \ array-size overhead-size
   ." Total:" space
   + dup        \ total-size total-size
   4 .r     space        \ total-size
   cell /        \ number cells
   ." Cells:" space
   3 .r            \ -- )
;

