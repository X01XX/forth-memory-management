\ Create a stack with:
\   The number cells in the first 16 bits of word 0.
\   The number of free cells (initially 0) in the second 16 bits of word 0.
\   Followed by the given number of cells.
\
\ The max size is 2^16 cells. 

\ Set the stack capacity, use only in this file.
: _stack-set-capacity ( n stack-addr -- )
    over    \ n stack-addr n
    1 <     \ n stack-addr flag
    if
       ." Stack capacity must be GT 0"
       -24 throw
    endif

    over    \ n stack-addr n
    65535 > \ n stack-addr flag
    if
       ." Stack capacity must be LT 65536"
       -24 throw
    endif

    w!      \ Store capacity into the first 16 bits
;

\ Get the stack capacity ( stack-addr -- n )
' uw@ alias stack-get-capacity ( stack-addr -- n )

\ Get the number used
: stack-get-num-free ( stack-addr -- n )
    2 +     \ stack-addr-2nd-16-bits
    uw@     \ num-free
;

\ Set the number used, use only in this file.
: _stack-set-num-free ( n stack-addr -- )
    2 +     \ n stack-addr-2nd-16-bits
    w!      \ (num-free set)
;

\ Get the start of the stack ( stack-addr -- start-addr )
' cell+ alias _stack-get-start

\ stack-new.  Run like: "<number-cells> stack-new value <stack-name>" to allocate cells and save addr in <stack-name>
: stack-new ( num-cells -- stack-addr )
    dup         \ num-cells num-cells
    1+          \ num-cells num-cells+
    cells       \ num-cells num-bytes
    allocate    \ num-cells stack-addr flag
    0<>
    if
        ." stack-new: memory allocation error"
        abort
    then
    tuck    \ stack-addr num-cells stack-addr

    _stack-set-capacity \ stack-addr

    0                   \ stack-addr 0
    over                \ stack-addr 0 stack-addr
    _stack-set-num-free \ stack-addr
;

\ Return true if the stack is full
\ Run before adding a value to an stack
: stack-full? ( stack-addr -- flag )
    dup                 \ stack-addr stack-addr
    stack-get-capacity  \ stack-addr stack-capacity

    swap                \ stack-capacity stack-addr
    stack-get-num-free  \ stack-capacity stack-len

    =                   \ flag
;

\ Return true if the stack is empty
\ Run before removing a value from an stack
: stack-empty? ( stack-addr -- flag )
    stack-get-num-free  \ n

    0=                  \ flag
;

\ Increment the number of cells in an stack, use only in this file.
: _stack-inc-used ( stack-addr -- )
    dup                    \ stack-addr stack-addr
    stack-get-num-free     \ stack-addr n

    1+ swap             \ new-num stack-addr
    _stack-set-num-free \
;

\ Decrement the number of cells in an stack, use only in this file.
: _stack-dec-used ( stack-addr -- )
    dup                 \ stack-addr stack-addr
    stack-get-num-free  \ stack-addr n

    1- swap             \ new-num stack-addr
    _stack-set-num-free \ -- )
;

\ stack-push. Run like: "<value to add> <stack-name> stack-push" 
\ Fails is the stack is full.
: stack-push ( n stack-addr -- )
    dup             \ n stack-addr stack-addr
    stack-full?     \ n stack-addr flag

    if
        cr
    ." stack-push stack is full"
        -24 throw
    then                \ n stack-addr

    swap over           \ stack-addr n stack-addr
    dup                 \ stack-addr n stack-addr stack-addr
    _stack-get-start    \ stack-addr n stack-addr stack-start

    swap                \ stack-addr n stack-start stack-addr
    stack-get-num-free  \ stack-addr n stack-start num-free

    cells +             \ stack-addr n stack-cell[num-free]
    !                   \ stack-addr ( n stored in cell[num-free] )

    _stack-inc-used     \
;

\ stack-pop.  Run like: "<stack-name> stack-pop"
\ Fails if the stack is empty
: stack-pop ( stack-addr -- n )
    dup             \ stack-addr stack-addr
    stack-empty?    \ stack-addr flag

    if
        cr
        ." stack-pop stack is empty"
        -24 throw
    then                \ stack-addr

    dup                 \ array-addr stack-addr
    _stack-dec-used     \ stack-addr

    dup                 \ stack-addr stack-addr
    stack-get-num-free  \ stack-addr num-free

    1+ cells +          \ stack-cell[last]
    @                   \ n
;

\ .stack-stats. Run like: "<stack-name> .stack-stats"
: .stack-stats ( stack-addr -- )
    ." <"
    dup                    \ stack-addr stack-addr
    stack-get-capacity  \ stack-addr capacity
    .                   \ stack-addr (emit capacity)
    44 emit             \ stack-addr (emit comma)
   stack-get-num-free   \ num-free
   .                    \ 
   62 emit              \ (emit >)
   32 emit              \ (emit space)
;

\ .stack. Run like: "<stack-name> .stack"
: .stack ( stack-addr -- )
   dup .stack-stats     \ stack-addr
   dup cell+            \ stack-addr stack-start
   swap                 \ stack-start stack-addr
   stack-get-num-free   \ stack-start num-free

   dup 0 >              \ stack-start num-free flag
   if
     0 do               \ stack-start
       dup              \ stack-start stack-start
       I cells + @ u.   \ stack-start stack-cell[I]
     loop
   else
     drop               \ stack-start  
   endif
   drop                 \
;

