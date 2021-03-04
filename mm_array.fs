\ Memmory Management array
\
\ Create and manage an array, where:
\   Allocation involves popping an array element address from a stack,
\   Deallocation involves pushing an array element address onto the stack.

include stack.fs 
cr
.( mma-deallocate: Run like: <item-addr> <mma-addr> mma-deallocate )
: mma-deallocate ( item-addr mma-addr -- )
	@		\ item-addr stack-addr
	stack-push	\
;
cr
.( mma-allocate: Run like: <mma-addr> mma-allocate -- array-item-addr )
: mma-allocate ( mma-addr -- item-addr )
	@		\ stack-addr
	stack-pop	\ item-addr
;
cr
.( mma-new.  Run like: <num-cells-per-item> <num-items> mma-new value <mma-name>.
: mma-new ( num-cells-per-item  num-items -- mma-addr )

        swap		\ n-i n-c-p-i
        over		\ n-i n-c-p-i n-i

        swap		\ n-i n-i n-c-p-i
        cells		\ n-i n-i item-size
        swap		\ n-i item-size n-i
        over		\ n-i item-size n-i item-size
        *		\ n-i item-size all-items-size
        1 cells +	\ n-i item-size total-size ( add one word for the stack address )

	here		\ n-i item-size total-size free-address
	swap		\ n-i item-size free-addr total-size
	allot		\ n-i item-size array-addr ( array memory allocated )

	rot		\ item-size array-addr n-i
	2dup		\ item-size array-addr n-i array-addr n-i
	stack-new	\ item-size array-addr n-i array-addr stack-addr ( stack allocated )
	swap		\ item-size array-addr n-i stack-addr array-addr 
	!		\ item-size array-addr n-i ( stack-addr stored in first array cell )

	rot		\ array-addr (end-state) n-i item-size
	0		\ array-addr n-i item-size 0
	2over	drop	\ array-addr n-i item-size 0 array-addr
	nip		\ array-addr n-i item-size array-addr
	1 cells +	\ array-addr n-i item-size array-addr+ ( addr after stack pointer )

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
cr
