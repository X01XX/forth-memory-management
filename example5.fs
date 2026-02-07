
\ Constants.
 #4 constant num-bits
#15 constant all-bits
 #8 constant ms-bit

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
include stackprint.fs
include structinfo.fs
include structinfolist.fs
cs

: memory-use ( -- )
    cr ." Memory use:"
    cr #4 spaces ." Region mma:       " region-mma .mma-usage
    cr #4 spaces ." Struct-info mma:  " struct-info-mma .mma-usage
    cr #4 spaces ." List mma:         " list-mma .mma-usage
    cr #4 spaces ." Link mma:         " link-mma .mma-usage
    cr #4 spaces ." dstack: "
    base @ >r decimal .s r> base ! 
;

0 value struct-info-list-store

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 struct-info-mma-init

\ Init struct-info list.
list-new to struct-info-list-store
s" Link" link-mma link-id struct-info-new struct-info-list-store list-push-struct
s" List" list-mma list-id struct-info-new struct-info-list-store list-push-end-struct
s" Struct-info" struct-info-mma struct-info-id struct-info-new struct-info-list-store list-push-end-struct

#103 region-mma-init
s" Region" region-mma region-id struct-info-new struct-info-list-store list-push-end-struct

#5 #6 region-new

\ Finish.
cr struct-info-list-store struct-info-list-print-memory-use cr
\ memory-use

\ Deallocate remaining struct instances.
cr ." Deallocating ..."

\ project items deallocate
region-deallocate

cr struct-info-list-store struct-info-list-print-memory-use cr
\ cr memory-use cr

struct-info-list-store struct-info-project-all-deallocated
struct-info-list-store struct-info-list-deallocate
assert-stack-empty

\ Free heap memory before exiting.
cr ." Freeing heap memory"
list-mma mma-free
link-mma mma-free
struct-info-mma mma-free
region-mma mma-free
cr
