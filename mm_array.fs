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

\ ( mma-deallocate: Run like: <item-addr> <mma-addr> mma-deallocate )
: mma-deallocate ( item-addr mma-addr -- )
	_mm-get-stack	\ item-addr stack-addr
        over swap	\ item-addr item-addr stack-addr
	stack-push	\ item-addr
        0 swap !	\ ( zero out first cell of deallocated item )
;
\ ( mma-allocate: Run like: <mma-addr> mma-allocate -- array-item-addr )
: mma-allocate ( mma-addr -- item-addr )
	_mm-get-stack	\ stack-addr
	stack-pop	\ item-addr
;
\ ( mma-new.  Run like: <num-cells-per-item> <num-items> mma-new value <mma-name>.
: mma-new ( num-cells-per-item  num-items -- mma-addr )

        swap		\ n-i n-c-p-i
        over		\ n-i n-c-p-i n-i

        swap		\ n-i n-i n-c-p-i
        cells		\ n-i n-i item-size
        swap		\ n-i item-size n-i
        over		\ n-i item-size n-i item-size
        *		\ n-i item-size all-items-size
        2 cells +	\ n-i item-size total-size ( add two cells for the stack address, item size )

        cfalign
	here		\ n-i item-size total-size free-address
	swap		\ n-i item-size free-addr total-size
	allot		\ n-i item-size array-addr ( array memory allocated )

	rot		\ item-size array-addr n-i
	2dup		\ item-size array-addr n-i array-addr n-i
	stack-new	\ item-size array-addr n-i array-addr stack-addr ( stack allocated )
	swap		\ item-size array-addr n-i stack-addr array-addr 
	_mm-set-stack	\ item-size array-addr n-i ( stack-addr stored in first array cell )

	rot		\ array-addr (end-state) n-i item-size
	0		\ array-addr n-i item-size 0
	2over	drop	\ array-addr n-i item-size 0 array-addr
	nip		\ array-addr n-i item-size array-addr

	2dup		\ array-addr n-i item-size array-addr item-size array-addr
	_mm-set-item-size	\ array-addr n-i item-size array-addr

	2 cells +	\ array-addr n-i item-size array-addr+ ( addr after stack pointer, item size )

        rot		\ array-addr item-size array-addr+ n-i

        0 do		\ array-addr item-size array-addr+

		dup		\ array-addr item-size array-addr+ array-addr+
		3 pick		\ array-addr item-size array-addr+ array-addr+ array-addr
		mma-deallocate	\ array-addr item-size array-addr+

		over	\ array-addr item-size array-addr+ item-size
		+	\ array-addr item-size array-addr+
        loop
	cr

	2drop		\ array-addr
;

\ .mma-usage. Run like: "<mma-name> .mma-usage"
: .mma-usage ( mma-addr -- )
    dup			\ mma-addr mma-addr
    _mm-get-stack	\ mma-addr stack-addr
    ." Capacity:"
    space
    dup			\ mma-addr stack-addr stack-addr
    _stack-get-capacity	\ mma-addr stack-addr capacity
    dup			\ mma-addr stack-addr capacity capacity
    3 .r		\ mma-addr stack-addr capacity (emit capacity)
    44 emit		\ mma-addr stack-addr capacity (emit comma)
    space
    ." Free:"
    space
    dup rot		\ mma-addr capacity capacity stack-addr 
   _stack-get-num-free	\ mma-addr capacity capacity num-free
    dup			\ mma-addr capacity capacity num-free num-free
    3 .r		\ mma-addr capacity capacity num-free
    44 emit		\ mma-addr capacity capacity num-free
    space
   ." In use:"
   space
   - 3 .r		\ mma-addr capacity
   swap			\ capacity mma-addr
   space
   ." Item Size:" space
   _mm-get-item-size	\ capacity item-size
   dup			\ capacity item-size item-size
   3 .r space		\ capacity item-size
   over *		\ capacity array-size
   tuck			\ array-size capacity array-size
   ." Array size:" space
   3 .r space 		\ array-size capacity
   3 +			\ array-size capacity-cells-in-stack-plust-3-cells-overhead 
   cells		\ array-size overhead-size
   dup			\ array-size overhead-size overhead-size
   ." Overhead:" space
   3 .r	space		\ array-size overhead-size
   ." Total:" space
   + dup		\ total-size total-size
   4 .r	 space		\ total-size
   cell /		\ number cells
   ." Cells:" space
   3 .r			\ -- )
;

