
\ Constants.
 #4 constant num-bits
#15 constant all-bits
 #8 constant ms-bit

0 value structinfo-list-store

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
include structinfo.fs
include structinfolist.fs
include stackprint2.fs
include name.fs
cs

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init

\ Init structinfo list.
list-new to structinfo-list-store
' link-deallocate ' .link s" Link" link-mma link-id structinfo-new structinfo-list-store structinfo-list-push
' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-id structinfo-new structinfo-list-store structinfo-list-push-end
' structinfo-deallocate ' .structinfo s" Struct-info" structinfo-mma structinfo-id structinfo-new structinfo-list-store structinfo-list-push-end

#103 region-mma-init
' region-deallocate ' .region s" Region" region-mma region-id structinfo-new structinfo-list-store structinfo-list-push-end

#50 name-mma-init       \ Initialize name struct array.  name-mma is set.
' name-deallocate ' .name s" Name" name-mma name-id structinfo-new structinfo-list-store structinfo-list-push-end

list-new                \ lst0
list-new                \ lst0 lst1

#5 #6 region-new        \ lst0 lst1 region
over list-push-struct   \ lst0 lst1

s" Mary" name-new       \ lst0 lst1 name
over list-push-struct   \ lst0 lst1

over list-push-struct   \ lst0

list-new                \ lst0 lst1

#3 #7 region-new        \ lst0 lst1 region
over list-push-struct   \ lst0 lst1

s" Dan" name-new        \ lst0 lst1 name
over list-push-struct   \ lst0 lst1

over list-push-struct   \ lst0

s" Jerry" name-new      \ lst0 lst1 name
over list-push-struct   \ lst0 lst1

#5
over list-push          \ lst0 lst1

cr cr ." list: "
dup structinfo-list-print-struct-list
cr

\ Finish.
cr structinfo-list-store structinfo-list-print-memory-use cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."

\ project items deallocate
structinfo-list-deallocate-struct-list

cr structinfo-list-store structinfo-list-print-memory-use cr

structinfo-list-store structinfo-list-project-deallocated

\ Free heap memory before exiting.
." Freeing heap memory"
structinfo-list-store structinfo-list-free-heap
cr
