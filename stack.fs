\ Create a stack with:
\   The number cells in the first 16 bits of word 0.
\   The number of used cells (initially 0) in the second 16 bits of word 0.
\   Followed by the given number of cells.
\
\ The max size is 2^16 cells. 

\ Set the stack capacity
: _stack-set-capacity ( n stack-addr -- )
    over	\ n stack-addr n
    1 <		\ n stack-addr flag
    if
       ." Stack capacity must be GT 0"
       -24 throw
    endif

    over	\ n stack-addr n
    65535 >	\ n stack-addr flag
    if
       ." Stack capacity must be LT 65536"
       -24 throw
    endif

    w!		\ Store capacity into the first 16 bits
;

\ Get the stack capacity
: _stack-get-capacity ( stack-addr -- n )
    uw@		\ Fetch capacity frim the first 16 bits
;

\ Get the number used
: _stack-get-num-used ( stack-addr -- n )
    2 +		\ stack-addr-2nd-16-bits
    uw@		\ num-used
;

\ Set the number used
: _stack-set-num-used ( n stack-addr -- )
    2 +		\ n stack-addr-2nd-16-bits
    w!		\ (num-used set)
;

\ Get the start of the stack
: _stack-get-start ( stack-addr -- start-addr )
    1 cells +	\ start-addr
;

\ stack-new.  Run like: "<number-cells> stack-new value <stack-name>" to allocate cells and save addr in <stack-name>
: stack-new ( num-cells -- stack-addr )
    here	\ num-cells stack-addr
    swap	\ stack-addr num-cells
    dup		\ stack-addr num-cells num-cells
    1 +		\ stack-addr num-cells num-cells+
    cells	\ stack-addr num-cells num-bytes
    allot	\ stack-addr num-cells ( memory allocated )
    over	\ stack-addr num-cells stack-addr

    _stack-set-capacity	\ stack-addr

    0           	\ stack-addr 0
    over		\ stack-addr 0 stack-addr
    _stack-set-num-used	\ stack-addr
;

\ Return true if the stack is full
\ Run before adding a value to an stack
: _stack-full? ( stack-addr -- flag )
    dup			\ stack-addr stack-addr
    _stack-get-capacity	\ stack-addr stack-capacity

    swap		\ stack-capacity stack-addr
    _stack-get-num-used	\ stack-capacity stack-len

    =			\ flag
;

\ Return true if the stack is empty
\ Run before removing a value from an stack
: _stack-empty? ( stack-addr -- flag )
    _stack-get-num-used	\ n

    0=			\ flag
;

\ Increment the number of cells in an stack
: _stack-inc-used ( stack-addr -- )
    dup			\ stack-addr stack-addr
    _stack-get-num-used	\ stack-addr n

    1+ swap		\ new-num stack-addr
    _stack-set-num-used	\
;

\ Decrement the number of cells in an stack
: _stack-dec-used ( stack-addr -- )
    dup			\ stack-addr stack-addr
    _stack-get-num-used	\ stack-addr n

    1- swap		\ new-num stack-addr
    _stack-set-num-used	\
;

\ stack-push. Run like: "<value to add> <stack-name> stack-push" 
\ Fails is the stack is full.
: stack-push ( n stack-addr -- )
    dup			\ n stack-addr stack-addr
    _stack-full?	\ n stack-addr flag

    if
        cr
	." stack-push stack is full"
        -24 throw
    then		\ n stack-addr

    swap over		\ stack-addr n stack-addr
    dup 		\ stack-addr n stack-addr stack-addr
    _stack-get-start	\ stack-addr n stack-addr stack-start

    swap		\ stack-addr n stack-start stack-addr
    _stack-get-num-used	\ stack-addr n stack-start num-used
  

    
    cells +	 	\ stack-addr n stack-cell[num-used]
    !           	\ stack-addr ( n stored in cell[num-used] )

    _stack-inc-used	\
;

\ stack-pop.  Run like: "<stack-name> stack-pop"
\ Fails if the stack is empty
: stack-pop ( stack-addr -- n )
    dup			\ stack-addr stack-addr
    _stack-empty?	\ stack-addr flag

    if
        cr
	." stack-pop stack is empty"
        -24 throw
    then		\ stack-addr

    dup			\ arra-addr stack-addr
    _stack-dec-used	\ stack-addr

    dup			\ stack-addr stack-addr
    _stack-get-num-used	\ stack-addr num-used

    1 + cells +		\ stack-cell[last]
    @			\ n
;

\ .stack. Run like: "<stack-name> .stack"
: .stack ( stack-addr -- )
    ." <"
    dup			\ stack-addr stack-addr
    _stack-get-capacity	\ stack-addr capacity
    .			\ stack-addr (emit capacity)
    44 emit		\ stack-addr (emit comma)
    dup			\ stack-addr stack-addr
   _stack-get-start	\ stack-addr stack-start
   swap			\ stack-start stack-addr
   _stack-get-num-used	\ stack-start num-used
   dup			\ stack-start num-used num-used
   .			\ stack-start num-used
   62 emit		\ stack-start num-used (emit >)
   32 emit		\ stack-start num-used (emit space)

   dup 0 > 		\ stack-start num-used flag
   if
     0 do		\ stack-start
       dup		\ stack-start stack-start
       I cells + @ .	\ stack-start stack-cell[I]
     loop
   else
     drop		\ stack-start  
   endif
   drop			\
;
