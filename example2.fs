
\ Constants.
 #4  constant num-bits
#15 constant all-bits
 #8  constant ms-bit

include tools.fs
include tools2.fs
include struct.fs
include mm_array.fs
include link.fs
include list.fs
include structlist.fs
include region.fs
include regionlist.fs
include state.fs
cs

: memory-use ( -- )
    cr ." Memory use:"
    cr #4 spaces ." Region mma:       " region-mma .mma-usage
    cr #4 spaces ." List mma:         " list-mma .mma-usage
    cr #4 spaces ." Link mma:         " link-mma .mma-usage
    cr #4 spaces ." dstack: "
    base @ >r decimal .s r> base ! 
;

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#103 region-mma-init

\ Get ~4 + ~5
cr
#4 #5 state-not-a-or-not-b    \ list

cr ." ~4 + ~5: " dup .region-list cr

\ Get ~3 + ~6
#3 #6 state-not-a-or-not-b    \ list45 list36

cr ." ~3 + ~6: " dup .region-list cr

\ Get intersection of lists.
2dup region-list-region-intersections   \ list45 list36 ints

dup cr ." Possible regions = (~4 + ~5) & (~3 + ~6) = " .region-list

\ Finish.
cr
memory-use
cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."
region-list-deallocate
region-list-deallocate
region-list-deallocate

cr memory-use cr

assert-list-mma-none-in-use
assert-link-mma-none-in-use
assert-region-mma-none-in-use
assert-forth-stack-empty

\ Free heap memory before exiting.
cr ." Freeing heap memory"
list-mma mma-free
link-mma mma-free
region-mma mma-free
cr
