
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

0 value structinfo-list-store

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init

\ Init structinfo list.
list-new to structinfo-list-store
s" Link" link-mma link-id structinfo-new structinfo-list-store list-push-struct
s" List" list-mma list-id structinfo-new structinfo-list-store list-push-end-struct
s" Struct-info" structinfo-mma structinfo-id structinfo-new structinfo-list-store list-push-end-struct

#103 region-mma-init
s" Region" region-mma region-id structinfo-new structinfo-list-store list-push-end-struct

#5 #6 region-new

\ Finish.
cr structinfo-list-store structinfo-list-print-memory-use cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."

\ project items deallocate
region-deallocate

cr structinfo-list-store structinfo-list-print-memory-use cr

structinfo-list-store structinfo-list-project-deallocated

\ Free heap memory before exiting.
." Freeing heap memory"
structinfo-list-store structinfo-list-free-heap
cr
