\ Print structs on the stack.
\ Non structs are printed as a decimal number.
\ List length is shown, like "-5".
\ An unallocated indicator is given, "-u".

\ Constants.
 #4  constant num-bits
#15 constant all-bits
 #8  constant ms-bit

include tools2.fs
include mm_array.fs
include link.fs
include list.fs
include region.fs
include stackprint.fs
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

23
2 link-new
list-new
2 3 region-new
4 5 region-new
dup region-deallocate

cr cr ." structs on stack: " .stack

\ Finish.
cr
memory-use
cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."
drop
region-deallocate
list-deallocate
link-deallocate

cr memory-use cr

\ Free heap memory before exiting.
cr ." Freeing heap memory"
list-mma mma-free
link-mma mma-free
region-mma mma-free
cr

