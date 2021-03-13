include mm_array.fs

\ Return the first link addr of a list-header
: list-get-first-link ( list-header -- link-addr )
    @
;

\ Set the first link addr of a list-header
: list-set-first-link ( link-addr list-header -- )
    !
;

\ Get the length of a list
: list-get-len ( list-addr - n )
    1 cells + @
;

\ Set the length of a list
: list-set-len ( n list-addr - )
    1 cells + !
;

\ Return the next pointer of a link
: link-get-next ( link-addr -- next-link-addr )
    @
;

\ Set the next pointer of a link (0 or another link)
: link-set-next ( addr link -- )
    !
;

\ Return the data pointer of a list link
: link-get-data ( link-addr -- data-addr )
    1 cells + @
;

\ Set link data pointer 
: link-set-data ( data link -- )
    1 cells + !
;

\ Get num value from num item
: num-get-value ( num-addr -- n )
    @
;

\ Set num value in num item
: num-set-value ( n num-addr -- )
    !
;

\ Return the address for a list header, link-addr and num-items initialized to zero
: list-new ( list-header-store -- list-header-addr )
	mma-allocate		\ ( list-header-store -- list-header-addr )

        \ Zero out first link addr
	0 over	 		\ ( list-header-addr -- list-header-addr 0 list-header-addr )
        list-set-first-link		\ ( list-header-addr 0 list-header-addr -- list-header-addr )

	\ Zero out list length
	0 over			\ ( list-header-addr -- list-header-addr 0 list-header-addr )
	list-set-len		\ ( list-header-addr 0 list-header-addr -- list-header-addr )
;

\ Add an address to the beginning of a list
: list-add-link ( link-addr list-header-addr -- ) 
	\ Store list header next pointer to link next pointer
        2dup		( link-addr list-header-addr -- link-addr list-header-addr any-addr list-header-addr )
        list-get-first-link	( link-addr list-header-addr link-addr list-header-addr  -- link-addr list-header-addr link-addr header-next-pointer )
        swap		( link-addr list-header-addr link-addr header-next-pointer -- link-addr list-header-addr list-get-first-link link )
        link-set-next	( link-addr list-header-addr list-get-first-link link  -- link-addr list-header-addr )

        \ Store link pointer to header next pointer
        swap over	( link-addr list-header-addr  -- list-header-addr link-addr list-header-addr )
        list-set-first-link  ( list-header-addr link-addr list-header-addr -- list-header-addr )

        \ Update the list count 
	1 cells +	( list-header-addr -- list-header-cell2-addr )
        1 swap +!	( list-header-cell2-addr -- )
;
 
\ Add a link to the end of a list
: list-push-link ( link-addr list-header-addr -- )

    \ Check for an empty list
    dup			\ link-addr list-header list-header
    list-get-first-link \ link-addr list-header link-next
    dup if
	begin
	    nip			\ link-new link-next
	    dup			\ link-new link-next link-next
	    link-get-next	\ link-new link-next link-next-next
	    dup			\ link-new link-next link-next-next link-next-next
	while			\ link-new link-next link-next-next
	repeat
	drop			\ link-new link-last
	link-set-next		\
    else
	drop			\ link-addr list-header
	list-set-first-link	\ 
    then
;

: .num-list ( list-addr -- )
    ." ("
    @		\ next-link-addr
    begin
    dup
    while
        dup link-get-data
	num-get-value 1 .r
    	link-get-next	\ next-link-addr
        dup if
	    ." ," space
	then
    repeat
    
    ." )"
    drop
;

: .list-header ( list-add -- )
	." List addr:"
	space
	dup .
	." List Next ptr:
	space
	dup list-get-first-link .
	." Num items:"
	space
	list-get-len .
;

\ Allocate a cell for a number, store the number, return the number cell-addr
: num-new ( n num-store-addr -- num-addr )
	 mma-allocate		( n num-store-addr -- n num-addr )
         swap over		( n num-addr -- num-addr n num-addr )
         num-set-value		( num-addr n num-addr -- num-addr )
;

: .num ( num-addr -- )
	." Num  addr:"
	space
	dup .
	." Num:"
	space
	num-get-value .
;

\ Return a new link
: link-new ( any-addr link-store -- link-addr )
	mma-allocate		\ ( any-addr link-store -- any-addr link-addr )
        swap over		\ ( any-addr link-addr -- link-addr any-addr link-addr )
        link-set-data		\ ( link-addr any-addr link-addr -- link-addr ) ( any-addr stored in second, data-pointer , cell )
        0 over link-set-next	\ ( link-addr -- link-addr ) ( 0 stored in first, next-pointer, cell )
;

: .link ( link-addr -- )
	." Link addr:"
	space
	dup .
	." Link Next ptr:"
	space
	dup link-get-next .
	space
	." Num ptr:"
	space
	dup link-get-data .
	." Num:"
	space
	link-get-data num-get-value .
;

\ ### M A I N ####

2 15 mma-new value list-header-store	\ Initialize linked list header store.		( link addr, num items, both initially zero )
2 30 mma-new value link-store		\ Initialize store for linked list links.	( next-link-addr, num-addr )
1 30 mma-new value num-store		\ Initialize store for numbers.			( Just a number )

\ Add a number to the beginning of a list
: num-list-add ( n num-list-addr -- )
    swap 			\ num-list-addr n 
    num-store num-new		\ num-list-addr num-item-addr
    link-store link-new		\ num-list-addr link-item-addr 
    swap			\ link-item-addr num-list-addr
    list-add-link		\
;

\ Add a number to the end of the list
: num-list-push ( n num-list-addr -- )
    swap 			\ num-list-addr n 
    num-store num-new		\ num-list-addr num-item-addr
    link-store link-new		\ num-list-addr link-item-addr 
    swap			\ link-item-addr num-list-addr
    list-push-link		\
;


\ Return true if a number if in a num-list
: num-list-num-in ( n num-list -- flag )

    list-get-first-link		\ n list-get-first-link
    false rot rot		\ false n list-get-first-link
    begin
    dup				\ flag n link link
    while
    	2dup			\ flag n cur-link n cur-link
    	link-get-data		\ flag n cur-link n data
        num-get-value		\ flag n cur-link n m
	= if			\ flag n cur-link
		rot drop	\ n cur-link-addr
		true rot rot	\ true n cur-link
	then

    	link-get-next		\ flag n next-link
    repeat
    
    2drop
;

\ Return the intersection of two num lists
: num-list-intersection ( list1 list2 -- list-intersection )

    list-header-store list-new	\ list1 list2 list-ret
    >r				\ list1 list2 R: list-ret

    list-get-first-link		\ list1 list-get-first-link R: list-ret
    begin
    dup				\ list1 next-link next-link R: list-ret
    while			\ list1 next-link R: list-ret

        2dup			\ list1 cur-link list1 cur-link R: list-ret

        link-get-data		\ list1 cur-link list1 data R: list-ret
        num-get-value		\ list1 cur-link list1 n R: list-ret

        dup			\ list1 cur-link list1 n n R: list-ret
        rot			\ list1 cur-link n n list1 R: list-ret
      
        num-list-num-in		\ list1 cur-link n flag R: list-ret
        
        if			\ list1 cur-link n R: list-ret

		r@		\ list1 cur-link n list-ret R: list-ret

		\ Check if the number is already in the return list
		2dup		\ list1 cur-link n list-ret n list-ret R: list-ret
        	num-list-num-in	\ list1 cur-link n list-ret flag R: list-ret

		if 
			2drop	\ list1 cur-link R: list-ret
		else
			num-list-add	\ list1 cur-link R: list-ret
		then
	else
		drop		\ list1 cur-link R: list-ret
	then

    	link-get-next		\ list1 next-link R: list-ret
    repeat
    2drop			\ R: list-ret
    r>				\ list-ret
;

\ Return a list multiplied by a number
: num-list-multiply ( n list1 -- list2 )

    list-header-store list-new	\ n list1 list-ret
    >r				\ n list1 R: list-ret

    list-get-first-link		\ n list-get-first-link R: list-ret
    begin
    dup				\ n next-link next-link R: list-ret
    while			\ n next-link R: list-ret

	2dup			\ n cur-link n cur-link R: list-ret

        link-get-data		\ n cur-link n data R: list-ret
        num-get-value		\ n cur-link n m R: list-ret

		*		\ n cur-link y R: list-ret
		r@		\ n cur-link y list-ret R: list-ret

		num-list-push	\ n cur-link R: list-ret

    	link-get-next		\ n next-link R: list-ret
    repeat
    2drop			\ R: list-ret
    r>				\ list-ret
;

\ Return a list plus a number
: num-list-plus ( n list1 -- list2 )
    list-header-store list-new	\ n list1 list-ret
    >r				\ n list1 R: list-ret

    list-get-first-link		\ n list-get-first-link R: list-ret
    begin
    dup				\ n next-link next-link R: list-ret
    while			\ n next-link R: list-ret

	2dup			\ n cur-link n cur-link R: list-ret

        link-get-data		\ n cur-link n data R: list-ret
        num-get-value		\ n cur-link n m R: list-ret

	+			\ n cur-link y R: list-ret

	r@			\ n cur-link y list-ret R: list-ret

	num-list-push		\ n cur-link R: list-ret

    	link-get-next		\ n next-link R: list-ret
    repeat
    2drop			\ R: n list-ret
    r>				\ list-ret
;

\ Return the difference of two num lists, same order as in subrtracting numbers in forth, list1 - list2
: num-list-difference ( list1 list2 -- list-difference )

    swap			\ list2 list1
    list-header-store list-new	\ list2 list1 list-ret
    >r				\ list2 list1 R: list-ret

    list-get-first-link		\ list2 list-get-first-link R: list-ret
    begin
    dup				\ list2 next-link next-link R: list-ret
    while			\ list2 next-link R: list-ret

        2dup			\ list2 cur-link list2 cur-link R: list-ret

        link-get-data		\ list2 cur-link list2 data R: list-ret
        num-get-value		\ list2 cur-link list2 n R: list-ret

        dup			\ list2 cur-link list2 n n R: list-ret
        rot			\ list2 cur-link n n list2 R: list-ret
      
        num-list-num-in		\ list2 cur-link n flag R: list-ret
       
        if			\ list2 cur-link n R: list-ret
		drop		\ list2 cur-link R: list-ret
	else

		r@		\ list2 cur-link n list-ret R: list-ret

		\ Check if the number is already in the return list
		2dup		\ list2 cur-link n list-ret n list-ret R: list-ret
        	num-list-num-in	\ list2 cur-link n list-ret flag R: list-ret

		if 
			2drop	\ list2 cur-link R: list-ret
		else
			num-list-add	\ list2 cur-link R: list-ret
		then
	then

    	link-get-next		\ list2 next-link R: list-ret
    repeat
    2drop			\ R: list-ret
    r>				\ list-ret
;

\ Return the union of two num lists
: num-list-union ( list1 list2 -- list-union )

    list-header-store list-new >r	\ list1 list2 R: list-ret

    \ Process list2 (copy may seem like a better option, but this gets rid of duplicates)
    list-get-first-link		\ list1 list-get-first-link R: list-ret
    begin
    dup				\ list1 next-link next-link R: list-ret
    while			\ list1 cur-link R: list-ret

	dup			\ list1 cur-link cur-link R: list-ret
        link-get-data		\ list1 cur-link data R: list-ret
        num-get-value		\ list1 cur-link n R: list-ret
        dup			\ list1 cur-link n n R: list-ret

        r@ num-list-num-in	\ list1 cur-link n flag R: list-ret
        if			\ list1 cur-link n R: list-ret
		drop		\ list1 cur-link R: list-ret
	else
		r@		\ list1 cur-link n list-ret R: list-ret
		num-list-add	\ list1 cur-link R: list-ret
	then

    	link-get-next		\ list1 next-link R: list-ret
    repeat

    drop			\ list1 R: list-ret

    \ Process list1
    list-get-first-link		\ list-get-first-link R: list-ret
    begin
    dup				\ next-link next-link R: list-ret
    while			\ cur-link R: list-ret

	dup			\ cur-link cur-link R: list-ret
        link-get-data		\ cur-link data R: list-ret
        num-get-value		\ cur-link n R: list-ret
        dup			\ cur-link n n R: list-ret

        r@ num-list-num-in	\ cur-link n flag R: list-ret
        if			\ cur-link n R: list-ret
		drop		\ cur-link R: list-ret
	else
		r@		\ cur-link n list-ret R: list-ret
		num-list-add	\ cur-link R: list-ret
	then

    	link-get-next		\ next-link R: list-ret
    repeat

    drop			\ R: list-ret
    r>				\ list-ret
;

: num-list-deallocate ( list-addr -- )
    dup list-get-first-link	\ list-addr cur-link-addr
    begin
        dup		\ list-addr cur-link-addr cur-link-addr (flag, zero or not)
    while		\ list-addr cur-link-addr 
        dup link-get-data 		\ list-addr cur-link-addr num-addr
	num-store mma-deallocate	\ list-addr cur-link-addr
        dup link-get-next		\ list-addr cur-link-addr next-link-addr
        swap				\ list-addr next-link-addr cur-link-addr
	link-store mma-deallocate	\ list-addr next-link-addr
    repeat
    drop				\ list-addr
    list-header-store mma-deallocate	\
;

\ Print memory use for num-lists
: num-list-memory-use ( -- )
	cr
	." Memory Use:" cr
	2 spaces ." list-header-store" space
	list-header-store .mma-usage cr
	2 spaces ." link-store" 8 spaces
	link-store .mma-usage cr
	2 spaces ." num-store" 9 spaces
	num-store .mma-usage cr
;

list-header-store list-new value list1	\ Get linked list header for a new list, store it in word list1
cr
5 list1 num-list-add
3 list1 num-list-add
1 list1 num-list-add
5 list1 num-list-add
." list1: "
list1 .num-list
cr

list-header-store list-new value list2	\ Get linked list header for a new list, store it in word list2
cr
1 list2 num-list-add
2 list2 num-list-add
5 list2 num-list-add
5 list2 num-list-add
." list2: "
list2 .num-list
cr

cr
." list3: "
list1 list2 num-list-intersection value list3
list3 .num-list
3 spaces ." (intersection, no duplicates)"
cr

cr
." list4: "
list1 list2 num-list-union value list4
list4 .num-list
3 spaces ." (union, no duplicates)"
cr

cr
." list5: "
list1 list2 num-list-difference value list5
list5 .num-list
3 spaces ." (list1 - list2)"
cr

cr
." list6: "
list2 list1 num-list-difference value list6
list6 .num-list
3 spaces ." (list2 - list1)"
cr

cr
." list7: "
2 list1 num-list-multiply value list7
list7 .num-list
3 spaces ." (list1 * 2)"
cr

cr
." list8: "
-1 list1 num-list-plus value list8
list8 .num-list
3 spaces ." (list1 + -1)"
cr

num-list-memory-use

cr
." Deallocating ..."
cr

list1 num-list-deallocate
list2 num-list-deallocate
list3 num-list-deallocate
list4 num-list-deallocate
list5 num-list-deallocate
list6 num-list-deallocate
list7 num-list-deallocate
list8 num-list-deallocate

num-list-memory-use

