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
\       one cell for item size in bytes [0]w. Min-free [1]w, or how close did we get to using the whole stack.
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
\
\ A method, tedious but effective.  For example, on empty lists:
\
\ In the struct code, add a source (just a number) field, probably in an unused part of the header.
\ Add <struct>-get-source, and <struct>-set-source, functions.
\ Change <struct>-new to take a number argument and store it in the source field.
\ Change .<struct> to display the source number.
\
\ Every reference to <struct>-new in the code should be changed to: <unique number>  <struct>-new.
\
\ Run the program to the degree needed to cause the memory leak.
\ End the session, deallocatating whatever can be deallocatted.
\
\ Use <struct>-mma .mma-in-use to see the addresses of the still-allocated structs.
\ Use .<struct> on one of the addresses to determine its source.

include stack.fs 

' @ alias _mma-get-stack ( mma-addr -- stack-addr )
' ! alias _mma-set-stack ( stack-addr mma-addr -- )

: _mma-set-item-size ( item-size mma-addr -- )
    cell+ w!
;

: _mma-get-item-size ( mma-addr -- item-size )
    cell+ uw@
;

: _mma-set-min-free ( item-size mma-addr -- )
    cell+ #2 + w!
;

: _mma-get-min-free ( mma-addr -- item-size )
    cell+ #2 + uw@
;

: _mma-set-end-addr ( end-addr mma-addr -- )
    #2 cells + !
;

: _mma-get-end-addr ( mma-addr -- end-addr )
    #2 cells + @
;

: _mma-get-array ( mma-addr -- array-addr )
    #3 cells +
;

\ Return the addr of the first array item.
: mma-array-start-addr ( mma-addr -- start-addr )
    _mma-get-array
;

\ Return true if an address is within an array, its a valid address and could be an instance.
\ So the caller will avoid fetching from a random number, causing an invalid address abort.
: mma-within-array ( addr mma-addr -- flag )
    dup mma-array-start-addr    \ addr mma-addr start-addr
    #2 pick                     \ addr mma-addr start-addr addr
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
    depth #2 <
    abort" mma-deallocate: data stack has too few items"

    _mma-get-stack  \ item-addr stack-addr
    over swap       \ item-addr item-addr stack-addr
    stack-push      \ item-addr
    0 swap !        \ ( zero out first cell of deallocated item )
;

\ Run like: <struct name>-mma mma-allocate
: mma-allocate ( mma-addr -- item-addr )
    \ Get item from stack.
    dup _mma-get-stack  \ a-addr s-addr
    dup stack-pop       \ a-addr s-addr item-addr

    \ Check min free
    swap stack-get-num-on-stack     \ a-addr item-addr num
    dup                             \ a-addr item-addr num num
    #3 pick _mma-get-min-free       \ a-addr i-addr num num min-f
    <                               \ a-addr i-addr num flag
    if                              \ a-addr i-addr num
        \ Update min free
        rot                         \ i-addr num a-addr
        _mma-set-min-free           \ i-addr
    else                            \ a-addr i-addr num
        drop nip                    \ i-addr
    then
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
    #3 cells +  \ n-i item-size total-size ( add three cells for the stack address, item size, end of array )

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
    #3 pick             \ mma-addr n-i item-size item-size mma-addr
    _mma-set-item-size   \ mma-addr n-i item-size

    \ Get array addr.
    #2 pick             \ mma-addr n-i item-size mma-addr
    _mma-get-array      \ mma-addr n-i item-size array-addr

    \ Set up end/start do loop parameters
    rot 0               \ mma-addr item-size array-addr n-i 0

    \ Initialize each item in array, add to item addr to stack.
    do                  \ mma-addr item-size item-addr
        \ Init item, add to stack.
        dup             \ mma-addr item-size item-addr item-addr
        #3 pick         \ mma-addr item-size item-addr item-addr mma-addr
        mma-deallocate  \ mma-addr item-size item-addr

        \ Point to the next item in array.
        over            \ mma-addr item-size item-addr item-size
        +               \ mma-addr item-size next-item-addr
    loop
    
    \ Clean up.
    2drop                       \ array-addr

    dup _mma-get-stack          \ a-addr s-addr
    stack-get-num-on-stack      \ a-addr num
    over _mma-set-min-free      \ a-addr
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
    swap stack-get-num-on-stack  \ capacity free
    -
;

\ Print one-line of  
\ Run like: "<struct name>-mma .mma-usage
: .mma-usage ( mma-addr -- )
    dup             \ mma-addr mma-addr
    _mma-get-stack  \ mma-addr stack-addr

    ." Capacity: "
    dup                 \ mma-addr stack-addr | stack-addr
    stack-get-capacity  \ mma-addr stack-addr | capacity
    #5 dec.r            \ mma-addr stack-addr |
    #2 spaces

    ." Min Free: "
    over _mma-get-min-free  \ mma-addr stack-addr | min-free
    #5 dec.r                \ mma-addr stack-addr |
    #2 spaces

    ." In use: "
    dup                     \ mma-addr stack-addr | stack-addr
    stack-get-capacity      \ mma-addr stack-addr | cap
    over                    \ mma-addr stack-addr | cap stack-addr
    stack-get-num-on-stack  \ mma-addr stack-addr | cap num-on
    -                       \ mma-addr stack-addr | in-use
    space
    #5 dec.r                \ mma-addr stack-addr |
    #2 spaces

    ." Item Size: "
    over                    \ mma-addr stack-addr | mma-addr
    _mma-get-item-size      \ mma-addr stack-addr | item-size
    #3 dec.r #2 spaces      \ mma-addr stack-addr |

    ." Array size: "
    over                    \ mma-addr stack-addr | mma-addr
    _mma-get-item-size      \ mma-addr stack-addr | i-size
    over                    \ mma-addr stack-addr | i-size stack-addr
    stack-get-capacity      \ mma-addr stack-addr | i-size capacity
    *                       \ mma-addr stack-addr | a-size
    #6 dec.r #2 spaces      \ mma-addr stack-addr |

   ." Overhead: "
   dup stack-get-capacity   \ mma-addr stack-addr | s-cells
   cells                    \ mma-addr stack-addr | s-bytes
   \ Add stack overhead
   #2 cells +
   \ Add mma overhead
   #3 cells +               \ mma-addr stack-addr | o-bytes
   #6 dec.r  #2 spaces      \ mma-addr stack-addr |
   
   ." Total: "
   \ Get mma size
    over                    \ mma-addr stack-addr | mma-addr
    _mma-get-item-size      \ mma-addr stack-addr | i-size
    over                    \ mma-addr stack-addr | i-size stack-addr
    stack-get-capacity      \ mma-addr stack-addr | i-size capacity
    *                       \ mma-addr stack-addr | a-item-bytes
    #3 cells +              \ mma-addr stack-addr | a-bytes
    \ Get stack size
    over                    \ mma-addr stack-addr | a-bytes stack-addr
    stack-get-capacity      \ mma-addr stack-addr | a-bytes capacity
    #2 + cells              \ mma-addr stack-addr | a-bytes s-bytes
    \ Get full size
    + dup                   \ mma-addr stack-addr | bytes bytes
    #6 dec.r  #2 spaces     \ mma-addr stack-addr | bytes

   ." Cells: "
   cell /                   \ mma-addr stack-addr | num-cells
   #5 dec.r

   2drop
;

\ Print out addresses that are still in use.
\ Run like: <struct name>-mma .mma-in-use
: .mma-in-use ( mma-addr -- )

    \ Setup for loop.
    dup _mma-get-item-size swap     \ size mma
    dup _mma-get-stack swap         \ size stack mma
    dup _mma-get-end-addr swap      \ size stack end mma
    mma-array-start-addr            \ size stack end next-item

    begin
        2dup <>
    while
        dup                         \ size stack end item item
        #3 pick                     \ size stack end item item stack
        stack-in                    \ size stack end item flag
        if
        else
            cr dup ." In use: " hex.
        then

        #3 pick                     \ size stack end item size
        +                           \ size stack end next-item
    repeat
    cr
    \ Clear stack
    2drop 2drop                     \
;
