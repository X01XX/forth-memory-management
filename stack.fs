\ Create a stack with:
\   The number cells in the first 16 bits of word 0.
\   The number of cells on stack (initially 0) in the second 16 bits of word 0.
\   Followed by the given number of cells.
\
\ The max size is 2^16 cells.
0                           constant stack-header-disp  \ 16-bits [0] capacity, [1] number cells on stack.
stack-header-disp   cell+   constant stack-items-disp   \ Item on the stack.

\ Set the stack capacity, use only in this file.
: _stack-set-capacity ( n stack-addr -- )
    over        \ n stack-addr n
    1 <         \ n stack-addr flag
    if
       ." Stack capacity must be GT 0"
       #-24 throw
    endif

    over        \ n stack-addr n
    #65535 >    \ n stack-addr flag
    if
       ." Stack capacity must be LT 65536"
       #-24 throw
    endif

    w!          \ Store capacity into the first 16 bits
;

\ Get the stack capacity ( stack-addr -- n )
' uw@ alias stack-get-capacity ( stack-addr -- n )

\ Get the number used
: stack-get-num-on-stack ( stack-addr -- n )
    #2 +        \ stack-addr-2nd-16-bits
    uw@         \ num-on-stack
;

\ Set the number used, use only in this file.
: _stack-set-num-on-stack ( n stack-addr -- )
    #2 +        \ n stack-addr-2nd-16-bits
    w!          \ (num-on-stack set)
;

\ stack-new.  Run like: "<number-cells> stack-new value <stack-name>" to allocate cells and save addr in <stack-name>
: stack-new ( num-cells -- stack-addr )
    dup                     \ num-cells num-cells
    1+                      \ num-cells num-cells+
    cells                   \ num-cells num-bytes
    allocate                \ num-cells stack-addr flag
    0<>
    abort" stack-new: memory allocation error"

    tuck                    \ stack-addr num-cells stack-addr

    _stack-set-capacity     \ stack-addr

    0                       \ stack-addr 0
    over                    \ stack-addr 0 stack-addr
    _stack-set-num-on-stack \ stack-addr
;

\ Return true if the stack is full
\ Run before adding a value to an stack
: stack-full? ( stack-addr -- flag )
    dup                     \ stack-addr stack-addr
    stack-get-capacity      \ stack-addr stack-capacity

    swap                    \ stack-capacity stack-addr
    stack-get-num-on-stack  \ stack-capacity stack-len

    =                       \ flag
;

\ Return true if the stack is empty
\ Run before removing a value from an stack
: stack-empty? ( stack-addr -- flag )
    stack-get-num-on-stack  \ n

    0=                      \ flag
;

\ Increment the number of cells in an stack, use only in this file.
: _stack-inc-used ( stack-addr -- )
    dup                    \ stack-addr stack-addr
    stack-get-num-on-stack \ stack-addr n

    1+ swap                 \ new-num stack-addr
    _stack-set-num-on-stack \
;

\ Decrement the number of cells in an stack, use only in this file.
: _stack-dec-used ( stack-addr -- )
    dup                     \ stack-addr stack-addr
    stack-get-num-on-stack  \ stack-addr n

    1- swap                 \ new-num stack-addr
    _stack-set-num-on-stack \ -- )
;

\ stack-push. Run like: "<value to add> <stack-name> stack-push"
\ Fails is the stack is full.
: stack-push ( n stack-addr -- )
    dup                     \ n stack-addr stack-addr
    stack-full?             \ n stack-addr flag

    if
        cr
    ." stack-push stack is full"
        #-24 throw
    then                    \ n stack-addr

    tuck                    \ stack-addr n stack-addr
    dup                     \ stack-addr n stack-addr stack-addr
    stack-items-disp +      \ stack-addr n stack-addr stack-start

    swap                    \ stack-addr n stack-start stack-addr
    stack-get-num-on-stack  \ stack-addr n stack-start num-on-stack

    cells +                 \ stack-addr n stack-cell[num-on-stack]
    !                       \ stack-addr ( n stored in cell[num-on-stack] )

    _stack-inc-used         \
;

\ stack-pop.  Run like: "<stack-name> stack-pop"
\ Fails if the stack is empty
: stack-pop ( stack-addr -- n )
    dup                         \ stack-addr stack-addr
    stack-empty?                \ stack-addr flag

    if
        cr
        ." stack-pop stack is empty"
        #-24 throw
    then                        \ stack-addr

    dup                         \ stack-addr stack-addr
    _stack-dec-used             \ stack-addr

    \ Get stack items start.
    dup stack-items-disp + swap \ stack-items stack-addr
    
    stack-get-num-on-stack      \ stack-items num-on-stack

    cells +                     \ stack-cell[last]
    @                           \ n
;

\ Return the item on top of the stack.
: stack-tos ( stack-addr -- )
    dup                         \ stack-addr stack-addr
    stack-empty?                \ stack-addr flag

    if
        cr
        ." stack-tos stack is empty"
        #-24 throw
    then                        \ stack-addr

    \ Get stack items start.
    dup stack-items-disp + swap \ stack-items stack-addr

    stack-get-num-on-stack      \ stack-addr num-on-stack

    1- cells +                  \ stack-cell[last]
    @                           \ n
;

\ .stack-stats. Run like: "<stack-name> .stack-stats"
: .stack-stats ( stack-addr -- )
    ." <"
    dup                     \ stack-addr stack-addr
    stack-get-capacity      \ stack-addr capacity
    .                       \ stack-addr (emit capacity)
    #44 emit                \ stack-addr (emit comma)
   stack-get-num-on-stack   \ num-on-stack
   .                        \
   #62 emit                 \ (emit >)
   #32 emit                 \ (emit space)
;

\ .stack. Run like: "<stack-name> .stack"
: .stack ( stack-addr -- )
   dup .stack-stats         \ stack-addr
   dup stack-items-disp +   \ stack-addr stack-items
   swap                     \ stack-items stack-addr
   stack-get-num-on-stack   \ stack-items num-on-stack

   dup 0 >                  \ stack-items num-on-stack flag
   if
     0 do                   \ stack-items
       dup                  \ stack-items stack-start
       I cells + @ u.       \ stack-items
     loop
   else
     drop                   \ stack-items
   endif
   drop                     \
;

\ Return true if an address is in the stack.
: stack-in ( addr stack-addr -- flag )
   dup stack-items-disp +   \ addr stack stack-start
   swap                     \ addr stack-start stack
   stack-get-num-on-stack   \ addr stack-start num-on-stack

   dup 0 >                  \ addr stack-start num-on-stack flag
   if
     0 do                   \ addr stack-start
       dup                  \ addr stack-start stack-start
       I cells + @          \ addr stack-start stack-cell[I]
       #2 pick              \ addr stack-start stack-cell[I] addr
       =                    \ addr stack-start
       if
            2drop true
            unloop
            exit
       then
     loop
   else
     drop               \ addr stack-start
   endif
   2drop                \
   false
;

: stack-tests
    #2 stack-new          \ stk
    dup stack-empty?
    0= abort" stock s/b empty"

    1 over stack-push
    #2 over stack-push

    dup stack-full?
    0= abort" stack s/b full"

    free
    if
        cr ." stack free failed" cr
    else
        cr ." stack test Ok" cr
    then
;
