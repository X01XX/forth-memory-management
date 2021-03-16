include mm_array.fs

\ Return the first link addr of a list-header  ( list-header -- link-addr )

' @ alias list-get-first-link

\ Set the first link addr of a list-header ( link-addr list-header -- )
' ! alias list-set-first-link

\ Get the length of a list
: list-get-len ( list-addr - n )
    cell + @
;

\ Set the length of a list
: list-set-len ( n list-addr - )
    cell + !
;

\ Return the next pointer of a link ( link-addr -- next-link-addr )
' @ alias link-get-next

\ Set the next pointer of a link ( addr link -- ) addr can be zero
' ! alias link-set-next

\ Return the data pointer of a list link
: link-get-data ( link-addr -- data-addr )
    cell + @
;

\ Set link data pointer 
: link-set-data ( data link -- )
    cell + !
;

\ Get num value from num item ( num-addr -- n )
' @ alias num-get-value

\ Set num value in num item ( n num-addr -- )
' ! alias num-set-value

\ Return the address for a list header, link-addr and num-items initialized to zero
: list-new ( list-header-store -- list-addr )
	mma-allocate		\ list-addr

        \ Zero out first link addr
	0 over	 		\ list-addr 0 list-addr
        list-set-first-link	\ list-addr 

	\ Zero out list length
	0 over			\ list-addr 0 list-addr
	list-set-len		\ list-addr
;

\ Add an address to the beginning of a list
: list-add-link ( link-addr list-addr -- ) 

	\ Store list header next pointer to link next pointer
        2dup			\ link-addr list-addr link-addr list-addr 
        list-get-first-link	\ link-addr list-addr link-addr first-link
        swap			\ link-addr list-addr first-link link-addr
        link-set-next		\ link-addr list-addr

        \ Store link pointer to header next pointer
        swap over		\ list-addr link-addr list-addr
        list-set-first-link  	\ list-addr

        \ Update the list count 
        1 swap list-set-len	\ -- )
;
 
\ Add a link to the end of a list
: list-push-link ( link-addr list-addr -- )

    \ Check for an empty list
    dup				\ link-addr list-addr list-addr
    list-get-first-link 	\ link-addr list-addr first-link
    dup if
	begin
	    nip			\ link-new link-next
	    dup			\ link-new link-next link-next
	    link-get-next	\ link-new link-next link-next-next
	    dup			\ link-new link-next link-next-next link-next-next
	while			\ link-new link-next link-next-next
	repeat
	drop			\ link-new link-last
	link-set-next		\ -- )
    else
	drop			\ link-addr list-header
	list-set-first-link	\ -- )
    then
;

: .list-header ( list-addr -- )
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
2 60 mma-new value link-store		\ Initialize store for linked list links.	( next-link-addr, num-addr )
1 30 mma-new value num-store		\ Initialize store for numbers.			( Just a number )
float cell / 30 mma-new value fpn-store		\ Initialize store for floating point numbers.

\ Allocate a cell for a number, store the number, return the number cell-addr
: num-new ( n num-store-addr -- num-addr )
	 mma-allocate		\ n num-addr
         swap over		\ num-addr n num-addr
         num-set-value		\ num-addr
;

: .num ( num-addr -- )
	." Num  addr:"
	space
	dup .
	." Num:"
	space
	num-get-value .
;

: .num-list ( list-addr -- )
    ." ("
    list-get-first-link		\ first-link
    
    begin
    dup
    while
        dup link-get-data	\ link data-addr
	num-get-value 1 .r	\ link
    	link-get-next		\ next-link
        dup if
	    ." ," space
	then
    repeat
    
    ." )"
    drop			\ -- )
;

\ Add a number to the beginning of a list
: num-list-add ( n num-list-addr -- )
    swap 			\ num-list-addr n 
    num-store num-new		\ num-list-addr num-item-addr
    link-store link-new		\ num-list-addr link-item-addr 
    swap			\ link-item-addr num-list-addr
    list-add-link		\ -- )
;

\ Add a number to the end of the list
: num-list-push ( n num-list-addr -- )
    swap 			\ num-list-addr n 
    num-store num-new		\ num-list-addr num-item-addr
    link-store link-new		\ num-list-addr link-item-addr 
    swap			\ link-item-addr num-list-addr
    list-push-link		\ -- )
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
    
    2drop			\ flag
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

    list-get-first-link		\ n first-link R: list-ret
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

    list-get-first-link		\ n first-link R: list-ret
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

    list-get-first-link		\ list2 first-link R: list-ret
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
    dup list-get-first-link		\ list-addr cur-link-addr
    begin
        dup				\ list-addr cur-link-addr cur-link-addr
    while				\ list-addr cur-link-addr 
        dup link-get-data 		\ list-addr cur-link-addr num-addr
	num-store mma-deallocate	\ list-addr cur-link-addr
        dup link-get-next		\ list-addr cur-link-addr next-link-addr
        swap				\ list-addr next-link-addr cur-link-addr
	link-store mma-deallocate	\ list-addr next-link-addr
    repeat
    drop				\ list-addr
    list-header-store mma-deallocate	\ -- )
;

\ Print memory use for num-lists
: memory-use ( -- )
	cr
	." Memory Use:" cr
	2 spaces ." list-header-store" space
	list-header-store .mma-usage cr
	2 spaces ." link-store" 8 spaces
	link-store .mma-usage cr
	2 spaces ." num-store" 9 spaces
	num-store .mma-usage cr
	2 spaces ." fpn-store" 9 spaces
	fpn-store .mma-usage cr
;

\ Get fpn value from fpn item ( fpn-addr -- n )
' f@ alias fpn-get-value

\ Set fpn value in fpn item ( n num-addr -- )
' f! alias fpn-set-value

\ Allocate two cells for a fp number, store the number, return the number cell-addr
: fpn-new ( fpn-store-addr F: n -- fpn-addr )
	 mma-allocate		\ fpn-addr F: n
         dup			\ fpn-addr fpn-addr F: n
         fpn-set-value		\ fpn-addr
;

\ Add a floating point number to the beginning of a list
: fpn-list-add ( num-list-addr F: n -- )
    fpn-store fpn-new		\ num-list-addr num-item-addr
    link-store link-new		\ num-list-addr link-item-addr 
    swap			\ link-item-addr num-list-addr
    list-add-link		\ -- )
;

\ Print a floating point list
: .fpn-list ( list-addr -- )
    ." ("
    list-get-first-link		\ first-link-addr
    begin
    dup				\ link link
    while
        dup link-get-data	\ link data
	fpn-get-value f.	\ link
    	link-get-next		\ next-link
        dup if			\ If not at end, print a comma
	    ." ," space
	then
    repeat
    
    ." )"
    drop			\ -- )
;

\ Print a list of floating point lists
: .list-of-lists-fpn ( list-addr -- )
    ." ("
    list-get-first-link		\ first-link
    begin
    dup				\ link link
    while
        dup link-get-data	\ link data
        .fpn-list		\ link
    	link-get-next		\ next-link
        dup if			\ If not at end, print comma
	    ." ," space
	then
    repeat
    
    ." )"
    drop			\ -- )
;

\ Add a fpn to the end of the list
: fpn-list-push ( num-list F: n -- )
    fpn-store fpn-new		\ fpn-list fpn-item
    link-store link-new		\ fpn-list link
    swap			\ link fpn-list
    list-push-link		\ -- )
;

\ Return a fpn list multiplied by a fp number
: fpn-list-multiply ( list1 F: n -- list2 )

    list-header-store list-new	\ list1 list-ret F: n
    >r				\ list1 F: n R: list-ret

    list-get-first-link		\ first-link F: n R: list-ret
    begin
    dup				\ next-link next-link F: n R: list-ret
    while			\ next-link F: n R: list-ret

	dup			\ cur-link cur-link F: n R: list-ret

        link-get-data		\ cur-link data F: n R: list-ret
	fdup			\ cur-link data F: n n R: ret-list
        fpn-get-value		\ cur-link F: m n n R: list-ret

		f*		\ cur-link F: y n  R: list-ret
		r@		\ cur-link list-ret F: y n R: list-ret

		fpn-list-push	\ cur-link F: n R: list-ret

    	link-get-next		\ next-link F: n R: list-ret
    repeat
    drop fdrop			\ R: list-ret
    r>				\ list-ret
;

\ Return a fpn list plus an fp number
: fpn-list-plus ( list1 F: n -- list2 )

    list-header-store list-new	\ list1 list-ret F: n
    >r				\ list1 F: n R: list-ret

    list-get-first-link		\ first-link F: n R: list-ret
    begin
    dup				\ next-link next-link F: n R: list-ret
    while			\ next-link F: n R: list-ret

	dup			\ cur-link cur-link F: n R: list-ret

        link-get-data		\ cur-link data F: n R: list-ret
	fdup			\ cur-link data F: n n R: ret-list
        fpn-get-value		\ cur-link F: m n n R: list-ret

		f+		\ cur-link F: y n  R: list-ret
		r@		\ cur-link list-ret F: y n R: list-ret

		fpn-list-push	\ cur-link F: n R: list-ret

    	link-get-next		\ next-link F: n R: list-ret
    repeat
    drop fdrop			\ R: list-ret
    r>				\ list-ret
;

\ Return a fpn list plus fpn list
: fpnl-plus-fpnl ( list1 list2 -- list3)

    \ Check the lengths are the same
    over list-get-len		\ list1 list2 len1
    over list-get-len		\ list1 list2 len1 len2
    <> if 			\ list1 list2 flag
	cr
	." fpnl-plus-fpnl list lengths not equal"
	cr
	-24 throw
    then			\ list1 list2

    list-header-store list-new	\ list1 list2 list3
    >r				\ list1 list2 R: list3

    list-get-first-link		\ list1 link2 R: list3
    swap			\ link2 list1 R: list3
    list-get-first-link		\ link2 link1 R: list3
    begin
    dup				\ link2 link1 link1 R: list3
    while			\ link2 link1 R: list3

	over			\ link2 link1 link2       R: list3
        link-get-data		\ link2 link1 data2       R: list3
	fpn-get-value		\ link2 link1       F: x  R: list3

	dup 			\ link2 link1 link1 F: x  R: list3
        link-get-data		\ link2 link1 data1       R: list3
	fpn-get-value		\ link2 link1       F: y x  R: list3
	
	f+			\ link1 link2       F: z  R: list3

	r@			\ link2 link1 list3 F: x R: list3
	fpn-list-push		\ link2 link1            R: list3

    	link-get-next		\ link2 link1+           R: list3
    	swap link-get-next	\ link1+ link2+          R: list3
    	swap			\ link2+ link1+          R: list3 (Keep order for consistency)
    repeat
    2drop			\ R: list3
    r>				\ list3
;

\  Add a fp-list to each fp-list in a list-of-lists
: fpnlol-plus-fpnl ( fpnl-addr fpnlol-addr -- fpnlol2-addr )

    list-header-store list-new	\ fpnl fpnlol list-ret
    >r				\ fpnl fpnlol R: fpnlol2

    list-get-first-link		\ fpnl link R: fpnlol2
    begin
    dup
    while
        swap			\ link fpnl      R: fpnlol2
        over link-get-data	\ link fpnl data R: fpnlol2
        over			\ link fpnl data fpnl R: fpnlol2
	fpnl-plus-fpnl		\ link fpnl fpnl+

	link-store link-new	\ link fpnl link+     R: fpnlol2

	r@			\ link fpnl link+ fpnlol2 R: fpnlol2
	list-push-link		\ link fpnl
	swap			\ fpnl link
	
    	link-get-next		\ fpnl link-next
    repeat
    
    2drop
    r>				\ list-ret
;

: fpn-list-deallocate ( list-addr -- )
    dup list-get-first-link		\ list-addr cur-link-addr
    begin
        dup				\ list-addr cur-link-addr cur-link-addr (flag, zero or not)
    while				\ list-addr cur-link-addr 
        dup link-get-data 		\ list-addr cur-link-addr num-addr
	fpn-store mma-deallocate	\ list-addr cur-link-addr
        dup link-get-next		\ list-addr cur-link-addr next-link-addr
        swap				\ list-addr next-link-addr cur-link-addr
	link-store mma-deallocate	\ list-addr next-link-addr
    repeat
    drop				\ list-addr
    list-header-store mma-deallocate	\ -- )
;

: list-of-lists-fpn-deallocate ( list-addr -- )
    dup list-get-first-link	\ list-addr cur-link-addr
    begin
        dup				\ list-addr cur-link-addr cur-link-addr (flag, zero or not)
    while				\ list-addr cur-link-addr 
        dup link-get-data 		\ list-addr cur-link-addr num-addr
	fpn-list-deallocate		\ list-addr cur-link-addr
        dup link-get-next		\ list-addr cur-link-addr next-link-addr
        swap				\ list-addr next-link-addr cur-link-addr
	link-store mma-deallocate	\ list-addr next-link-addr
    repeat
    drop				\ list-addr
    list-header-store mma-deallocate	\ -- )
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


\ Do floating point list things
cr
list-header-store list-new value list-fp-1	\ Get linked list header for a new floating point list
2.1e list-fp-1 fpn-list-add
3.2e list-fp-1 fpn-list-add
4.6e list-fp-1 fpn-list-add
." list-fp-1: "
list-fp-1 .fpn-list
cr
cr

3.5e list-fp-1 fpn-list-multiply value list-fp-2
." list-fp-2: "
list-fp-2 .fpn-list
3 spaces ." (list-fp-1 * 3.5)"
cr
cr
1.5e list-fp-1 fpn-list-plus value list-fp-3
." list-fp-3: "
list-fp-3 .fpn-list
3 spaces ." (list-fp-1 + 1.5)"
cr

memory-use

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

list-fp-1 fpn-list-deallocate
list-fp-2 fpn-list-deallocate
list-fp-3 fpn-list-deallocate

memory-use

\  Do list-of-lists things
list-header-store list-new value list-of-lists-fp-1	\ Get linked list header for a new lists-of-lists

list-header-store list-new				\ fpn-list   Start a fpn list
dup 5.1e fpn-list-add
dup 4.1e fpn-list-add					\ fpn-list
link-store link-new					\ link
list-of-lists-fp-1 list-add-link			\ 

list-header-store list-new				\ fpn-list   Start a fpn list
dup 2.1e fpn-list-add
dup 1.1e fpn-list-add					\ fpn-list
link-store link-new					\ link
list-of-lists-fp-1 list-add-link			\ 

cr
." list-of-lists-fp-1: "
list-of-lists-fp-1 .list-of-lists-fpn
3 spaces ." (Lets say these are Cartesian coordinates)"
cr

list-header-store list-new value list-fp-4		\ Get linked list header for a new lists-of-lists

list-fp-4						\ fpn-list   Start a fpn list
dup 3.1e fpn-list-add
6.7e fpn-list-add					\ fpn-list

list-fp-4 list-of-lists-fp-1 fpnlol-plus-fpnl value list-of-lists-fp-2

cr
." list-of-lists-fp-2: "
list-of-lists-fp-2 .list-of-lists-fpn
3 spaces ." (Euclidean Translation using "
list-fp-4 .fpn-list
." )"
cr

memory-use

cr
." Deallocating  ..."
cr

list-of-lists-fp-1 list-of-lists-fpn-deallocate
list-fp-4 fpn-list-deallocate
list-of-lists-fp-2 list-of-lists-fpn-deallocate

memory-use

cr
." dstack end:" space .s cr
." fstack end:" space f.s cr
